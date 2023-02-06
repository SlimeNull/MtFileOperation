using CommandLine;
using LibFileOperation;
using LibMtCopy;
using System.Text;

internal class Program
{
    class Options
    {
        [Value(0, HelpText = "Source (file or directory)", Required = true)]
        public string Source { get; set; } = string.Empty;

        [Value(1, HelpText = "Destination (directory)")]
        public string Destination { get; set; } = string.Empty;

        [Option('w', "overwrite", HelpText = "Overwrite the destination file when copying the file.")]
        public bool Overwrite { get; set; } = false;

        [Option('t', "thread-count", HelpText = "Thread count for processing (use -1 for not limited thread count, default:5)")]
        public int ThreadCount { get; set; } = 5;

        [Option('c', "show-total-count", HelpText = "Show file total count")]
        public bool ShowTotalCount { get; set; } = false;
    }

    private static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(o =>
            {
                Console.WriteLine(
                    """
                    MtCopy - SlimeNull [Version 20230206]
                    (c) SlimeNull, All rights reserved.
                    """);
                Console.WriteLine();

                Console.WriteLine($" Source       : {o.Source}");
                Console.WriteLine($" Dest         : {o.Destination}");
                Console.WriteLine($" Overwirte    : {o.Overwrite}");
                Console.WriteLine($" Thread count : {o.ThreadCount}");
                Console.WriteLine();

                try
                {
                    DateTime startTime = DateTime.Now;

                    IEnumerable<FileSystemInfo> allSources = CommonUtils.MatchFileAndDirectories(o.Source);

                    if (!allSources.Any())
                        throw new Exception("No source that exists");

                    MtCopyCore.Process(allSources.Select(info => info.FullName), o.Destination, o.Overwrite, o.ThreadCount, o.ShowTotalCount, (currentFile, index, totalCount) =>
                    {
                        Console.WriteLine($" OK: {index}{(totalCount != null ? $"/{totalCount}" : string.Empty)} {currentFile.FullName}");
                    });

                    DateTime endTime = DateTime.Now;
                    Console.WriteLine($"Done. Elapsed: {CommonUtils.TimeSpan2String(endTime - startTime)}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
            });
    }
}