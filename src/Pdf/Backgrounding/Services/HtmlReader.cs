using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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