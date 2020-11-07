using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pdf.Models;
using PuppeteerSharp;

namespace Pdf.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PdfController : ControllerBase
    {
        private readonly ILogger<PdfController> _logger;

        public PdfController(ILogger<PdfController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string url)
        {
            url = url ?? "https://google.com";
            var browser = await GetBrowser();

            var page = await browser.NewPageAsync();
            await page.GoToAsync(url);
            var pdf = File(await page.PdfDataAsync(), "application/pdf");
            return pdf;
        }

        [HttpPost]
        public async Task<IActionResult> Post(PdfModel model)
        {
            var browser = await GetBrowser();

            using (var page = await browser.NewPageAsync())
            {
                await page.SetContentAsync(model.Html);
                var result = await page.GetContentAsync();
                var pdf = File(await page.PdfDataAsync(), "application/pdf");
                return pdf;
            }
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            var browser = await GetBrowser();
            var sb = new StringBuilder();

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                {
                    sb.AppendLine(await reader.ReadLineAsync());
                }
            }

            using (var page = await browser.NewPageAsync())
            {
                await page.SetContentAsync(sb.ToString());
                var result = await page.GetContentAsync();
                var pdf = File(await page.PdfDataAsync(), "application/pdf");
                return pdf;
            }
        }

        private async Task<Browser> GetBrowser()
        {
            var executablePath = Environment.GetEnvironmentVariable("PUPPETEER_EXECUTABLE_PATH");

            // No custom path specified, download required
            if (string.IsNullOrEmpty(executablePath))
            {
                await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            }

            return await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[]
                {
                    "--no-sandbox"
                }
            });
        }
    }
}
