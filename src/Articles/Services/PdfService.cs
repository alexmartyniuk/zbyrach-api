using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PuppeteerSharp;

namespace MediumGrabber.Api.Articles
{
    public class PdfService
    {
        private readonly string _chromiumDownloadDirectory;
        public PdfService(IConfiguration configuration)
        {
            _chromiumDownloadDirectory = configuration["ChromiumDownloadDirectory"];   
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
                    // TODO: Running as root without --no-sandbox is not supported. See https://crbug.com/638180.
                    "--no-sandbox",
                    "--disable-plugins", "--incognito", "--disable-sync", "--disable-gpu", "--disable-speech-api",
                    "--disable-remote-fonts", "--disable-shared-workers", "--disable-webgl", "--no-experiments",
                    "--no-first-run", "--no-default-browser-check", "--no-wifi", "--no-pings", "--no-service-autorun",
                    "--disable-databases", "--disable-default-apps", "--disable-demo-mode", "--disable-notifications",
                    "--disable-permissions-api", "--disable-background-networking", "--disable-3d-apis",
                    "--disable-bundled-ppapi-flash",
                 },
                 // TODO: Change this to using _chromiumDownloadDirectory field
                 ExecutablePath = @"d:\Projects\Zbyrach\chromium\Win64-706915\chrome-win\chrome.exe",
            };

            var browserFetcher = Puppeteer.CreateBrowserFetcher(new BrowserFetcherOptions
            {
                Path = _chromiumDownloadDirectory
            });
            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);          

            using var browser = await Puppeteer.LaunchAsync(options);
            using var page = await browser.NewPageAsync();

            await page.SetJavaScriptEnabledAsync(false);
            await page.GoToAsync(url);

            return await page.PdfStreamAsync();
        }
    }
}