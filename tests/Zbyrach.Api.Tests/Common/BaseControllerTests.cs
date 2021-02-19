using System;
using System.Net.Http;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zbyrach.Api.Account;
using Zbyrach.Api.Account.Exceptions;

namespace Zbyrach.Api.Tests.Common
{
    public abstract class BaseControllerTests : BaseServiceTests
    {
        protected HttpClient Client { get; private set; }

        public BaseControllerTests()
        {
            // Run for every test case
            SetupServer();
        }

        private void SetupServer()
        {
            var builder = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseEnvironment("Production");
                    webHost.UseTestServer();
                    webHost.ConfigureTestServices(services =>
                    {
                        services.AddProblemDetails(options =>
                        {
                            // Map exceptions to HTTP status codes here
                            options.MapToStatusCode<InvalidTokenException>(StatusCodes.Status401Unauthorized);

                            options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);
                            options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
                        });

                        AddApplicationServices(services);

                        services.AddCors();

                        services.AddAuthentication("TokenAuthentication")
                            .AddScheme<AuthenticationSchemeOptions, AuthenticationHandler>("TokenAuthentication", null);
                        services.AddAuthorization();
                        services.AddControllers(options =>
                        {
                            options.Filters.Add(typeof(ModelStateValidatorAttribute));
                        })
                        .AddApplicationPart(typeof(Program).Assembly);

                        services.Configure<KestrelServerOptions>(options =>
                        {
                            options.AllowSynchronousIO = true;
                        });
                    });

                    webHost.Configure(app =>
                    {
                        app.UseProblemDetails();
                        app.UseMiddleware<AuthenticatedTestRequestMiddleware>();
                        app.UseAuthentication();
                        app.UseRouting();
                        app.UseAuthorization();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                        });
                    });
                });

            var host = builder.Start();
            var testServer = host.GetTestServer();
            testServer.AllowSynchronousIO = true;
            Client = testServer.CreateClient();
        }
    }
}
