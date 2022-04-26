using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorWebAssemblyApp
{
    // originally named 'Global' now in .NET 6, that means something else
    public static class SystemWide
    {
        public const string CSV = "csv";
        public const string XLS = "xls";
        public const string XLSX = "xlsx";
        public const string XLSM = "xlsm";

        public const int FileCountWarning = 100;

        public static ImmutableList<string> AllowedFileTypes = ImmutableList.Create<string>
            (CSV, XLS, XLSX, XLSM);

        public static string CurrentConfig;
        public static string StartupFolder;

        public static string RelativeFolderPath(this string fullFolderPath, string basePath)
        {
            return fullFolderPath.Replace(basePath, "");
        }
        public static IEnumerable<string> EnumerateFilesRecursive(string root, ImmutableList<string> searchTypes = null)
        {
            var todo = new Queue<string>();
            todo.Enqueue(root);
            while (todo.Count > 0)
            {
                string dir = todo.Dequeue();
                string[] subdirs = new string[0];
                //string[] files = new string[0];
                IEnumerable<string> files = null;
                try
                {
                    subdirs = Directory.GetDirectories(dir);
                    if (searchTypes != null)
                    {
                        files = Directory.GetFiles(dir, "*.*")
                            .Where(file => searchTypes.Any(file.ToLower().EndsWith));
                    }
                    else
                    {
                        files = Directory.GetFiles(dir, "*.*");
                    }
                }
                catch (IOException)
                {
                }
                catch (System.UnauthorizedAccessException)
                {
                }

                foreach (string subdir in subdirs)
                {
                    todo.Enqueue(subdir);
                }

                if (files != null)
                {
                    foreach (string filename in files)
                    {
                        yield return filename;
                    }
                }
            }
        }

  

    }
}
