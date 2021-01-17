using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Reflection;
using Zbyrach.Api.Account;
using Zbyrach.Api.Admin.Services;
using Zbyrach.Api.Articles;
using Zbyrach.Api.Common;
using Zbyrach.Api.Mailing;
using Zbyrach.Api.Tags;

namespace Zbyrach.Api.Tests.Common
{
    public abstract class BaseServiceTests : BaseDatabaseTests
    {
        private IServiceProvider _serviceProvider;

        protected readonly Mock<MailService> _mailService = new Mock<MailService>(MockBehavior.Strict);
        protected readonly Mock<DateTimeService> _dateTimeService = new Mock<DateTimeService>(MockBehavior.Strict);
        protected readonly Mock<MediumTagsService> _mediumTagService = new Mock<MediumTagsService>(MockBehavior.Strict);
        protected readonly Mock<PdfService> _pdfService = new Mock<PdfService>(MockBehavior.Strict, null, null);
        protected readonly Mock<GoogleAuthService> _googleAuthService = new Mock<GoogleAuthService>(MockBehavior.Strict, null);

        public BaseServiceTests()
        {
            var services = new ServiceCollection();
            AddApplicationServices(services);
            SetupBaseMocks();
        }

        private void SetupBaseMocks()
        {
        }

        protected void AddApplicationServices(IServiceCollection services)
        {
            // external services
            services.AddLogging();
            services.AddScoped(sp => Context);
            services.AddScoped(sp => _mailService.Object);
            services.AddScoped(sp => _dateTimeService.Object);
            services.AddScoped(sp => _mediumTagService.Object);
            services.AddScoped(sp => _pdfService.Object);
            services.AddScoped(sp => _googleAuthService.Object);

            services.AddDetection();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            
            services.AddSingleton<CronService>();
            services.AddSingleton<TranslationService>();
            services.AddScoped<AdminService>();                  
            services.AddScoped<UsersService>();
            services.AddScoped<AccessTokenService>();
            services.AddScoped<TagService>();
            services.AddScoped<MailingSettingsService>();
            services.AddScoped<ArticleService>();

            services.AddMediatR(Assembly.GetAssembly(typeof(Startup)));

            _serviceProvider = services.BuildServiceProvider();
        }

        protected T GetSut<T>()
        {
            return (T)_serviceProvider.GetService(typeof(T));
        }
    }
}
