using LibFileOperation;

namespace LibMtCopy
{
    public static class MtCopyCore
    {
        public static IEnumerable<KeyValuePair<FileInfo, FileInfo>> GetFilesForWriting(string source, string destination, bool overwrite)
        {
            if (!Directory.Exists(destination))
                throw new ArgumentException("Destination must be a existed directory", nameof(destination));
            DirectoryInfo destDir = new DirectoryInfo(destination);

            if (File.Exists(source))
            {
                FileInfo sourceFile = new FileInfo(source);
                FileInfo destFile = new FileInfo(Path.Combine(destDir.FullName, sourceFile.Name));
                if (!overwrite && destFile.Exists)
                    yield break;

                yield return new KeyValuePair<FileInfo, FileInfo>(sourceFile, destFile);
            }
            else if (Directory.Exists(source))
            {
                DirectoryInfo sourceDir = new DirectoryInfo(source);
                if (sourceDir.Parent == null)
                    throw new ArgumentException("Destination cannot be a root directory");

                string sourceDirParentPath = sourceDir.Parent.FullName;
                IEnumerable<FileInfo> sourceFiles = CommonUtils.EnumDiretoryFiles(sourceDir);

                foreach (var sourceFile in sourceFiles)
                {
                    string sourceFileRelPath = Path.GetRelativePath(sourceDirParentPath, sourceFile.FullName);
                    string destFilePath = Path.Combine(destDir.FullName, sourceFileRelPath);
                    FileInfo destFile = new FileInfo(destFilePath);

                    yield return new KeyValuePair<FileInfo, FileInfo>(sourceFile, destFile);
                }
            }
            else
            {
                throw new ArgumentException("Source is not exist", nameof(source));
            }
        }
        
        public static void Process(IEnumerable<string> sources, string destination, bool overwrite, int threadCount, bool getTotalCount, MtCopyProgressCallback progressCallback)
        {
            if (threadCount < 0)
                threadCount = int.MaxValue;

            IEnumerable<KeyValuePair<FileInfo, FileInfo>> allFiles = sources
                .Select(source => GetFilesForWriting(source, destination, overwrite))
                .Aggregate((files1, files2) => files1.Concat(files2));

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = threadCount
            };

            int index = 0;
            int? totalCount = getTotalCount ? allFiles.Count() : null;
            Parallel.ForEach(allFiles, options, (copy) =>
            {
                FileInfo sourceFile = copy.Key;
                FileInfo destFile = copy.Value;
                if (destFile.DirectoryName == null)
                    throw new Exception("Unexpected exceptions");

                Directory.CreateDirectory(destFile.DirectoryName);

                using FileStream source = sourceFile.OpenRead();
                using FileStream dest = destFile.Create();

                source.CopyTo(dest);

                progressCallback.Invoke(copy.Key, index, totalCount);
                Interlocked.Increment(ref index);
            });
        }
    }

    public delegate void MtCopyProgressCallback(FileInfo currentFile, int index, int? totalCount);
}