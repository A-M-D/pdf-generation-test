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
        private readonly HtmlProcessingService _htmlProcessingService;
        private readonly string _filepath;
        private FileSystemWatcher _htmlFileWatcher;

        public HtmlWatcherBackgroundService(ILogger<HtmlWatcherBackgroundService> logger, HtmlProcessingService htmlProcessingService, IOptions<HtmlWatcherOptions> options)
        {
            _logger = logger;
            _htmlProcessingService = htmlProcessingService;
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

            var files = Directory.GetFiles(_filepath);
            var stringifiedFileNames = string.Join(", ", files);
            _logger.LogInformation($"{stringifiedFileNames} found");

            _htmlFileWatcher = new FileSystemWatcher(_filepath, "*.html")
            {
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.CreationTime
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
                await _htmlProcessingService.Process(cancellationToken);
            }
        }
    }
}