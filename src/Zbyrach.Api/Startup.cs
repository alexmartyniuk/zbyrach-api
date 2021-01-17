using System.Reflection;
using MediatR;
using Zbyrach.Api.Account;
using Zbyrach.Api.Articles;
using Zbyrach.Api.Mailing;
using Zbyrach.Api.Migrations;
using Zbyrach.Api.Tags;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zbyrach.Api.Admin.Services;
using Zbyrach.Api.Common;
using Hellang.Middleware.ProblemDetails;
using System;

namespace Zbyrach.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddProblemDetails(options =>
            {
                // Map exeptions to HTTP status codes here
                options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);                
                options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
            });

            services.AddAuthentication("TokenAuthentication")
                .AddScheme<AuthenticationSchemeOptions, AuthenticationHandler>("TokenAuthentication", null);
            services.AddAuthorization();

            services.AddHttpClient();
            services.AddHttpClient<PdfService>();
            services.AddHttpClient<GoogleAuthService>();

            services.AddHostedService<ArticlesSearcher>();
            services.AddHostedService<NotificationsSender>();
            services.AddControllers(options => {
                options.Filters.Add(typeof(ModelStateValidatorAttribute));            
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Zbyrach API",
                    Version = "v1"
                });
            });

            services.AddDetection();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<DateTimeService>();
            services.AddSingleton<MailService>();
            services.AddSingleton<MediumTagsService>();
            services.AddSingleton<CronService>();
            services.AddSingleton<TranslationService>();

            services.AddScoped<AdminService>();

            services.AddDbContext<ApplicationContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString());
            });

            services.AddScoped<UsersService>();
            services.AddScoped<AccessTokenService>();
            services.AddScoped<TagService>();
            services.AddScoped<MailingSettingsService>();
            services.AddScoped<ArticleService>();

            services.AddMediatR(Assembly.GetExecutingAssembly());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseProblemDetails();

            app.UseDetection();

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Zbyrach API");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
