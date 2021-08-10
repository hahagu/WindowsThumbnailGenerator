using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Thumbnail_Generator_Library
{
    public class ProcessHandler
    {
        private static readonly string[] supportedFiles = {
            "jpg", "jpeg", "png", "mp4", "mov", "wmv", "avi", "mkv"
        };

        private static volatile int progressCount;
        private static volatile float progressPercentage;

        public static async Task<int> GenerateThumbnailsForFolder(
            IProgress<float> progress,
            string rootFolder,
            int maxThumbCount,
            int maxThreads,
            bool recurse,
            bool clearCache,
            bool skipExisting,
            bool shortCover
        )
        {
            progressCount = 0;
            progressPercentage = 0;

            string[] pathList = { rootFolder };

            await Task.Run(() => {
                if (recurse)
                {
                    pathList = pathList.Concat(Directory.GetDirectories(rootFolder, "*", SearchOption.AllDirectories)).ToArray();
                }
            });

            await Task.Run(() =>
            {
                _ = Parallel.ForEach(
                pathList,
                new ParallelOptions { MaxDegreeOfParallelism = maxThreads },
                directory =>
                {
                    progressCount++;

                    string iconLocation = Path.Combine(directory, "thumb.ico");
                    string iniLocation = Path.Combine(directory, "desktop.ini");

                    if (File.Exists(iniLocation) && skipExisting)
                    {
                        return;
                    }

                    FSHandler.UnsetSystem(iconLocation);

                    string[] fileList = { };
                    foreach (string fileFormat in supportedFiles)
                    {
                        fileList = fileList.Concat(Directory.GetFiles(directory, "*." + fileFormat)).ToArray();
                    }

                    if (fileList.Length <= 0) return;
                    if (fileList.Length > maxThumbCount) fileList = fileList.Take(maxThumbCount).ToArray();

                    ImageHandler.GenerateThumbnail(fileList, iconLocation, shortCover);

                    FSHandler.SetSystem(iconLocation);
                    FSHandler.ApplyFolderIcon(directory, @".\thumb.ico");

                    progressPercentage = (float)progressCount / pathList.Length * 100;

                    progress.Report(progressPercentage);
                });
            });

            if (clearCache){
                await Task.Run(() => {
                   FSHandler.ClearCache();
                });
            }

            return 0;
        }
    }
}
