using System.Text;

namespace LibFileOperation
{
    public static class CommonUtils
    {
        public static IEnumerable<FileInfo> EnumDiretoryFiles(DirectoryInfo dir)
        {
            foreach (var file in dir.EnumerateFiles())
                yield return file;

            foreach (var subDir in dir.EnumerateDirectories())
                foreach (var file in EnumDiretoryFiles(subDir))
                    yield return file;
        }

        public static IEnumerable<DirectoryInfo> EnumDirectorySubDirectories(DirectoryInfo dir)
        {
            foreach (var subDir in dir.EnumerateDirectories())
            {
                foreach (var subSubDir in EnumDirectorySubDirectories(subDir))
                    yield return subSubDir;
                yield return subDir;
            }
        }

        /// <summary>
        /// Match file and directories (如果传入的 pattern 是一个目录，则返回只包含该目录的序列)
        /// </summary>
        /// <param name="pattern">匹配</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IEnumerable<FileSystemInfo> MatchFileAndDirectories(string pattern)
        {
            string? dirPath = Path.GetDirectoryName(pattern);
            string filePattern = Path.GetFileName(pattern);

            if (string.IsNullOrEmpty(dirPath))
                dirPath = ".";
            if (!Directory.Exists(dirPath))
                throw new ArgumentException("Directory not exist", nameof(pattern));

            DirectoryInfo dir = new DirectoryInfo(dirPath);
            if (string.IsNullOrEmpty(filePattern))
                return new FileSystemInfo[] { dir };
            return dir.EnumerateFileSystemInfos(filePattern);
        }

        public static string TimeSpan2String(TimeSpan timeSpan)
        {
            StringBuilder sb = new StringBuilder();

            bool hasDays = false;
            if (timeSpan.Days > 0)
            {
                sb.Append(timeSpan.Days);
                sb.Append('d');
                hasDays = true;
            }

            bool hasHours = false;
            if (timeSpan.Hours > 0 || hasDays)
            {
                sb.Append(timeSpan.Hours);
                sb.Append('h');
                hasHours = true;
            }

            bool hasMinutes = false;
            if (timeSpan.Minutes > 0 || hasHours)
            {
                sb.Append(timeSpan.Minutes);
                sb.Append('m');
                hasMinutes = true;
            }

            if (timeSpan.Seconds > 0 || hasMinutes)
            {
                sb.Append(timeSpan.Seconds);
                sb.Append('s');
            }

            sb.Append(timeSpan.Milliseconds);
            sb.Append("ms");

            return sb.ToString();
        }
    }
}