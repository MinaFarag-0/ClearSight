using ClearSight.Core.Dtos.ApiResponse;
using ClearSight.Core.Helpers;
using ClearSight.Core.Interfaces;
using ClearSight.Core.Interfaces.Services;
using ClearSight.Core.Mosels;
using ClearSight.Infrastructure.Context;
using ClearSight.Infrastructure.Implementations.Middlewares;
using ClearSight.Infrastructure.Implementations.Services;
using ClearSight.Infrastructure.Implementations.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace ClearSight.Infrastructure
{
    public static class ModuleInfrastructureDependencies
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services, IConfigurationManager configuration)
        {
            #region LifeScope

            services.AddHttpClient();
            services.AddHttpClient<MLModelService>();
            services.AddAutoMapper(typeof(MappingProfile));

            services.AddControllers()
                .AddXmlSerializerFormatters()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var errors = context.ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList();

                        var response = new ModelStateErrorResponse
                        {
                            StatusCode = 400,
                            Errors = errors
                        };

                        return new BadRequestObjectResult(response);
                    };
                });

            services.AddOpenApi();
            services.AddTransient<AuthenticationService>();
            services.AddTransient<MailingService>();
            services.AddSingleton<CloudinaryService>();
            services.AddScoped<ActivateUserAccountsServices>();
            services.AddScoped<GenerateCodeServices>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IDoctorService, DoctorService>();
            #endregion

            #region Configuration Section

            services.Configure<JWT>(configuration.GetSection("JWT"));
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.Configure<CloudinarySettings>(configuration.GetSection("Cloudinary"));
            #endregion



            #region Context
            services.AddDbContext<AppDbContext>(op => op.UseSqlServer
                (configuration.GetConnectionString("defaultconnection")));
            #endregion

            #region Identity
            services.AddIdentity<User, IdentityRole>(options =>
                {
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                    options.Lockout.MaxFailedAccessAttempts = 3;
                    options.Lockout.AllowedForNewUsers = true;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            #endregion

            #region RateLimiting
            services.AddRateLimiter(options =>
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
                    string? userId = context.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier) == null ? "Anonymous"
                    : context.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);

                    // Get the IP address
                    string ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";

                    // Log the rate-limited request
                    Log.Warning($"Rate limit exceeded. User: {userId}, IP: {ipAddress}, Path: {context.HttpContext.Request.Path}");
                    var response = new ApiErrorResponse
                    {
                        StatusCode = 429,
                        err_message = "Too many requests. Please try again later."
                    };

                    await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(response), cancellationToken);
                };
            });
            #endregion

            #region Authentication
            services.AddAuthentication(options =>
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
                        ValidIssuer = configuration["JWT:Issuer"],
                        ValidAudience = configuration["JWT:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"])),
                        ClockSkew = TimeSpan.Zero,
                    };
                    o.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            if (!context.Response.HasStarted)
                            {
                                var response = new ApiErrorResponse
                                {
                                    StatusCode = 401,
                                    err_message = "Unauthorized: Please provide a valid token."
                                };
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
                            var response = new ApiErrorResponse
                            {
                                StatusCode = 403,
                                err_message = "You do not have permission to access this resource."
                            };
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
                        }
                    };

                })
                .AddGoogle(options =>
                {
                    options.ClientId = configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                })
                .AddCookie()
            ;

            #endregion


            return services;
        }

        public static IApplicationBuilder Configurations(this IApplicationBuilder app)
        {

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseRouting();
            app.UseStatusCodePages();
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            app.UseRateLimiter();

            return app;
        }
    }
}
