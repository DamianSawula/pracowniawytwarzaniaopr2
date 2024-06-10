
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using WeatherAPI.Context;
using WeatherAPI.Interfaces;
using WeatherAPI.Middleware;
using WeatherAPI.Repository;

namespace WeatherAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
            builder.Services.AddScoped<DapperContext>();
            builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Weather API", Version = "v1" });

                // Dodaj konfiguracjê dla ApiKey Authorization
                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "ApiKey",
                    Type = SecuritySchemeType.ApiKey,
                    Description = "Type your API key below."
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                   {
                       {
                           new OpenApiSecurityScheme
                           {
                               Reference = new OpenApiReference
                               {
                                   Type = ReferenceType.SecurityScheme,
                                   Id = "ApiKey"
                               }
                           },
                           new string[] {}
                       }
                   });
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
            {
                builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
            }));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            var apiKey = builder.Configuration["ApiKey"];
            app.UseMiddleware<ApiKeyMiddleware>(Options.Create(new ApiKeyMiddlewareOptions { ApiKey = apiKey }));
            app.UseHttpsRedirection();
            app.UseCors("corsapp");
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
