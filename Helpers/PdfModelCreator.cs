using System.Text.Json;
using Pdf.Models;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace Services
{
    public static class PdfModelCreator
    {
        public static string GetBody(string html, string headerHtml, string footerHtml)
        {
            var model = new PdfModel
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
                HeaderTemplate = headerHtml,
                FooterTemplate = footerHtml,
                Format = PaperFormat.A4,
                MarginOptions = new MarginOptions
                {
                    Top = "25mm",
                    Right = "0mm",
                    Bottom = "25mm",
                    Left = "0mm"
                }
            };
        }
    }
}