using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Backgrounding.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pdf.Services;

namespace Backgrounding.Services
{
    public class HtmlProcessingService
    {
        private readonly ILogger<HtmlProcessingService> _logger;
        private readonly PuppeteerPdfRequester _puppeteerPdfRequester;
        private readonly DinkToPdfRequester _dinkToPdfRequester;
        private readonly HtmlReader _htmlReader;
        private readonly string _filepath;

        public HtmlProcessingService(ILogger<HtmlProcessingService> logger, HtmlReader htmlReader, PuppeteerPdfRequester puppeteerPdfRequester, DinkToPdfRequester dinkToPdfRequester, IOptions<HtmlWatcherOptions> options)
        {
            _logger = logger;
            _htmlReader = htmlReader;
            _puppeteerPdfRequester = puppeteerPdfRequester;
            _dinkToPdfRequester = dinkToPdfRequester;
            _filepath = options.Value.HtmlWatcherPath;
        }

        public async Task Process(CancellationToken cancellationToken = default)
        {
            var content = await _htmlReader.Read(Path.Combine(_filepath, "content.html"));
            var header = await _htmlReader.Read(Path.Combine(_filepath, "header.html"));
            var footer = await _htmlReader.Read(Path.Combine(_filepath, "footer.html"));

            await ProcessPuppeteer(content, header, footer, cancellationToken);
            await ProcessDinkToPdf(content, header, footer, cancellationToken);
        }

        public async Task ProcessPuppeteer(string content, string header, string footer, CancellationToken cancellationToken = default)
        {
            var model = PuppeteerPdfModelService.GetBody(content, header, footer);
            var jsonStringContent = new StringContent(model.ToString(), Encoding.UTF8, "application/json");

            var fileInfo = new FileInfo(Path.Combine(_filepath, "PuppeteerPdf.pdf"));

            try
            {
                using var ms = await _puppeteerPdfRequester.GetPdf(jsonStringContent, cancellationToken);
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

        public async Task ProcessDinkToPdf(string content, string header, string footer, CancellationToken cancellationToken = default)
        {
            var model = DinkToPdfModelService.GetBody(content, header, footer);
            var jsonStringContent = new StringContent(model.ToString(), Encoding.UTF8, "application/json");

            var fileInfo = new FileInfo(Path.Combine(_filepath, "DinkToPdf.pdf"));

            try
            {
                using var ms = await _dinkToPdfRequester.GetPdf(jsonStringContent, cancellationToken);
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