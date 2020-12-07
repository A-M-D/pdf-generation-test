using DinkToPdf;

namespace Pdf.Models
{
    public class DinkToPdfModel
    {
        public string Html { get; set; }

        public string HeaderHtml { get; set; }

        public string FooterHtml { get; set; }

        public GlobalSettings GlobalSettings { get; set; }
    }
}