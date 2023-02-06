namespace LibMtDelete
{
    public static class MtDeleteCore
    {
        public static IEnumerable<FileSystemInfo> GetItemsForDeleting(string target, bool recursive)
        {
            if (File.Exists(target))
            {
                FileInfo targetFile = new FileInfo(target);
                yield return targetFile;
            }
            else if (Directory.Exists(target))
            {
                DirectoryInfo targetDir = new DirectoryInfo(target);
                if (targetDir.Parent == null)
                    throw new ArgumentException("Target cannot be a root directory");

                DirectoryInfo[] subDirs = targetDir.GetDirectories("*", SearchOption.AllDirectories);
                FileInfo[] files = targetDir.GetFiles("*", SearchOption.AllDirectories);

                if (!recursive && subDirs.Length > 0 && files.Length > 0)
                    throw new ArgumentException("Target directory is not empty", nameof(target));

                foreach (var subDir in subDirs)
                    foreach (var subItem in GetItemsForDeleting(subDir.FullName, recursive))
                        yield return subItem;

                foreach (var file in files)
                    yield return file;

                yield return targetDir;
            }
            else
            {
                throw new ArgumentException("Target is not exist", nameof(target));
            }
        }

        public static void Process(string target, bool recursive, int threadCount, bool getTotalCount, MtDeleteProgressCallback progressCallback)
        {
            if (threadCount < 0)
                threadCount = int.MaxValue;

            IEnumerable<FileSystemInfo> allItems =
                GetItemsForDeleting(target, recursive);

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = threadCount
            };

            int index = 0;
            int? totalCount = getTotalCount ? allItems.Count() : null;
            Parallel.ForEach(allItems, (item) =>
            {
                item.Delete();
                progressCallback?.Invoke(item, index, totalCount);
                Interlocked.Increment(ref index);
            });
        }

        public delegate void MtDeleteProgressCallback(FileSystemInfo currentItem, int index, int? totalCount);
    }
}