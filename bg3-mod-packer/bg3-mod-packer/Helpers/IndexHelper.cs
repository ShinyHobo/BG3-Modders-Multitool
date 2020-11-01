namespace bg3_mod_packer.Helpers
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using bg3_mod_packer.Models;
    using System.Windows;
    using System.Linq;
    using Lucene.Net.Store;
    using Lucene.Net.Analysis;
    using Lucene.Net.Util;
    using Lucene.Net.Index;
    using Lucene.Net.Documents;
    using Lucene.Net.Analysis.Standard;
    using Lucene.Net.Search;
    using Lucene.Net.QueryParsers.Classic;
    using Lucene.Net.Analysis.Shingle;
    using System.Threading.Tasks;

    public class IndexHelper
    {
        private string[] extensionsToExclude = { ".png", ".DDS", ".lsfx", ".lsbc", ".lsbs", ".ttf", ".gr2", ".GR2", ".tga" };
        private readonly string luceneIndex = "lucene/index";
        public SearchResults DataContext;
        private FSDirectory FSDirectory;

        public IndexHelper()
        {
            FSDirectory = FSDirectory.Open(luceneIndex);
        }

        /// <summary>
        /// Generates an index using the given filelist.
        /// </summary>
        /// <param name="filelist">The list of files to index.</param>
        public Task Index(List<string> filelist)
        {
            return Task.Run(() =>
            {
                // .lsf files do not have spaces to use as token separators, they must use shingles
                //var lsfFiles = filelist.Where(f => Path.GetExtension(f) == ".lsf").ToList();
                // all other allowed files use spaces or line endings as token separators
                //var allOtherFiles = filelist.Where(f => Path.GetExtension(f) != ".lsf").ToList();

                // Display total file count being indexed
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DataContext.IndexFileCount = 0;
                    DataContext.IndexFileTotal = filelist.Count;
                });

                if (System.IO.Directory.Exists(luceneIndex))
                    System.IO.Directory.Delete(luceneIndex, true);
                IndexFiles(filelist, new ShingleAnalyzerWrapper(new StandardAnalyzer(LuceneVersion.LUCENE_48), 2, 2, string.Empty, true, true, string.Empty));
                //IndexFiles(allOtherFiles, new StandardAnalyzer(LuceneVersion.LUCENE_48));
            });
        }

        /// <summary>
        /// Indexes the given files using an analyzer.
        /// </summary>
        /// <param name="files">The file list to index.</param>
        /// <param name="analyzer">The analyzer to use when indexing.</param>
        private void IndexFiles(List<string> files, Analyzer analyzer)
        {
            using (Analyzer a = analyzer)
            {
                IndexWriterConfig config = new IndexWriterConfig(LuceneVersion.LUCENE_48, a);
                using (IndexWriter writer = new IndexWriter(FSDirectory, config))
                {
                    foreach (string file in files)
                    {
                        IndexLuceneFile(file, writer);
                    }
                    writer.Commit();
                }
            }
        }

        /// <summary>
        /// Adds a file to the index.
        /// </summary>
        /// <param name="file">The file to add.</param>
        /// <param name="writer">The index to write to.</param>
        private void IndexLuceneFile(string file, IndexWriter writer)
        {
            var fileName = Path.GetFileName(file);
            var extension = Path.GetExtension(file);
            // if file type is excluded, only track file name and path so it can be searched for by name
            var contents = extensionsToExclude.Contains(extension) ? string.Empty : File.ReadAllText(file);
            var doc = new Document
            {
                //new Int64Field("id", id, Field.Store.YES),
                new TextField("path", file, Field.Store.YES),
                new TextField("title", fileName, Field.Store.YES),
                new TextField("body", contents, Field.Store.NO)
            };
            writer.AddDocument(doc);

            Application.Current.Dispatcher.Invoke(() =>
            {
                DataContext.IndexFileCount++;
            });
        }

        /// <summary>
        /// Searches for and displays results.
        /// </summary>
        /// <param name="search">The text to search for. Supports file title and contents.</param>
        public Task<List<string>> SearchFiles(string search)
        {
            return Task.Run(() => { 
                var matches = new List<string>();
                if(!System.IO.Directory.Exists(luceneIndex)||!System.IO.Directory.EnumerateFiles(luceneIndex).Any())
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += $"No index available! Please unpack game assets and generate an index.\n";
                    });
                    return matches;
                }

                if(DirectoryReader.IndexExists(FSDirectory))
                {
                    using (Analyzer analyzer = new ShingleAnalyzerWrapper(new StandardAnalyzer(LuceneVersion.LUCENE_48), 2, 2, string.Empty, true, true, string.Empty))
                    using (IndexReader reader = DirectoryReader.Open(FSDirectory))
                    {
                        IndexSearcher searcher = new IndexSearcher(reader);
                        MultiFieldQueryParser queryParser = new MultiFieldQueryParser(LuceneVersion.LUCENE_48, new[] { "title", "body" }, analyzer)
                        {
                            AllowLeadingWildcard = true
                        };
                        Query searchTermQuery = queryParser.Parse('*' + search + '*');

                        BooleanQuery aggregateQuery = new BooleanQuery() {
                            { searchTermQuery, Occur.MUST }
                        };

                        if (reader.MaxDoc != 0)
                        {
                            // perform search
                            TopDocs topDocs = searcher.Search(aggregateQuery, reader.MaxDoc);

                            Application.Current.Dispatcher.Invoke(() => {
                                ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += $"Search returned {topDocs.ScoreDocs.Length} results\n";
                            });

                            // display results
                            foreach (ScoreDoc scoreDoc in topDocs.ScoreDocs)
                            {
                                float score = scoreDoc.Score;
                                int docId = scoreDoc.Doc;

                                Document doc = searcher.Doc(docId);

                                matches.Add(doc.Get("path"));
                            }
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(() => {
                                ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += "No documents available. Please generate the index again.\n";
                            });
                        }
                    }
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += "Index does not exist yet.\n";
                    });
                }
                
                return matches;
            });
        }

        #region Static Methods

        /// <summary>
        /// Gets a list of files in a directory.
        /// </summary>
        /// <param name="directory">The directory root to search.</param>
        /// <returns>A list of files in the directory.</returns>
        public static List<string> DirectorySearch(string directory)
        {
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            var fileList = RecurisiveFileSearch(directory);
            if (fileList.Count == 0)
            {
                ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += $"No files unpacked for indexing!\n";
            }
            return fileList;
        }

        /// <summary>
        /// Recursively searches for all files within the given directory.
        /// </summary>
        /// <param name="directory">The directory root to search.</param>
        /// <returns>A list of files in the directory.</returns>
        private static List<string> RecurisiveFileSearch(string directory)
        {
            var fileList = new List<string>();
            foreach (string dir in System.IO.Directory.GetDirectories(directory))
            {
                foreach (string file in System.IO.Directory.GetFiles(dir))
                {
                    fileList.Add(Path.GetFullPath(file));
                }
                fileList.AddRange(RecurisiveFileSearch(dir));
            }
            return fileList;
        }

        /// <summary>
        /// Gets the complete list of extensions of the files in the given file list.
        /// </summary>
        /// <param name="fileList">The file list to scan.</param>
        /// <returns>The list of file extensions.</returns>
        public static List<string> GetFileExtensions(List<string> fileList)
        {
            var extensions = new List<string>();
            foreach (var file in fileList)
            {
                var extension = Path.GetExtension(file);
                if (!extensions.Contains(extension))
                {
                    extensions.Add(extension);
                }
            }
            return extensions;
        }

        #endregion
    }
}
