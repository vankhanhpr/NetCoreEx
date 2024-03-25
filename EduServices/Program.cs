using AuthServer.service.impl;
using AuthService.services.auth;
using FuBonServices.data;
using FuBonServices.services.user.impl;
using FuBonServices.services.user;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ModelClassLibrary.imlp;
using ModelClassLibrary.interfaces;
using Microsoft.IdentityModel.Tokens;
using ModelClassLibrary.area.auth;
using System.Text;
using EduServices.services.test;
using EduServices.services.test.impl;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.

builder.Services.AddControllers();

//Connection Db
builder.Services.AddDbContext<DataContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

//DI
builder.Services.AddTransient<IHashPass, HashPass>();
builder.Services.AddTransient<IAuth, AuthImpl>();
builder.Services.AddTransient<IUser, UserImpl>();
builder.Services.AddTransient<ITest, TestImpl>();

//authen
builder.Services.Configure<Audience>(configuration.GetSection("Audience"));
var audienceConfig = configuration.GetSection("Audience");
var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(audienceConfig["Secret"]));
var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = signingKey,
    ValidateIssuer = true,
    ValidIssuer = audienceConfig["Iss"],
    ValidateAudience = true,
    ValidAudience = audienceConfig["Aud"],
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero,
    RequireExpirationTime = true,
};
// Adding Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "TestKey";
})
 .AddJwtBearer("TestKey", x =>
 {
     x.RequireHttpsMetadata = false;
     x.TokenValidationParameters = tokenValidationParameters;
 });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
