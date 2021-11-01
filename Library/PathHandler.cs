using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    string[] subDir = Directory.GetDirectories(searchPath, searchPattern);
                    foreach (string directories in subDir) {
                        searchList.Push(directories);
                        returnList.Push(directories);
                    }
                }
                catch { }
            }

            return returnList;
        }
    }
}
