using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PuppeteerSharp;

namespace Zbyrach.Api.Articles
{
    public class PdfService
    {
        private readonly string _chromiumExecutablePath;
        public PdfService(IConfiguration configuration)
        {
            _chromiumExecutablePath = configuration["PUPPETEER_EXECUTABLE_PATH"];
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
                ExecutablePath = _chromiumExecutablePath
            };

            using var browser = await Puppeteer.LaunchAsync(options);
            using var page = await browser.NewPageAsync();

            page.Console += async (sender, args) =>
            {
                switch (args.Message.Type)
                {
                    case ConsoleType.Error:
                        try
                        {
                            var errorArgs = await Task.WhenAll(args.Message.Args.Select(arg => arg.ExecutionContext.EvaluateFunctionAsync("(arg) => arg instanceof Error ? arg.message : arg", arg)));
                            System.Console.WriteLine($"{args.Message.Text} args: [{string.Join<object>(", ", errorArgs)}]");
                        }
                        catch { }
                        break;
                    case ConsoleType.Warning:
                        System.Console.WriteLine(args.Message.Text);
                        break;
                    default:
                        System.Console.WriteLine(args.Message.Text);
                        break;
                }
            };

            await page.SetJavaScriptEnabledAsync(false);
            await page.GoToAsync(url);

            var script = @"()=> {
                const links = document.querySelectorAll('a, button');
                for (let link of links) {
                    if (link.textContent.includes('Follow')) {
                        link.style.display = 'none';
                    }
                }

                var style = document.createElement('style');
                style.innerHTML = `
                    h1, h2 {
                        page-break-inside: avoid;
                    }
                    h1::after, h2::after {
                        content: '';
                        display: block;
                        height: 100px;
                        margin-bottom: -100px;
                    }
                    .paragraph-image {
                        page-break-inside: avoid;
                        page-break-before: auto;
                        page-break-after: auto;
                    }
                    `;
                document.head.appendChild(style);

                const article = document.querySelectorAll('article')[0];
                const parent = article.parentNode;
                parent.innerHTML = '';
                parent.append(article);
            }";

            await page.EvaluateFunctionAsync(script);

            return await page.PdfStreamAsync(new PdfOptions
            {
                MarginOptions = new PuppeteerSharp.Media.MarginOptions
                {
                    Top = "40px",
                    Bottom = "40px"
                }
            });
        }
    }
}