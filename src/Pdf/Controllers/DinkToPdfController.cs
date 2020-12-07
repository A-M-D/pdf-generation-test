using System;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pdf.Models;
using Pdf.Services;

namespace Pdf.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DinkToPdfController : ControllerBase
    {
        private readonly ILogger<DinkToPdfController> _logger;
        private readonly IConverter _converter;
        private readonly FileCommands _fileCommands;

        public DinkToPdfController(ILogger<DinkToPdfController> logger, IConverter converter, FileCommands fileCommands)
        {
            _logger = logger;
            _converter = converter;
            _fileCommands = fileCommands;
        }

        [HttpPost]
        public async Task<IActionResult> Post(DinkToPdfModel model)
        {
            var headerFilePath = await _fileCommands.CreateTempFile(model.HeaderHtml, "html");
            var footerFilePath = await _fileCommands.CreateTempFile(model.FooterHtml, "html");

            var doc = new HtmlToPdfDocument
            {
                GlobalSettings = model.GlobalSettings,
                Objects =
                {
                    new ObjectSettings
                    {
                        HeaderSettings = new HeaderSettings { HtmUrl = headerFilePath },
                        HtmlContent = model.Html,
                        FooterSettings = new FooterSettings { HtmUrl = footerFilePath },
                    }
                }
            };

            return new FileContentResult(_converter.Convert(doc), "application/pdf");
        }
    }
}
