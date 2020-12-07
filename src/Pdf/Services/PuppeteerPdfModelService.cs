using System.Text.Json;
using Pdf.Models;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace Pdf.Services
{
    public static class PuppeteerPdfModelService
    {
        public static string GetBody(string html, string headerHtml, string footerHtml)
        {
            var model = new PuppeteerPdfModel
            {
                Html = html,
                PdfOptions = GetPdfOptions(headerHtml, footerHtml)
            };

            return JsonSerializer.Serialize(model);
        }

        private static PdfOptions GetPdfOptions(string headerHtml, string footerHtml)
        {
            return new PdfOptions
            {
                DisplayHeaderFooter = true,
                PrintBackground = true,
                HeaderTemplate = headerHtml,
                FooterTemplate = footerHtml,
                Format = PaperFormat.A4,
                MarginOptions = new MarginOptions
                {
                    Top = "20mm",
                    Right = "0mm",
                    Bottom = "20mm",
                    Left = "0mm"
                }
            };
        }
    }
}