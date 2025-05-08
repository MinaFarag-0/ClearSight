using ClearSight.Core.Dtos.ApiResponse;
using ClearSight.Core.Helpers;
using ClearSight.Core.Interfaces;
using ClearSight.Core.Interfaces.Repository;
using ClearSight.Core.Interfaces.Services;
using ClearSight.Core.Mosels;
using ClearSight.Infrastructure.Context;
using ClearSight.Infrastructure.Implementations.Middelwares;
using ClearSight.Infrastructure.Implementations.Repositories;
using ClearSight.Infrastructure.Implementations.Services;
using ClearSight.Infrastructure.Implementations.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();


builder.Host.UseSerilog();

builder.Services.AddHttpClient();
builder.Services.AddHttpClient<MLModelService>();
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddControllers()
    .AddXmlSerializerFormatters()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToArray();

            //var response = new ModelStateErrorResponse
            //{
            //    StatusCode = 400,
            //    Errors = errors
            //};
            var response = ApiResponse<string>.FailureResponse(string.Join(',', errors));

            return new BadRequestObjectResult(response);
        };
    });

builder.Services.AddOpenApi();
builder.Services.AddTransient<AuthenticationService>();
builder.Services.AddTransient<MailingService>();
builder.Services.AddSingleton<CloudinaryService>();
builder.Services.AddScoped<ActivateUserAccountsServices>();
builder.Services.AddScoped<GenerateCodeServices>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IDoctorReposatory, DoctorReposatory>();
builder.Services.AddScoped<IPatientReposatory, PatientReposatory>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});


builder.Services.AddDbContext<AppDbContext>(op => op.UseSqlServer
(builder.Configuration.GetConnectionString("defaultconnection")));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

#region RateLimiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 10;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueLimit = 0;
    });
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Request.Headers.Host.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 15,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            });
    });
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";

        //var response = new ApiErrorResponse
        //{
        //    StatusCode = 429,
        //    err_message = "Too many requests. Please try again later."
        //};
        var response = ApiResponse<string>.FailureResponse("Too many requests. Please try again later.");

        await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(response), cancellationToken);
    };
});
#endregion

#region Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(o =>
    {
        o.SaveToken = false;
        o.RequireHttpsMetadata = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
            ClockSkew = TimeSpan.Zero,
        };
        o.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                if (!context.Response.HasStarted)
                {
                    //var response = new ApiErrorResponse
                    //{
                    //    StatusCode = 401,
                    //    err_message = "Unauthorized: Please provide a valid token."
                    //};
                    var response = ApiResponse<string>.FailureResponse("Unauthorized: Please provide a valid token.");

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(JsonSerializer.Serialize(response));
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                var userId = context.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                var securityStamp = context.Principal.FindFirstValue("SecurityStamp");

                if (userId != null && securityStamp != null)
                {
                    var user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        var currentSecurityStamp = await userManager.GetSecurityStampAsync(user);
                        if (currentSecurityStamp != securityStamp)
                        {
                            context.Fail("Token expired. Please log in again.");
                        }
                    }
                }
            },
            OnForbidden = context =>
            {
                //var response = new ApiErrorResponse
                //{
                //    StatusCode = 403,
                //    err_message = "You do not have permission to access this resource."
                //};
                var response = ApiResponse<string>.FailureResponse("You do not have permission to access this resource.");

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        };

    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    })
    .AddCookie()
;

#endregion

#region Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
    });
});
#endregion/


var app = builder.Build();

#region SeedingRoles
using var scope = app.Services.CreateScope();
await SeedingRoles.Initialize(scope.ServiceProvider);
#endregion

app.UseSwagger();
app.UseSwaggerUI();


app.UseSerilogRequestLogging();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseRouting();
app.UseStatusCodePages();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseRateLimiter();
app.MapControllers();
app.Run();