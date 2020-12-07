using System.Text.Json;
using DinkToPdf;
using Pdf.Models;

namespace Pdf.Services
{
    public static class DinkToPdfModelService
    {
        public static string GetBody(string html, string headerHtml, string footerHtml)
        {
            var model = new DinkToPdfModel
            {
                Html = html,
                FooterHtml = footerHtml,
                HeaderHtml = headerHtml,
                GlobalSettings = GetSettings()
            };

            return JsonSerializer.Serialize(model);
        }

        private static GlobalSettings GetSettings()
        {
            return new GlobalSettings
            {
                PaperSize = PaperKind.A4,
                Orientation = Orientation.Portrait,
                Margins = new MarginSettings
                {
                    Unit = Unit.Millimeters,
                    Top = 25F,
                    Right = 0F,
                    Bottom = 25F,
                    Left = 0F
                }
            };
        }
    }
}