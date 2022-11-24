using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GistologyTransfer
{
    /// <summary>
    /// Асинхронный копировальщик файлов
    /// </summary>
    public static class FileCopy
    {

        public static async Task CopyFileAsync(string sourcePath, string destinationPath)
        {
            using (Stream source = File.OpenRead(sourcePath))
            {
                using (Stream destination = File.Create(destinationPath))
                {
                    await source.CopyToAsync(destination);
                }
            }

        }
    }
}
