namespace bg3_modders_multitool.Models
{
    /// <summary>
    /// Error from linting lsx files
    /// </summary>
    public class LintingError
    {
        /// <summary>
        /// An lsx linting error
        /// </summary>
        /// <param name="path">The file path</param>
        /// <param name="error">The error</param>
        public LintingError(string path, string error)
        {
            Path = path;
            Error = error;
        }

        public string Path { get; set; }
        public string Error { get; set; }
    }
}
