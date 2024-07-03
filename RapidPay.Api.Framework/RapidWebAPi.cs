using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RapidPay.Api.Filters;
using RapidPay.Api.Framework.Authentication;
using RapidPay.Domain.Repository;
using System.Text;

namespace RapidPay.Api.Framework
{
    public class RapidWebAPi
    {
        public void ConfigureAndRun(string[] args,
                Action<IHostApplicationBuilder>? builderConfiguringAction = null,
                Action<IHostApplicationBuilder>? builderConfiguredAction = null,
                Action<IApplicationBuilder>? appConfiguringAction = null,
                Action<IApplicationBuilder>? appConfiguredAction = null
                )
        {
            var builder = WebApplication.CreateBuilder(args);

            if (builderConfiguringAction is not null)
                builderConfiguringAction.Invoke(builder);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder => builder.AllowAnyOrigin()
                                      .AllowAnyMethod()
                                      .AllowAnyHeader());
            });


            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ExceptionsFilter>();
            });


            // Add JWT Authentication
            var jwtSettings = JwtSettings.ReadFromConfiguration(builder.Configuration);
            var symmetricKey = Encoding.ASCII.GetBytes(jwtSettings!.SecretKey);
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(symmetricKey),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                options.SaveToken = true;
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                // Add JWT auth UI
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please write 'Bearer ' + JWT token obtained from the api/Auth/login endpoint (the valid user is 'testuser', password 'testpassword')",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
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
                            }
                        },
                        new string[] { }
                    }
                });
            });


            builder.Services.AddHttpClient(Microsoft.Extensions.Options.Options.DefaultName, client =>
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            builder.Services.AddScoped<IAuthTokenProvider, AuthTokenHolder>();

            if (builderConfiguredAction is not null)
                builderConfiguredAction.Invoke(builder);


            WebApplication app = builder.Build();

            if (appConfiguringAction is not null)
                appConfiguringAction.Invoke(app);


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowAllOrigins");
            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseMiddleware<AuthTokenSavingMiddleware>();

            app.MapControllers();

            if (appConfiguredAction is not null)
                appConfiguredAction.Invoke(app);

            app.Run();
        }
    }
}
