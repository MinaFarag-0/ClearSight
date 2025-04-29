using ClearSight.Infrastructure;
using Microsoft.OpenApi.Models;
using Serilog;


var builder = WebApplication.CreateBuilder(args);


#region Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    //options.IncludeXmlComments(xmlPath);
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
#endregion

#region SeriLog
Log.Logger = new LoggerConfiguration()
.ReadFrom.Configuration(builder.Configuration)
.Enrich.FromLogContext()
.CreateLogger();
builder.Host.UseSerilog();
#endregion

builder.Services.AddInfrastructureDependencies(builder.Configuration);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
#region SeedingRoles
//using var scope = app.Services.CreateScope();
//await SeedingRoles.Initialize(scope.ServiceProvider);
#endregion

app.UseSerilogRequestLogging();
app.MapControllers();
app.Configurations();
app.Run();