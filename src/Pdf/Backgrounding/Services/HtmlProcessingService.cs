using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Backgrounding.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services;

namespace Backgrounding.Services
{
    public class HtmlProcessingService
    {
        private readonly ILogger<HtmlProcessingService> _logger;
        private readonly PdfRequester _pdfRequester;
        private readonly HtmlReader _htmlReader;
        private readonly string _filepath;

        public HtmlProcessingService(ILogger<HtmlProcessingService> logger, HtmlReader htmlReader, PdfRequester pdfRequester, IOptions<HtmlWatcherOptions> options)
        {
            _logger = logger;
            _htmlReader = htmlReader;
            _pdfRequester = pdfRequester;
            _filepath = options.Value.HtmlWatcherPath;
        }

        public async Task Process(CancellationToken cancellationToken = default)
        {
            string content = await _htmlReader.Read(Path.Combine(_filepath, "content.html"));
            string header = await _htmlReader.Read(Path.Combine(_filepath, "header.html"));
            string footer = await _htmlReader.Read(Path.Combine(_filepath, "footer.html"));

            var model = PdfModelCreator.GetBody(content, header, footer);
            var jsonStringContent = new StringContent(model.ToString(), Encoding.UTF8, "application/json");

            var fileInfo = new FileInfo(Path.Combine(_filepath, "PuppeteerPdf.pdf"));
            try
            {
                using var ms = await _pdfRequester.GetPdf(jsonStringContent, cancellationToken);
                using var fs = File.Create(fileInfo.FullName);
                ms.Seek(0, SeekOrigin.Begin);
                ms.CopyTo(fs);
            }
            catch (IOException ex)
            {
                _logger.LogError($"Could not open file '{fileInfo.FullName}', {ex.Message}");
            }

            _logger.LogInformation($"Saved file {fileInfo.FullName}");
        }
    }
}