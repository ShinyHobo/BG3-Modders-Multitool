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
        /// <param name="lintingErrorType">The error type</param>
        public LintingError(string path, string error, LintingErrorType lintingErrorType)
        {
            Path = path;
            Error = error;
            Type = lintingErrorType;
        }

        public string Path { get; set; }
        public string Error { get; set; }
        public LintingErrorType Type { get; set; }
}

    public enum LintingErrorType
    {
        Xml,
        AttributeMissing
    }
}
