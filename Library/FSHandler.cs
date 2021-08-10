using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Thumbnail_Generator_Library
{
    internal class FSHandler
    {
        public static void ApplyFolderIcon(string targetFolderPath, string iconFilePath)
        {
            string iniPath = Path.Combine(targetFolderPath, "desktop.ini");
            UnsetSystem(iniPath);

            // Writes desktop.ini Contents
            string iniContents = new StringBuilder()
                .AppendLine("[.ShellClassInfo]")
                .AppendLine("IconResource=" + iconFilePath + ",0")
                .AppendLine("IconFile=" + iconFilePath)
                .AppendLine("IconIndex=0")
                .AppendLine("[ViewState]")
                .AppendLine("Mode=")
                .AppendLine("Mode=")
                .AppendLine("Vid=")
                .ToString();

            File.WriteAllText(iniPath, iniContents);

            // Set Folder SYSTEM flag, to show thumbnail
            File.SetAttributes(
                targetFolderPath,
                FileAttributes.ReadOnly);

            SetSystem(iniPath);
        }

        public static void UnsetSystem(string targetPath)
        {
            if (File.Exists(targetPath))
            {
                // Make Read Writable
                File.SetAttributes(
                   targetPath,
                   File.GetAttributes(targetPath) &
                   ~(FileAttributes.Hidden | FileAttributes.System));
            }
        }

        public static void SetSystem(string targetPath)
        {
            File.SetAttributes(
               targetPath,
               File.GetAttributes(targetPath) | FileAttributes.Hidden | FileAttributes.System);
        }

        public static void ClearCache()
        {
            using RestartManagerSession rm = new();
            rm.RegisterProcess(GetShellProcess());
            rm.Shutdown(RestartManagerSession.ShutdownType.ForceShutdown);

            string localAppData = Environment.GetEnvironmentVariable("LocalAppData");
            string targetFolder = Path.Combine(localAppData, @"Microsoft\Windows\Explorer\");

            string[] targetFiles = Directory.GetFiles(targetFolder, "thumbcache_*.db");
            targetFiles = targetFiles.Concat(Directory.GetFiles(targetFolder, "iconcache_*.db")).ToArray();

            foreach (string file in targetFiles)
            {
                File.Delete(file);
            }

            rm.Restart();
        }

        [DllImport("user32")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr windowHandle, out uint processId);

        public static Process GetShellProcess()
        {
            try
            {
                var shellWindowHandle = GetShellWindow();

                if (shellWindowHandle != IntPtr.Zero)
                {
                    GetWindowThreadProcessId(shellWindowHandle, out var shellPid);

                    if (shellPid > 0)
                    {
                        return Process.GetProcessById((int) shellPid);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }
    }
}
