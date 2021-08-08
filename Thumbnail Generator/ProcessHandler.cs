using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Thumbnail_Generator
{
    class ProcessHandler
    {
        private static readonly string[] supportedFiles = {
            "jpg", "jpeg", "png", "mp4", "mov", "wmv", "avi", "mkv"
        };

        private volatile int progressCount;
        private volatile float progressPercentage;

        private FSHandler fsHandler = new FSHandler();

        public async Task generateThumbnailsForFolder(string rootFolder, int fileCount, int threadCount, bool recursive, bool skipExisting, bool useShort, bool clearCache)
        {
            progressCount = 0;
            progressPercentage = 0;

            if (MainWindow.mainWindowInstance != null) MainWindow.disableControls();

            string[] pathList = { rootFolder };

            if (recursive)
            {
                pathList = pathList.Concat(Directory.GetDirectories(rootFolder, "*", SearchOption.AllDirectories)).ToArray();
            }

            await Task.Run(() => processParallel(pathList, fileCount, skipExisting, useShort, threadCount));

            if (MainWindow.mainWindowInstance != null) MainWindow.setProgress(100);

            if (clearCache) fsHandler.clearCache();

            if (MainWindow.mainWindowInstance != null) MainWindow.enableControls();
        }

        private void processParallel(string[] pathList, int fileCount, bool skipExisting, bool useShort, int maxThreads = 4)
        {
            _ = Parallel.ForEach(pathList,
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

                      if (MainWindow.mainWindowInstance != null) MainWindow.setProgress(progressPercentage);
                  }
              );
        }
    }
}
