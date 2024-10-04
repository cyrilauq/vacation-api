using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using VacationApi.Auth;
using VacationApi.Domains;
using VacationApi.Infrastructure;
using VacationApi.Repository;
using VacationApi.Services;
using VacationApi.Utils;

namespace VacationApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Mettre un if pour le choix de la connection string
            var builderConf = new ConfigurationBuilder()
                            .SetBasePath(builder.Environment.ContentRootPath)
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                            .AddEnvironmentVariables();

            IConfiguration Configuration = builderConf.Build();

            var connectionString = Configuration.GetConnectionString("default");

            builder.Services.AddDbContext<VacationApiDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            // For Identity
            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<VacationApiDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddScoped<IVacationInfrastructure, VacationRepository>();
            builder.Services.AddScoped<IVacationActivitiesInfrastructure, ActivitesBDRepository>();
            builder.Services.AddScoped<IUserInfrastructure, BDUserRepository>();
            builder.Services.AddScoped<IVacationGetter, ApiVacationGetter>();
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddScoped<ActivitiesServices, ActivitiesServices>();
            builder.Services.AddScoped<VacationsServices, VacationsServices>();
            builder.Services.AddScoped<UsersServices, UsersServices>();
            builder.Services.AddScoped<FileService, FileService>();

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = Configuration[AuthConstants.IssuerPath],
                        ValidAudience = Configuration[AuthConstants.AudiencePath],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration[AuthConstants.SecretPath]!))
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddGoogle(options =>
                {
                    options.ClientId = SecretConfig.CLIENT_ID;
                    options.ClientSecret = SecretConfig.CLIENT_SECRET;
                });

            builder.Services.AddAuthorization();

            builder.Services.AddControllers(/*x => x.Filters.Add<ApiKeyAuthFilter>()*/);
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Version = "V1",
                    Title = "Vacations API",
                    Description = "API for the HELMo Vacations apps"
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            /**
             * The order matters because first come authentification and second comes the authorization.
             */
            app.UseAuthentication();
            app.UseAuthorization();

            // Seed database
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                DataInitializer.SeedRole(roleManager);
                DataInitializer.Seed(userManager);
            }

            app.UseCors();

            app.MapControllers();

            app.Run();
        }
    }
}