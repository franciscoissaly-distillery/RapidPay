using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RapidPay.Api.Auth;
using RapidPay.Api.Filters;
using RapidPay.Domain.Repository;
using RapidPay.Domain.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddSingleton<ICardsManagementRepository>(new CardsManagementInMemoryRepository())
    .AddSingleton<IPaymentFeesManager>(RandomPaymentFeesManager.Instance)
    .AddSingleton<IUsersManager>(new TestUsersManager())
    .AddTransient<ICardsManager, CardsManager>();


builder.Services.AddControllers(options =>
{
    options.Filters.Add<ExceptionsFilter>();
});

// Add basic auth
//builder.Services
//    .AddAuthentication("BasicAuthentication")
//    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

//var secretKey = (new KeyGenerator()).Generate256BitKey();// used to generate the 256 bits SecretKey configured in the appsettings.json file

// Add JWT auth
var jwtConfigSection = builder.Configuration.GetSection("Jwt");
var jwtSettings = jwtConfigSection.Get<JwtSettings>();
var symmetricKey = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);
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
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
    };
});

builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Add basic auth UI
    //options.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    //{
    //    Type = SecuritySchemeType.Http,
    //    Scheme = "basic",
    //    Description = "Input your username and password to access this API" + Environment.NewLine
    //    + "(valid username:'testuser', password:'testpassword')"
    //});

    //options.AddSecurityRequirement(new OpenApiSecurityRequirement
    //{
    //    {
    //        new OpenApiSecurityScheme
    //        {
    //            Reference = new OpenApiReference
    //            {
    //                Type = ReferenceType.SecurityScheme,
    //                Id = "basic"
    //            }
    //        },
    //        new string[] {}
    //    }
    //});

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
