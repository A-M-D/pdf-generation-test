using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pdf.Models;
using PuppeteerSharp;

namespace Pdf.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PuppeteerController : ControllerBase
    {
        private readonly ILogger<PuppeteerController> _logger;

        public PuppeteerController(ILogger<PuppeteerController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post(PuppeteerPdfModel model)
        {
            var launchOptions = GetLaunchOptions();

            using (var browser = await Puppeteer.LaunchAsync(launchOptions))
            using (var page = await browser.NewPageAsync())
            {
                await page.SetContentAsync(model.Html);
                var result = await page.GetContentAsync();
                var data = await page.PdfDataAsync(model.PdfOptions);

                return new FileContentResult(data, "application/pdf");
            }
        }

        private static LaunchOptions GetLaunchOptions()
        {
            return new LaunchOptions
            {
                Headless = true,
                Args = new[]
                {
                    "--no-sandbox"
                }
            };
        }
    }
}
