/// <summary>
/// The indexer/searcher service.
/// </summary>
namespace bg3_modders_multitool.Services
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Linq;
    using Lucene.Net.Store;
    using Lucene.Net.Analysis;
    using Lucene.Net.Util;
    using Lucene.Net.Index;
    using Lucene.Net.Documents;
    using Lucene.Net.Search;
    using Lucene.Net.QueryParsers.Classic;
    using System.Threading.Tasks;
    using bg3_modders_multitool.ViewModels;
    using Lucene.Net.Analysis.Core;
    using Lucene.Net.Analysis.Util;
    using Alphaleonis.Win32.Filesystem;
    using Lucene.Net.Index.Extensions;
    using System.Collections.Concurrent;
    using Lucene.Net.Search.Spans;
    using System.Text.RegularExpressions;

    public class IndexHelper
    {
        // images: .png, .DDS, .dds, .tga
        // models: .ttf, .gr2, .GR2, .gtp
        // audio: .wem
        // video: .bk2
        // shaders: .bshd, .shd
        private static readonly string[] extensionsToExclude = { ".bin",".png", ".dds", ".DDS", ".ttf", ".gr2", ".GR2", ".fbx", ".dae", ".gtp", ".wem", ".bk2", ".ffxanim", ".tga", ".bshd", ".shd", ".jpg",".gts",".data",".patch",".psd" };
        private static readonly string[] imageExtensions = { ".png", ".dds", ".DDS", ".tga", ".jpg" };
        public static readonly string[] BinaryExtensions = { ".lsf", ".bin", ".loca", ".data", ".patch" };
        private static readonly string luceneIndex = "lucene/index";
        public SearchResults DataContext;
        public string SearchText;
        private readonly FSDirectory fSDirectory;

        public IndexHelper()
        {
            fSDirectory = FSDirectory.Open(luceneIndex);
        }

        public void Clear()
        {
            fSDirectory.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        #region Direct Indexing
        /// <summary>
        /// Indexes the pak files directly in memory without the need for unpacking them to disk
        /// </summary>
        /// <returns>The indexing task</returns>
        public Task IndexDirectly()
        {
            return Task.Run(() => {
                Application.Current.Dispatcher.Invoke(() => {
                    //DataContext.AllowIndexing = false;
                });

                var helpers = new List<PakReaderHelper>();
                var fileCount = 0;
                var paks = Alphaleonis.Win32.Filesystem.Directory.GetFiles(FileHelper.DataDirectory, "*.pak", System.IO.SearchOption.AllDirectories).Select(file => Path.GetFullPath(file)).ToList();
                foreach ( var pak in paks )
                {
                    var helper = new PakReaderHelper(pak);
                    if (helper.PackagedFiles != null)
                    {
                        helpers.Add(helper);
                        fileCount += helper.PackagedFiles.Count;
                    }
                }

                // Display total file count being indexed
                GeneralHelper.WriteToConsole(Properties.Resources.FileListRetrieved);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DataContext.IsIndexing = true;
                    DataContext.IndexFileTotal = fileCount;
                    DataContext.IndexStartTime = DateTime.Now;
                    DataContext.IndexFileCount = 0;
                });

                if (System.IO.Directory.Exists(luceneIndex))
                    System.IO.Directory.Delete(luceneIndex, true);
                IndexFilesDirectly(helpers);
            });
        }

        /// <summary>
        /// Indexes the provided pak helpers
        /// </summary>
        /// <param name="helpers">The pak reader helpers that contain the files streams</param>
        private void IndexFilesDirectly(List<PakReaderHelper> helpers)
        {
            GeneralHelper.WriteToConsole(Properties.Resources.IndexingInProgress);
            using (Analyzer a = new CustomAnalyzer())
            {
                IndexWriterConfig config = new IndexWriterConfig(LuceneVersion.LUCENE_48, a);
                config.SetUseCompoundFile(false);
                using (IndexWriter writer = new IndexWriter(fSDirectory, config))
                {
                    Parallel.ForEach(helpers, GeneralHelper.ParallelOptions, helper => {
                        Parallel.ForEach(helper.PackagedFiles, GeneralHelper.ParallelOptions, file => {
                            try
                            {
                                IndexLuceneFileDirectly(file.Name, helper, writer);
                            }
                            catch (OutOfMemoryException)
                            {
                                GeneralHelper.WriteToConsole(Properties.Resources.OutOfMemFailedToIndex, file);
                            }
                        });
                    });
                    GeneralHelper.WriteToConsole(Properties.Resources.FinalizingIndex);
                    writer.Commit();
                }
            }
            GeneralHelper.WriteToConsole(Properties.Resources.IndexFinished, DataContext.GetTimeTaken().ToString("hh\\:mm\\:ss"));
            Application.Current.Dispatcher.Invoke(() => {
                DataContext.IsIndexing = false;
            });
        }

        /// <summary>
        /// Indexes the file contents
        /// </summary>
        /// <param name="file">The internal pak file path</param>
        /// <param name="helper">The pak helper</param>
        /// <param name="writer">The index writer</param>
        private void IndexLuceneFileDirectly(string file, PakReaderHelper helper, IndexWriter writer)
        {
            var path = $"{helper.PakName}\\{file}";
            try
            {
                var fileName = Path.GetFileName(file);
                var extension = Path.GetExtension(file);

                var doc = new Document
                {
                    //new Int64Field("id", id, Field.Store.YES),
                    new TextField("path", path.Replace("/","\\"), Field.Store.YES),
                    new TextField("title", fileName, Field.Store.YES)
                };

                // if file type is excluded, only track file name and path so it can be searched for by name
                if (!extensionsToExclude.Contains(extension))
                {
                    var contents = helper.ReadPakFileContents(file);
                    doc.Add(new TextField("body", contents, Field.Store.NO));
                }

                writer.AddDocument(doc);
            }
            catch (Exception ex)
            {
                GeneralHelper.WriteToConsole(Properties.Resources.FailedToIndexFile, path, ex.Message);
            }
            lock (DataContext)
                DataContext.IndexFileCount++;
        }
        #endregion

        #region Indexing
        /// <summary>
        /// Generates an index using the given filelist.
        /// </summary>
        /// <param name="filelist">The list of files to index.</param>
        public Task Index(List<string> filelist = null)
        {
            return Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() => {
                    DataContext.AllowIndexing = false;
                });
                if (filelist==null)
                {
                    GeneralHelper.WriteToConsole(Properties.Resources.RetrievingFileList);
                    filelist = FileHelper.DirectorySearch(@"\\?\" + FileHelper.UnpackedDataPath);
                }

                // Display total file count being indexed
                GeneralHelper.WriteToConsole(Properties.Resources.FileListRetrieved);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DataContext.IsIndexing = true;
                    DataContext.IndexFileTotal = filelist.Count;
                    DataContext.IndexStartTime = DateTime.Now;
                    DataContext.IndexFileCount = 0;
                });

                if (System.IO.Directory.Exists(luceneIndex))
                    System.IO.Directory.Delete(luceneIndex, true);
                IndexFiles(filelist);
            });
        }

        /// <summary>
        /// Indexes the given files using an analyzer.
        /// </summary>
        /// <param name="files">The file list to index.</param>
        private void IndexFiles(List<string> files)
        {
            GeneralHelper.WriteToConsole(Properties.Resources.IndexingInProgress);
            using (Analyzer a = new CustomAnalyzer())
            {
                IndexWriterConfig config = new IndexWriterConfig(LuceneVersion.LUCENE_48, a);
                config.SetUseCompoundFile(false);
                using (IndexWriter writer = new IndexWriter(fSDirectory, config))
                {
                    Parallel.ForEach(files, GeneralHelper.ParallelOptions, file => {
                        try
                        {
                            IndexLuceneFile(file, writer);
                        }
                        catch (OutOfMemoryException)
                        {
                            GeneralHelper.WriteToConsole(Properties.Resources.OutOfMemFailedToIndex, file);
                        }
                    });
                    GeneralHelper.WriteToConsole(Properties.Resources.FinalizingIndex);
                    writer.Commit();
                }
            }
            GeneralHelper.WriteToConsole(Properties.Resources.IndexFinished, DataContext.GetTimeTaken().ToString("hh\\:mm\\:ss"));
            Application.Current.Dispatcher.Invoke(() => {
                DataContext.IsIndexing = false;
            });
        }

        /// <summary>
        /// Adds a file to the index.
        /// </summary>
        /// <param name="file">The file to add.</param>
        /// <param name="writer">The index to write to.</param>
        private void IndexLuceneFile(string file, IndexWriter writer)
        {
            var path = file.Replace(@"\\?\", string.Empty).Replace(@"\\", @"\").Replace($"{FileHelper.UnpackedDataPath}\\", string.Empty);
            try
            {
                var fileName = Path.GetFileName(file);
                var extension = Path.GetExtension(file);

                var doc = new Document
                {
                    //new Int64Field("id", id, Field.Store.YES),
                    new TextField("path", path, Field.Store.YES),
                    new TextField("title", fileName, Field.Store.YES)
                };

                // if file type is excluded, only track file name and path so it can be searched for by name
                if (!extensionsToExclude.Contains(extension))
                {
                    var contents = File.ReadAllText(file);
                    doc.Add(new TextField("body", contents, Field.Store.NO));
                }
                
                writer.AddDocument(doc);
            }
            catch(Exception ex)
            {
                GeneralHelper.WriteToConsole(Properties.Resources.FailedToIndexFile, path, ex.Message);
            }
            lock(DataContext)
                DataContext.IndexFileCount++;
        }
        #endregion

        #region Searching
        /// <summary>
        /// Determines whether a lucene index directory exists.
        /// </summary>
        /// <returns>Whether the index exists.</returns>
        public static bool IndexDirectoryExists()
        {
            return System.IO.Directory.Exists(luceneIndex) && System.IO.Directory.EnumerateFiles(luceneIndex).Any();
        }

        public static bool IsIndexCorrupt(FSDirectory fSDirectory)
        {
            return !new CheckIndex(fSDirectory).DoCheckIndex().Clean;
        }

        /// <summary>
        /// Searches for and displays results.
        /// </summary>
        /// <param name="search">The text to search for. Supports file title and contents.</param>
        /// <param name="writeToConsole">Whether or not to write search status to console (errors still report).</param>
        /// <param name="selectedFileTypes">The selected file types to filter on</param>
        /// <param name="enableLeadingWildCard">Whether to enable the leading wildcard. Disabling is faster, but will produce fewer and/or unexpected results</param>
        /// <returns>The list of matches and the list of filtered matches</returns>
        public Task<(List<string> Matches,List<string>FilteredMatches)> SearchFiles(string search, bool writeToConsole = true, System.Collections.IList selectedFileTypes = null, bool enableLeadingWildCard = true)
        {
            SearchText = search;
            return Task.Run(() => { 
                var matches = new List<string>();
                var filteredMatches = new List<string>();
                if (!IndexDirectoryExists() && !DirectoryReader.IndexExists(fSDirectory))
                {
                    GeneralHelper.WriteToConsole(Properties.Resources.IndexNotFound);
                    return (Matches: matches, FilteredMatches: filteredMatches);
                }

                try
                {
                    using (IndexReader reader = DirectoryReader.Open(fSDirectory))
                    {
                        IndexSearcher searcher = new IndexSearcher(reader);
                        BooleanQuery query = new BooleanQuery();
                        var wildCardChar = enableLeadingWildCard ? "*" : string.Empty;
                        var pathQuery = new WildcardQuery(new Term("path", wildCardChar + QueryParserBase.Escape(search.ToLower().Trim()) + '*'));
                        query.Add(pathQuery, Occur.SHOULD);

                        var searchTerms = search.Trim().ToLower().Split(' ');
                        if(searchTerms.Length > 1)
                        {
                            var spanQueries = new List<SpanQuery>();
                            for (int i = 0; i < searchTerms.Length; i++)
                            {
                                var term = searchTerms[i];
                                if (i == 0)
                                {
                                    WildcardQuery wildcard = new WildcardQuery(new Term("body", wildCardChar + term));
                                    SpanQuery spanWildcard = new SpanMultiTermQueryWrapper<WildcardQuery>(wildcard);
                                    spanQueries.Add(spanWildcard);
                                }
                                else if (i == searchTerms.Length - 1)
                                {
                                    SpanQuery last = new SpanMultiTermQueryWrapper<PrefixQuery>(new PrefixQuery(new Term("body", term)));
                                    spanQueries.Add(last);
                                }
                                else
                                {
                                    SpanQuery mid = new SpanTermQuery(new Term("body", term));
                                    spanQueries.Add(mid);
                                }
                            }
                            query.Add(new SpanNearQuery(spanQueries.ToArray(), 0, true), Occur.SHOULD);
                        }
                        else
                        {
                            query.Add(new WildcardQuery(new Term("body", wildCardChar + searchTerms[0] + '*')), Occur.SHOULD);
                        }

                        if (reader.MaxDoc != 0)
                        {
                            var start = DateTime.Now;
                            if(writeToConsole)
                                GeneralHelper.WriteToConsole(Properties.Resources.IndexSearchStarted);

                            // perform search
                            TopDocs topDocs = searcher.Search(query, reader.MaxDoc);

                            var filteredSomeResults = 0;
                            var missingExtensions = new List<string>();

                            // display results
                            foreach (ScoreDoc scoreDoc in topDocs.ScoreDocs)
                            {
                                float score = scoreDoc.Score;
                                int docId = scoreDoc.Doc;

                                Document doc = searcher.Doc(docId);
                                var path = doc.Get("path");
                                var ext = Path.GetExtension(path).ToLower();
                                ext = string.IsNullOrEmpty(ext) ? Properties.Resources.Extensionless : ext;
                                if (selectedFileTypes != null && !selectedFileTypes.Contains(ext)) // TODO - add option to turn this off in config
                                {
                                    filteredSomeResults++;
                                    if(!FileHelper.FileTypes.Contains(ext))
                                    {
                                        missingExtensions.Add(ext);
                                    }
                                    filteredMatches.Add(path);
                                    continue;
                                }

                                matches.Add(path);
                            }

                            if(missingExtensions.Count > 0)
                            {
                                GeneralHelper.WriteToConsole(Properties.Resources.MissingFileTypes, string.Join(",", missingExtensions.Distinct()));
                            }

                            if (writeToConsole)
                            {
                                GeneralHelper.WriteToConsole(Properties.Resources.IndexSearchReturned, matches.Count, TimeSpan.FromTicks(DateTime.Now.Subtract(start).Ticks).TotalMilliseconds);
                                if(filteredSomeResults > 0)
                                {
                                    GeneralHelper.WriteToConsole(Properties.Resources.ResultsHaveBeenFiltered, filteredSomeResults);
                                }
                            }
                        }
                        else
                        {
                            GeneralHelper.WriteToConsole(Properties.Resources.IndexSearchNoDocuments);
                        }
                    }
                }
                catch
                {
                    // Checking if the index is corrupt is slower than just letting it fail
                    GeneralHelper.WriteToConsole(Properties.Resources.IndexCorrupt);
                }

                return (Matches: matches, FilteredMatches: filteredMatches);
            });
        }
        #endregion

        /// <summary>
        /// Gets a list of matching lines within a given file.
        /// </summary>
        /// <param name="path">The file path to read from.</param>
        /// <returns>A list of file line and trimmed contents.</returns>
        public Dictionary<long, string> GetFileContents(string path)
        {
            var lines = new Dictionary<long, string>();
            if (File.Exists(path))
            {
                var extension = Path.GetExtension(path);
                var isExcluded = extensionsToExclude.Contains(extension);
                if (!isExcluded)
                {
                    lines = ReadFileContentsForMatches(File.ReadLines(path));
                }

                if (lines.Count == 0)
                {
                    if(imageExtensions.Contains(extension))
                    {
                        lines.Add(0, $"<InlineUIContainer xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Image Source=\"{path.Replace("\\\\?\\", "")}\" Height=\"500\"></Image></InlineUIContainer>");
                    }
                    else
                    {
                        lines.Add(0, Properties.Resources.NoLinesFound);
                    }
                }
            }
            else
            {
                var pakPath = path.Replace(FileHelper.UnpackedDataPath + "\\", string.Empty);
                var pak = pakPath.Split('\\')[0];
                var regex = new Regex(Regex.Escape(pak + "\\"));
                path = regex.Replace(pakPath, string.Empty, 1);
                pakPath = Alphaleonis.Win32.Filesystem.Directory.GetFiles(FileHelper.DataDirectory, "*.pak", System.IO.SearchOption.AllDirectories).FirstOrDefault(f => f.Contains(pak + ".pak"));
                if (pakPath != null)
                {
                    var helper = new PakReaderHelper(pakPath);
                    var contents = helper.ReadPakFileContents(path.Replace('\\', '/'));
                    lines = ReadFileContentsForMatches(contents.Split('\n'));
                }

                if (lines.Count == 0)
                {
                    lines.Add(0, string.Format(Properties.Resources.FileNoExist, path));
                }
            }
            return lines.OrderBy(l => l.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Reads the given file contents for line matches and highlights them
        /// </summary>
        /// <param name="contents">The file contents</param>
        /// <returns>The list of matched lines</returns>
        private Dictionary<long, string> ReadFileContentsForMatches(IEnumerable<string> contents)
        {
            var lines = new ConcurrentDictionary<long, string>();
            Parallel.ForEach(contents, GeneralHelper.ParallelOptions, (line, _, lineNumber) =>
            {
                var index = line.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    var text = System.Security.SecurityElement.Escape(line.Substring(index, SearchText.Length));
                    var escapedLine = System.Security.SecurityElement.Escape(line);
                    escapedLine = escapedLine.Replace(text, $"<Span Background=\"Yellow\">{text}</Span>");
                    lines.TryAdd(lineNumber, escapedLine);
                }
            });
            return lines.OrderBy(l => l.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }

    /// <summary>
    /// Custom analyzer for handling UUIDs. Forces lowercase and ignores common stop words
    /// </summary>
    public class CustomAnalyzer : Analyzer
    {
        protected override TokenStreamComponents CreateComponents(string fieldName, System.IO.TextReader reader)
        {
            Tokenizer tokenizer = new CustomTokenizer(LuceneVersion.LUCENE_48, reader);
            TokenStream filter = new LowerCaseFilter(LuceneVersion.LUCENE_48, tokenizer);
            return new TokenStreamComponents(tokenizer, filter);
        }
    }

    /// <summary>
    /// Custom tokenizer for handling UUIDs.
    /// </summary>
    public sealed class CustomTokenizer : CharTokenizer
    {
        public CustomTokenizer(LuceneVersion matchVersion, System.IO.TextReader input) : base(matchVersion, input) { }

        /// <summary>
        /// Split tokens on all command characters, spaces, and extended character codes
        /// </summary>
        /// <param name="c">The character to compare</param>
        /// <returns>Whether the token should be split.</returns>
        protected override bool IsTokenChar(int c)
        {
            return c > 32 && c < 127;
        }
    }
}
