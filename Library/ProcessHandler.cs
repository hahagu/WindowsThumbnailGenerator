using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Thumbnail_Generator_Library
{
    public class ProcessHandler
    {
        private static readonly string[] SupportedFiles = {
            "jpg", "jpeg", "png", "mp4", "mov", "wmv", "avi", "mkv"
        };

        private static volatile int ProgressCount;
        private static volatile float ProgressPercentage;

        public static async Task<int> GenerateThumbnailsForFolder(IProgress<float> Progress, string RootFolder, int FileCount, int MaxThreads, bool Recurse, bool ClearCache, bool SkipExisting, bool ShortCover)
        {
            ProgressCount = 0;
            ProgressPercentage = 0;

            string[] pathList = { RootFolder };

            await Task.Run(() => {
                if (Recurse)
                {
                    pathList = pathList.Concat(Directory.GetDirectories(RootFolder, "*", SearchOption.AllDirectories)).ToArray();
                }
            });

            await Task.Run(() =>
            {
                _ = Parallel.ForEach(
                pathList,
                new ParallelOptions { MaxDegreeOfParallelism = MaxThreads },
                directory =>
                {
                    ProgressCount++;

                    string iconLocation = Path.Combine(directory, "thumb.ico");
                    string iniLocation = Path.Combine(directory, "desktop.ini");

                    if (File.Exists(iniLocation) && SkipExisting)
                    {
                        return;
                    }

                    FSHandler.UnsetSystem(iconLocation);

                    string[] FileList = { };
                    foreach (string fileFormat in SupportedFiles)
                    {
                        FileList = FileList.Concat(Directory.GetFiles(directory, "*." + fileFormat)).ToArray();
                    }

                    if (FileList.Length <= 0) return;
                    if (FileList.Length > FileCount) FileList = FileList.Take(FileCount).ToArray();

                    ImageHandler.GenerateThumbnail(FileList, iconLocation, ShortCover);

                    FSHandler.SetSystem(iconLocation);
                    FSHandler.ApplyFolderIcon(directory, @".\thumb.ico");

                    ProgressPercentage = (float)ProgressCount / pathList.Length * 100;

                    Progress.Report(ProgressPercentage);
                });
            });

            if (ClearCache){
                await Task.Run(() => {
                   FSHandler.ClearCache();
                });
            }

            return 0;
        }
    }
}
