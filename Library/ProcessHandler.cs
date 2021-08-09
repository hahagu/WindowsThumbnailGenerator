using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Core_Library
{
    public class ProcessHandler
    {
        private static readonly string[] supportedFiles = {
            "jpg", "jpeg", "png", "mp4", "mov", "wmv", "avi", "mkv"
        };

        private static volatile int progressCount;
        private static volatile float progressPercentage;

        private static FSHandler fsHandler = new FSHandler();

        public static async Task<int> generateThumbnailsForFolder(IProgress<float> progress, string rootFolder, int fileCount, bool recursive, bool skipExisting, bool useShort, bool clearCache, int maxThreads = 4)
        {
            progressCount = 0;
            progressPercentage = 0;

            string[] pathList = { rootFolder };

            if (recursive)
            {
                pathList = pathList.Concat(Directory.GetDirectories(rootFolder, "*", SearchOption.AllDirectories)).ToArray();
            }

            await Task.Run(() =>
            {
                Parallel.ForEach(
                pathList,
                new ParallelOptions { MaxDegreeOfParallelism = maxThreads },
                directory =>
                {
                    string iconLocation = Path.Combine(directory, "thumb.ico");
                    string iniLocation = Path.Combine(directory, "desktop.ini");

                    if (File.Exists(iniLocation) && skipExisting) return;

                    ImageHandler imageHandler = new();
                    fsHandler.unsetSystem(iconLocation);

                    string[] fileList = { };
                    foreach (string fileFormat in supportedFiles)
                    {
                        fileList = fileList.Concat(Directory.GetFiles(directory, "*." + fileFormat)).ToArray();
                    }

                    if (fileList.Length <= 0) return;
                    if (fileList.Length > fileCount) fileList = fileList.Take(fileCount).ToArray();

                    imageHandler.generateThumbnail(fileList, iconLocation, useShort);

                    fsHandler.setSystem(iconLocation);
                    fsHandler.applyFolderIcon(directory, @".\thumb.ico");

                    progressCount++;
                    progressPercentage = (float)progressCount / pathList.Length * 100;

                    progress.Report(progressPercentage);
                });
            });


            if (clearCache) fsHandler.clearCache();

            return 0;
        }
    }
}
