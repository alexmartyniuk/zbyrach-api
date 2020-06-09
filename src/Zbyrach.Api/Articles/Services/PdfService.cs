using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PuppeteerSharp;

namespace Zbyrach.Api.Articles
{
    public class PdfService
    {
        private readonly string _chromiumDownloadDirectory;
        public PdfService(IConfiguration configuration)
        {
            _chromiumDownloadDirectory = configuration["PUPPETEER_EXECUTABLE_PATH"];
        }
        public async Task<Stream> ConvertUrlToPdf(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            var options = new LaunchOptions
            {
                Headless = true,
                Args = new[]
                {
                    "--no-sandbox",
                    "--disable-plugins",
                    "--incognito",
                    "--disable-sync",
                    "--disable-gpu",
                    "--disable-speech-api",
                    "--disable-remote-fonts",
                    "--disable-shared-workers",
                    "--disable-webgl",
                    "--no-experiments",
                    "--no-first-run",
                    "--no-default-browser-check",
                    "--no-wifi",
                    "--no-pings",
                    "--no-service-autorun",
                    "--disable-databases",
                    "--disable-default-apps",
                    "--disable-demo-mode",
                    "--disable-notifications",
                    "--disable-permissions-api",
                    "--disable-background-networking",
                    "--disable-3d-apis",
                    "--disable-bundled-ppapi-flash",
                 },
               ExecutablePath = _chromiumDownloadDirectory
            };

            using var browser = await Puppeteer.LaunchAsync(options);
            using var page = await browser.NewPageAsync();

            await page.SetJavaScriptEnabledAsync(false);
            await page.GoToAsync(url);

            return await page.PdfStreamAsync();
        }
    }
}