using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services;

namespace Backgrounding
{
    public class HtmlProcessingService
    {
        private readonly ILogger<HtmlProcessingService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _filepath;
        private FileSystemWatcher _htmlFileWatcher;

        public HtmlProcessingService(ILogger<HtmlProcessingService> logger, IHttpClientFactory httpClient, IOptions<HtmlWatcherOptions> options)
        {
            _logger = logger;
            _httpClient = httpClient.CreateClient();
            _filepath = options.Value.HtmlWatcherPath;
        }

        public async Task Process(CancellationToken cancellationToken = default(CancellationToken))
        {
            string content, header, footer;

            using (var sr = new StreamReader(Path.Combine(_filepath, "content.html")))
            {
                content = sr.ReadToEnd();
            }

            using (var sr = new StreamReader(Path.Combine(_filepath, "header.html")))
            {
                header = sr.ReadToEnd();
            }

            using (var sr = new StreamReader(Path.Combine(_filepath, "footer.html")))
            {
                footer = sr.ReadToEnd();
            }

            var model = PdfModelCreator.GetBody(content, header, footer);
            var json = new StringContent(model.ToString(), Encoding.UTF8, "application/json");

            var url = "http://localhost:5000/puppeteer";
            _logger.LogInformation($"Posting to {url}");

            var response = await _httpClient.PostAsync(url, json, cancellationToken);

            _logger.LogInformation(response.StatusCode.ToString());

            response.EnsureSuccessStatusCode();

            var fileInfo = new FileInfo(Path.Combine(_filepath, "PuppeteerPdf.pdf"));

            try
            {
                using (var ms = await response.Content.ReadAsStreamAsync(cancellationToken))
                using (var fs = File.Create(fileInfo.FullName))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.CopyTo(fs);
                }
            }
            catch (IOException ex)
            {
                _logger.LogError($"Could not open file {fileInfo.FullName}, {ex.Message}");
            }

            _logger.LogInformation($"Saved file {fileInfo.FullName}");
        }
    }
}