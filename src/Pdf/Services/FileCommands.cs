using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Pdf.Services
{
    public class FileCommands
    {
        private readonly List<string> _tempFiles = new List<string>();

        public async Task<string> CreateTempFile(string contents, string extension)
        {
            var temporaryFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.{extension}");

            using (var writer = new StreamWriter(temporaryFilePath))
            {
                await writer.WriteAsync(contents);
            }

            _tempFiles.Add(temporaryFilePath);

            return temporaryFilePath;
        }

        public void DeleteTempFiles()
        {
            foreach (var file in _tempFiles)
            {
                File.Delete(file);
            }
        }
    }
}