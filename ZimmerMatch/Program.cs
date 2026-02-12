using ZimmerMatch.Interfaces;
using ZimmerMatch.Migrations;
using ZimmerMatch;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository.Entities;
using Repository.Repositories;
using Service.Interfaces;
using Service.Services;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "securityLessonWebApi", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Please enter your bearer token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                    new string[] {}
                }
            });
});
// אימות
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(option =>
    option.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    });
var connection = builder.Configuration.GetConnectionString("database-work");
//builder.Services.AddSingleton<IContext>(new ZimmerDbContext(connection));
builder.Services.AddDbContext<ZimmerDbContext>(options =>
    options.UseSqlServer(connection));
builder.Services.AddScoped<IContext>(provider => provider.GetRequiredService<ZimmerDbContext>());


builder.Services.AddAutoMapper(typeof(MapperProfile));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddServices();
builder.Services.AddOpenApi();

//builder.Services.AddScoped<IContext, ZimmerDbContext>();
var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    try
//    {
//        var context = scope.ServiceProvider.GetRequiredService<ZimmerDbContext>();
//        ZimmerSeederFull.Seed(context);
//        Console.WriteLine("Seeder executed successfully.");
//    }
//    catch (Exception ex)
//    {
//       Console.WriteLine("Seeder failed: " + ex.Message);
//    }
//}



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();// הרשאת גישה 
app.UseAuthorization(); // אימות 

app.MapControllers();

app.Run();


