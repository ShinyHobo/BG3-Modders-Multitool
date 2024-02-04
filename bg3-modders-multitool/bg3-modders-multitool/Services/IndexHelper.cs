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
    using Newtonsoft.Json;
    using static Lucene.Net.Documents.Field;

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
        private static readonly string luceneRoot = "lucene";
        private static readonly string luceneIndex = $"{luceneRoot}/index";
        private static readonly string luceneDeltaDirectory = $"{luceneRoot}/paks";
        private static readonly string luceneCacheFile = $"{luceneRoot}\\cache.json";
        public SearchResults DataContext;
        public string SearchText;
        private readonly FSDirectory mainFSDirectory;

        public IndexHelper()
        {
            mainFSDirectory = FSDirectory.Open(luceneIndex);
        }

        public void Clear()
        {
            mainFSDirectory.Dispose();
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
                    DataContext.AllowIndexing = false;
                });

                if (System.IO.Directory.Exists(luceneIndex) && !File.Exists(luceneCacheFile))
                    System.IO.Directory.Delete(luceneIndex, true);

                if (System.IO.Directory.Exists(luceneDeltaDirectory))
                    System.IO.Directory.Delete(luceneDeltaDirectory, true);

                var helpers = new List<PakReaderHelper>();
                var fileCount = 0;
                var paks = PakReaderHelper.GetPakList();

                var cachedJson = new List<string>();
                if(File.Exists(luceneCacheFile))
                using (System.IO.TextReader reader = File.OpenText(luceneCacheFile))
                {
                    var fileContents = reader.ReadToEnd();
                    cachedJson = JsonConvert.DeserializeObject<List<string>>(fileContents);
                }

                foreach (var pak in paks.Where(p => !cachedJson.Contains(Path.GetFileNameWithoutExtension(p))))
                {
                    var helper = new PakReaderHelper(pak);
                    if (helper.PackagedFiles != null)
                    {
                        helpers.Add(helper);
                        fileCount += helper.PackagedFiles.Count;
                    }
                }
                
                if(helpers.Count == 0)
                {
                    GeneralHelper.WriteToConsole(Properties.Resources.IndexUpToDate);
                    Application.Current.Dispatcher.Invoke(() => {
                        DataContext.AllowIndexing = true;
                    });
                    return;
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
            var cachedPaks = new ConcurrentBag<string>();
            Parallel.ForEach(helpers, new ParallelOptions { MaxDegreeOfParallelism = 4 }, helper =>
            {
                using (Analyzer a = new CustomAnalyzer())
                {
                    IndexWriterConfig config = new IndexWriterConfig(LuceneVersion.LUCENE_48, a);
                    try
                    {
                        Alphaleonis.Win32.Filesystem.Directory.CreateDirectory($"{luceneDeltaDirectory}\\{helper.PakName}");
                        using (FSDirectory fSDirectory = FSDirectory.Open($"{luceneDeltaDirectory}\\{helper.PakName}"))
                        using (IndexWriter writer = new IndexWriter(fSDirectory, config))
                        {
                            Parallel.ForEach(helper.PackagedFiles, new ParallelOptions { MaxDegreeOfParallelism = 6 }, file =>
                            {
                                try
                                {
                                    IndexLuceneFileDirectly(file.Name, helper, writer);
                                }
                                catch (OutOfMemoryException)
                                {
                                    GeneralHelper.WriteToConsole(Properties.Resources.OutOfMemFailedToIndex, file);
                                }
                            });
                            writer.Commit();
                            cachedPaks.Add(helper.PakName);
                        }
                    }
                    catch (Exception ex)
                    {
                        GeneralHelper.WriteToConsole(ex.Message);
                    }
                }
            });

            GeneralHelper.WriteToConsole(Properties.Resources.MergingIndices);

            var originalTime = DataContext.IndexStartTime;

            Application.Current.Dispatcher.Invoke(() =>
            {
                DataContext.IndexFileCount = 0;
                DataContext.IndexStartTime = DateTime.Now;
                DataContext.IndexFileTotal = cachedPaks.Count;
            });

            // Merge indexes
            using (Analyzer a = new CustomAnalyzer())
            using (IndexWriter writer = new IndexWriter(mainFSDirectory, new IndexWriterConfig(LuceneVersion.LUCENE_48, a)))
            {
                foreach(var pak in cachedPaks)
                {
                    var indexDir = Path.Combine(Alphaleonis.Win32.Filesystem.Directory.GetCurrentDirectory(), luceneDeltaDirectory, pak);
                    if(Alphaleonis.Win32.Filesystem.Directory.Exists(indexDir))
                    {
                        using (var index = DirectoryReader.Open(FSDirectory.Open(indexDir)))
                        {
                            writer.AddIndexes(index);
                        }

                        Application.Current.Dispatcher.Invoke(() => { DataContext.IndexFileCount++; });
                    }
                }
            }

            GeneralHelper.WriteToConsole(Properties.Resources.DeletingTempIndecies);
            if (System.IO.Directory.Exists(luceneDeltaDirectory))
                System.IO.Directory.Delete(luceneDeltaDirectory, true);

            var cacheInfo = new FileInfo(luceneCacheFile);
            if (!cacheInfo.Exists)
            {
                File.Create(luceneCacheFile).Dispose();
            }

            GeneralHelper.WriteToConsole(Properties.Resources.UpdatingIndexPakList);
            using (System.IO.TextReader reader = File.OpenText(luceneCacheFile))
            {
                var fileContents = reader.ReadToEnd();
                var cachedJson = JsonConvert.DeserializeObject<List<string>>(fileContents);
                if (cachedJson == null)
                {
                    cachedJson = cachedPaks.OrderBy(x => x).ToList();
                }
                else
                {
                    cachedJson.AddRange(cachedPaks);
                    cachedJson = cachedJson.OrderBy(x => x).Distinct().ToList();
                }
                reader.Close();

                var contentsToWriteToFile = JsonConvert.SerializeObject(cachedJson, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                File.WriteAllText(luceneCacheFile, contentsToWriteToFile);
            }

            var timeTaken = TimeSpan.FromTicks(DateTime.Now.Subtract(originalTime.Add(DataContext.GetTimeTaken())).Ticks);
            GeneralHelper.WriteToConsole(Properties.Resources.IndexFinished, timeTaken.ToString("hh\\:mm\\:ss"));
            Application.Current.Dispatcher.Invoke(() => {
                DataContext.IsIndexing = false;
                DataContext.AllowIndexing = true;
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
                //var fileName = Path.GetFileName(file);
                var extension = Path.GetExtension(file);

                var doc = new Document
                {
                    //new Int64Field("id", id, Field.Store.YES),
                    new TextField("path", path.Replace("\\","/"), Field.Store.YES),
                    //new TextField("title", fileName, Field.Store.YES)
                };

                // if file type is excluded, only track file name and path so it can be searched for by name
                if (!extensionsToExclude.Contains(extension))
                {
                    var contents = helper.ReadPakFileContents(file, true);
                    doc.Add(new TextField("body", System.Text.Encoding.UTF8.GetString(contents), Field.Store.NO));
                }

                writer.AddDocument(doc);
            }
            catch (Exception ex)
            {
                GeneralHelper.WriteToConsole(Properties.Resources.FailedToIndexFile, path, ex.Message);
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                DataContext.IndexFileCount++;
            });
        }
        #endregion

        #region Indexing
        /// <summary>
        /// Generates an index using the given filelist.
        /// </summary>
        /// <param name="filelist">The list of files to index.</param>
        public Task Index(string[] filelist = null)
        {
            return Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() => {
                    DataContext.AllowIndexing = false;
                });
                if (filelist==null)
                {
                    GeneralHelper.WriteToConsole(Properties.Resources.RetrievingFileList);
                    filelist = Alphaleonis.Win32.Filesystem.Directory.GetFiles(FileHelper.UnpackedDataPath, "*", System.IO.SearchOption.AllDirectories);
                }

                // Display total file count being indexed
                GeneralHelper.WriteToConsole(Properties.Resources.FileListRetrieved);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DataContext.IsIndexing = true;
                    DataContext.IndexFileTotal = filelist.Length;
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
        private void IndexFiles(string[] files)
        {
            GeneralHelper.WriteToConsole(Properties.Resources.IndexingInProgress);
            using (Analyzer a = new CustomAnalyzer())
            {
                IndexWriterConfig config = new IndexWriterConfig(LuceneVersion.LUCENE_48, a);
                config.SetUseCompoundFile(false);
                using (IndexWriter writer = new IndexWriter(mainFSDirectory, config))
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
                //var fileName = Path.GetFileName(file);
                var extension = Path.GetExtension(file);

                var doc = new Document
                {
                    //new Int64Field("id", id, Field.Store.YES),
                    new TextField("path", path, Field.Store.YES),
                    //new TextField("title", fileName, Field.Store.YES)
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
            Application.Current.Dispatcher.Invoke(() =>
            {
                DataContext.IndexFileCount++;
            });
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
                var matches = new ConcurrentBag<string>();
                var filteredMatches = new ConcurrentBag<string>();
                if (!IndexDirectoryExists() && !DirectoryReader.IndexExists(mainFSDirectory))
                {
                    GeneralHelper.WriteToConsole(Properties.Resources.IndexNotFound);
                    return (Matches: matches.ToList(), FilteredMatches: filteredMatches.ToList());
                }

                try
                {
                    using (IndexReader reader = DirectoryReader.Open(mainFSDirectory))
                    {
                        IndexSearcher searcher = new IndexSearcher(reader);
                        BooleanQuery query = new BooleanQuery();

                        if(!string.IsNullOrEmpty(search))
                        {
                            var wildCardChar = enableLeadingWildCard ? "*" : string.Empty;
                            var pathQuery = new WildcardQuery(new Term("path", wildCardChar + QueryParserBase.Escape(search.ToLower().Trim()) + '*'));
                            query.Add(pathQuery, Occur.SHOULD);

                            var searchTerms = search.Trim().ToLower().Split(' ');
                            if (searchTerms.Length > 1)
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
                        }

                        if (reader.MaxDoc != 0)
                        {
                            var start = DateTime.Now;
                            if(writeToConsole)
                                GeneralHelper.WriteToConsole(Properties.Resources.IndexSearchStarted);

                            // perform search
                            TopDocs topDocs = string.IsNullOrEmpty(search) ? searcher.Search(new MatchAllDocsQuery(), reader.MaxDoc) : searcher.Search(query, reader.MaxDoc);

                            var filteredSomeResults = 0;
                            var missingExtensions = new List<string>();

                            // display results
                            var pathSet = new HashSet<string> { "path" };
                            Parallel.ForEach(topDocs.ScoreDocs, GeneralHelper.ParallelOptions, scoreDoc => {
                                float score = scoreDoc.Score;
                                int docId = scoreDoc.Doc;

                                Document doc = searcher.Doc(docId, pathSet);
                                var path = doc.Get("path").Replace("/", "\\");
                                var ext = Path.GetExtension(path).ToLower();
                                ext = string.IsNullOrEmpty(ext) ? Properties.Resources.Extensionless : ext;
                                if (selectedFileTypes != null && !selectedFileTypes.Contains(ext))
                                {
                                    filteredSomeResults++;
                                    if (!FileHelper.FileTypes.Contains(ext))
                                    {
                                        missingExtensions.Add(ext);
                                    }
                                    filteredMatches.Add(path);
                                    return;
                                }

                                matches.Add(path);
                            });

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

                return (Matches: matches.ToList(), FilteredMatches: filteredMatches.ToList());
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
            var fileExists = File.Exists(path);
            var extension = Path.GetExtension(path);
            var isExcluded = extensionsToExclude.Contains(extension);
            if (fileExists)
            {
                if (!isExcluded)
                {
                    // TODO if lsf, convert first
                    lines = ReadFileContentsForMatches(File.ReadLines(path));
                }

                if (lines.Count == 0)
                {
                    if(imageExtensions.Contains(extension))
                    {
                        lines.Add(0, $"<InlineUIContainer xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Image Source=\"{path.Replace("\\\\?\\", "")}\" Height=\"500\"></Image></InlineUIContainer>");
                    }
                }
            }
            else
            {
                var helper = PakReaderHelper.GetPakHelper(path);
                if (helper.Path != null)
                {
                    fileExists = true;
                    var contents = new byte[0];
                    var textFileContents = string.Empty;

                    contents = helper.Helper.ReadPakFileContents(helper.Path, true);

                    if (!isExcluded)
                    {
                        textFileContents = contents.Length > 0 ? System.Text.Encoding.UTF8.GetString(contents) : textFileContents;
                        var allLines = new List<string>();
                        using (System.IO.StringReader reader = new System.IO.StringReader(textFileContents))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                allLines.Add(line);
                            }
                        }
                        lines = ReadFileContentsForMatches(allLines);
                    }

                    if (lines.Count == 0)
                    {
                        if (contents != null && contents.Length != 0)
                        {
                            if (imageExtensions.Contains(extension)) // Normal texture
                            {
                                lines.Add(0, $"<InlineUIContainer xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Image Base64 Source=\"{Convert.ToBase64String(contents)}\" Height=\"250\"></Image></InlineUIContainer>");
                            }
                            else if (FileHelper.IsGTP(path)) // Virtual texture
                            {
                                var previewCount = 0;
                                foreach (var file in TextureHelper.ExtractGTPContents(helper.Path, helper.Helper, contents))
                                {
                                    lines.Add(previewCount, $"<InlineUIContainer xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Image Base64 Source=\"{Convert.ToBase64String(file)}\" Height=\"250\"></Image></InlineUIContainer>");
                                    previewCount++;
                                }

                                if (lines.Count == 0)
                                {
                                    lines.Add(0, string.Format(Properties.Resources.CouldNotLoadImage, $"{helper.Pak}\\{helper.Path}"));
                                }
                            }
                        }
                        else 
                        {
                            lines.Add(0, string.Format(Properties.Resources.EmptyFile, $"{helper.Pak}\\{helper.Path}"));
                        }
                    }
                }
            }

            if (lines.Count == 0)
            {
                if(fileExists)
                {
                    lines.Add(0, Properties.Resources.NoLinesFound);
                }
                else
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
                if(string.IsNullOrEmpty(SearchText))
                {
                    var text = System.Security.SecurityElement.Escape(line);
                    var escapedLine = System.Security.SecurityElement.Escape(line);
                    lines.TryAdd(lineNumber, escapedLine);
                }
                else
                {
                    var index = line.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                    {
                        var text = System.Security.SecurityElement.Escape(line.Substring(index, SearchText.Length));
                        var escapedLine = System.Security.SecurityElement.Escape(line);
                        escapedLine = escapedLine.Replace(text, $"<Span Background=\"DimGray\" Foreground=\"White\">{text}</Span>");
                        lines.TryAdd(lineNumber, escapedLine);
                    }
                }
                
            });
            return lines.OrderBy(l => l.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Deletes the index
        /// </summary>
        public void DeleteIndex()
        {
            if (System.IO.Directory.Exists(luceneRoot))
            {
                System.IO.Directory.Delete(luceneRoot, true);
                GeneralHelper.WriteToConsole(Properties.Resources.IndexCleared);
            }
            else
            {
                GeneralHelper.WriteToConsole(Properties.Resources.NoIndexToRemove);
            }
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
