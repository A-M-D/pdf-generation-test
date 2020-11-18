
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Backgrounding
{
public class HtmlPollingBackgroundService : IHostedService, IDisposable
{
    private int executionCount = 0;
    private readonly ILogger<HtmlPollingBackgroundService> _logger;
    private readonly HtmlProcessingService _htmlProcessingService;
    private Timer _timer;

    public HtmlPollingBackgroundService(ILogger<HtmlPollingBackgroundService> logger, HtmlProcessingService htmlProcessingService)
    {
        _logger = logger;
        _htmlProcessingService = htmlProcessingService;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HtmlPollingBackgroundService running.");

        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));

        return Task.CompletedTask;
    }

    private async void DoWork(object state)
    {
        var count = Interlocked.Increment(ref executionCount);
        await _htmlProcessingService.Process();
        _logger.LogInformation("HtmlPollingBackgroundService is working. Count: {Count}", count);
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HtmlPollingBackgroundService is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}}