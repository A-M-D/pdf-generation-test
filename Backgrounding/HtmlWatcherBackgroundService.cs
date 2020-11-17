using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services;

namespace Backgrounding
{
    public class HtmlWatcherBackgroundService : BackgroundService
    {
        private readonly ILogger<HtmlWatcherBackgroundService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _filepath;
        private FileSystemWatcher _htmlFileWatcher;

        public HtmlWatcherBackgroundService(ILogger<HtmlWatcherBackgroundService> logger, IHttpClientFactory httpClient, IOptions<HtmlWatcherOptions> options)
        {
            _logger = logger;
            _httpClient = httpClient.CreateClient();
            _filepath = options.Value.HtmlWatcherPath;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.CompletedTask;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("HtmlWatcherBackgroundService starting");

            if (!Directory.Exists(_filepath))
            {
                _logger.LogWarning($"Please ensure sure the folder [{_filepath}] exists, then restart the service.");
                return Task.CompletedTask;
            }

            _logger.LogInformation($"Using folder: {_filepath}");

            _htmlFileWatcher = new FileSystemWatcher(_filepath, "*.html")
            {
                NotifyFilter = NotifyFilters.Size
            };

            _htmlFileWatcher.Changed += (s, e) => OnChanged(s, e, cancellationToken);
            _htmlFileWatcher.Error += (s, e) => OnError(s, e, cancellationToken);
            _htmlFileWatcher.EnableRaisingEvents = true;

            return base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping HtmlWatcherBackgroundService");
            _htmlFileWatcher.EnableRaisingEvents = false;
            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _htmlFileWatcher?.Dispose();
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        private void OnError(object sender, ErrorEventArgs e, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"File Watcher error: {e.GetException().Message}");
        }

        private async void OnChanged(object sender, FileSystemEventArgs e, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"File '{e.Name}' {e.ChangeType.ToString().ToLower()}");

            if (e.Name == "content.html" || e.Name == "header.html" || e.Name == "footer.html")
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
}