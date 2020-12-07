using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Backgrounding.Services
{
    public class PuppeteerPdfRequester
    {
        private readonly ILogger<PuppeteerPdfRequester> _logger;
        private readonly HttpClient _httpClient;

        public PuppeteerPdfRequester(ILogger<PuppeteerPdfRequester> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<Stream> GetPdf(StringContent json, CancellationToken cancellationToken = default)
        {
            var url = "http://pdf:5000/puppeteer"; // TODO make variable
            _logger.LogInformation($"Posting to {url}");

            var response = await _httpClient.PostAsync(url, json, cancellationToken);

            _logger.LogInformation(response.StatusCode.ToString());

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }
    }
}