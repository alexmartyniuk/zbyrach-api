using Zbyrach.Api.Account;
using Zbyrach.Api.Articles;
using Zbyrach.Api.Mailing;
using Zbyrach.Api.Migrations;
using Zbyrach.Api.Tags;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

            services.AddAuthentication("TokenAuthentication")
                .AddScheme<AuthenticationSchemeOptions, AuthenticationHandler>("TokenAuthentication", null);
            services.AddAuthorization();

            services.AddHostedService<ArticlesSearcher>();
            services.AddHostedService<NotificationsSender>();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Zbyrach API",
                    Version = "v1"
                });
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<PdfService>();
            services.AddSingleton<FileService>();
            services.AddSingleton<MailService>();
            services.AddSingleton<MediumTagsService>();
            services.AddSingleton<CronService>();

            services.AddDbContext<ApplicationContext>(options =>
            {
                // TODO: temporaty solution, need to be replaced with SQL database
                options.UseSqlite($"Data Source={Configuration["Database"]}");
            });

            services.AddScoped<UsersService>();
            services.AddScoped<TokenService>();
            services.AddScoped<AccountService>();            
            services.AddScoped<TagService>();            
            services.AddScoped<MailingSettingsService>();
            services.AddScoped<ArticleService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                loggerFactory.AddFile("Logs/{Date}.txt");
            }

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
