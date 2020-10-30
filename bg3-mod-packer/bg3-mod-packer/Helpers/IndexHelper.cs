namespace bg3_mod_packer.Helpers
{
    using System;
    using Indexer;
    using System.IO;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using bg3_mod_packer.Models;
    using System.Windows;
    using System.Collections.Concurrent;
    using System.Linq;

    public static class IndexHelper
    {
        /// <summary>
        /// Recursively searches for all files within the given directory.
        /// </summary>
        /// <param name="directory">The directory root to search.</param>
        /// <returns>A list of files in the directory.</returns>
        public static List<string> DirectorySearch(string directory)
        {
            var fileList = new List<string>();
            foreach (string dir in Directory.GetDirectories(directory))
            {
                foreach (string f in Directory.GetFiles(dir))
                {
                    fileList.Add(f);
                }
                fileList.AddRange(DirectorySearch(dir));
            }
            return fileList;
        }

        public static void Index(List<string> filelist)
        {
            Application.Current.Dispatcher.Invoke(() => {
                ((MainWindow)Application.Current.MainWindow.DataContext).IndexFileCount = 0;
                ((MainWindow)Application.Current.MainWindow.DataContext).IndexFileTotal = filelist.Count;
            });
            IndexEngine ie = new IndexEngine("bg3-index.db");
            filelist.ForEachAsync(8, async (file) => await IndexFile(file, ie));
        }

        public static async Task IndexFile(string file, IndexEngine ie)
        {
            var guid = Guid.NewGuid().ToString();
            var fileName = Path.GetFileName(file);
            var contents = File.ReadAllText(file);
            var contentsBytes = Encoding.UTF8.GetBytes(contents);
            Document doc = new Document(guid, fileName, string.Empty, file, string.Empty, string.Empty, contentsBytes);
            await ie.AddAsync(doc).ContinueWith(delegate {
                Application.Current.Dispatcher.Invoke(() => {
                    ((MainWindow)Application.Current.MainWindow.DataContext).IndexFileCount++;
                });
            });
        }

        //https://devblogs.microsoft.com/pfxteam/implementing-a-simple-foreachasync-part-2/
        public static Task ForEachAsync<T>(this IEnumerable<T> source, int dop, Func<T, Task> body)
        {
            return Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(dop)
                select Task.Run(async delegate {
                    using (partition)
                        while (partition.MoveNext())
                            await body(partition.Current);
                }));
        }
    }
}
