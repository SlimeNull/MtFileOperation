using LibFileOperation;

namespace LibMtDelete
{
    public static class MtDeleteCore
    {
        private static IEnumerable<IEnumerable<T>> CombineSequenceContainer<T>(IEnumerable<IEnumerable<T>> sequence1, IEnumerable<IEnumerable<T>> sequence2)
        {
            IEnumerator<IEnumerable<T>> enumerator1 = sequence1.GetEnumerator();
            IEnumerator<IEnumerable<T>> enumerator2 = sequence2.GetEnumerator();

            bool seq1Next = false;
            bool seq2Next = false;
            while (true)
            {
                seq1Next = enumerator1.MoveNext();
                seq2Next = enumerator2.MoveNext();

                if (!seq2Next)
                {
                    yield return enumerator1.Current;
                    while (enumerator1.MoveNext())
                        yield return enumerator1.Current;
                }
                else if (!seq1Next)
                {
                    yield return enumerator2.Current;
                    while (enumerator2.MoveNext())
                        yield return enumerator2.Current;
                }
                else
                {
                    yield return enumerator1.Current.Concat(enumerator2.Current);
                }
            }
        }

        public static IEnumerable<FileInfo> GetFilesForDeleting(string target, bool recursive)
        {
            if (File.Exists(target))
            {
                FileInfo targetFile = new FileInfo(target);
                yield return targetFile;
            }
            else if (Directory.Exists(target))
            {
                DirectoryInfo targetDir = new DirectoryInfo(target);

                if (recursive)
                {
                    foreach (var file in CommonUtils.EnumDiretoryFiles(targetDir))
                        yield return file;
                }
                else
                {
                    foreach (var file in targetDir.EnumerateFiles())
                        yield return file;
                }
            }
            else
            {
                throw new ArgumentException("Target is not exist", nameof(target));
            }
        }

        public static IEnumerable<IEnumerable<DirectoryInfo>> GetLayerdDirectoriesForDeleting(string target, bool recursive)
        {
            if (File.Exists(target))
            {
                yield break;
            }
            else if (Directory.Exists(target))
            {
                DirectoryInfo targetDir = new DirectoryInfo(target);

                if (recursive)
                {
                    var subDirs = targetDir.EnumerateDirectories();

                    if (subDirs.Any())
                    {
                        var layerdSubDirs = subDirs
                            .Select(subdir => GetLayerdDirectoriesForDeleting(subdir.FullName, recursive))
                            .Aggregate((seq1, seq2) => CombineSequenceContainer(seq1, seq2));

                        foreach (var layer in layerdSubDirs)
                            yield return layer;
                    }

                    yield return new DirectoryInfo[] { targetDir };
                }
                else
                {
                    yield return targetDir.EnumerateDirectories();
                    yield return new DirectoryInfo[] { targetDir };
                }
            }
            else
            {
                throw new ArgumentException("Target is not exist", nameof(target));
            }
        }

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

                if (!recursive)
                {
                    yield return targetDir;
                    yield break;
                }

                foreach (var file in CommonUtils.EnumDiretoryFiles(targetDir))
                    yield return file;

                foreach (var subDir in CommonUtils.EnumDirectorySubDirectories(targetDir))
                    yield return subDir;

                yield return targetDir;
            }
            else
            {
                throw new ArgumentException("Target is not exist", nameof(target));
            }
        }

        public static void Process(IEnumerable<string> targets, bool recursive, int threadCount, bool getTotalCount, MtDeleteProgressCallback progressCallback)
        {
            if (threadCount < 0)
                threadCount = int.MaxValue;

            IEnumerable<FileInfo> allFilesForDeleting = targets
                .Select(target => GetFilesForDeleting(target, recursive))
                .Aggregate((items1, items2) => items1.Concat(items2));
            IEnumerable<IEnumerable<DirectoryInfo>> allLayerdDirectoriesForDeleting = targets
                .Select(target => GetLayerdDirectoriesForDeleting(target, recursive))
                .Aggregate((items1, items2) => items1.Concat(items2));

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = threadCount
            };

            int index = 0;
            int? totalCount = getTotalCount ? allFilesForDeleting.Count() : null;

            Parallel.ForEach(allFilesForDeleting, options, (item) =>
            {
                item.Delete();
                progressCallback?.Invoke(item, index, totalCount);
                Interlocked.Increment(ref index);
            });

            foreach (var layer in allLayerdDirectoriesForDeleting)
            {
                Parallel.ForEach(layer, options, (item) =>
                {
                    item.Delete();
                    progressCallback?.Invoke(item, index, totalCount);
                    Interlocked.Increment(ref index);
                });
            }
        }

        public delegate void MtDeleteProgressCallback(FileSystemInfo currentItem, int index, int? totalCount);
    }
}