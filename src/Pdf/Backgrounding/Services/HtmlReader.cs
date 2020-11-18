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
    public class HtmlReader
    {
        private readonly ILogger<HtmlReader> _logger;

        public HtmlReader(ILogger<HtmlReader> logger)
        {
            _logger = logger;
        }

        public async Task<string> Read(string fullname)
        {
            _logger.LogInformation($"Reading file '{fullname}'");
            using (var sr = new StreamReader(fullname))
            {
                return await sr.ReadToEndAsync();
            };
        }
    }
}