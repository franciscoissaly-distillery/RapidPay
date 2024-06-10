using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RapidPay.Api.Auth;
using RapidPay.Api.Filters;
using RapidPay.Auth.Mocks;
using RapidPay.DataAccess.Mocks;
using RapidPay.DataAccess.Sql;
using RapidPay.Domain.Adapters;
using RapidPay.Domain.Repository;
using RapidPay.Domain.Services;
using RapidPay.Fees.Mocks;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CardsManagementDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("RapidPay"), options =>
    {
        var asmName = Assembly.GetExecutingAssembly().FullName;
        options.MigrationsAssembly(asmName);
    });
    options.EnableSensitiveDataLogging(); // Enable parameter value logging
    options.LogTo(Console.WriteLine, LogLevel.Information); // Log SQL to the console
});

// Add services to the container.
builder.Services
    .AddSingleton<DefaultEntities>()
    //.AddSingleton<ICardsManagementRepository, CardsManagementInMemoryRepository>()
    .AddScoped<ICardsManagementRepository, CardsManagementSqlRepository>()
    .AddSingleton<IPaymentFeesAdapter>(RandomPaymentFeesManager.Instance)
    .AddSingleton<IUsersAdapter>(new TestUsersManager())
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
