using System.Collections.Generic;
using System.IO;

namespace Thumbnail_Generator_Library
{
    class PathHandler
    {
        public static IEnumerable<string> GetAllDirectories(string rootDirectory, string searchPattern)
        {
            Stack<string> searchList = new();
            Stack<string> returnList = new();

            searchList.Push(rootDirectory);
            returnList.Push(rootDirectory);

            while (searchList.Count != 0)
            {
                string searchPath = searchList.Pop();
                try
                {
                    List<string> subDir = new(Directory.EnumerateDirectories(searchPath, searchPattern));
                    foreach (string directory in subDir) {
                        searchList.Push(directory);
                        if (IsDirectoryWritable(directory)) returnList.Push(directory);
                    }
                }
                catch { }
            }

            return returnList;
        }


        public static bool IsDirectoryWritable(string dirPath)
        {
            try
            {
                string randomFileName = Path.Combine(dirPath, Path.GetRandomFileName());
                using (FileStream fs = File.Create(randomFileName, 1, FileOptions.DeleteOnClose)) { }
                return true;
            }
            catch
            {
                 return false;
            }
        }
    }
}
