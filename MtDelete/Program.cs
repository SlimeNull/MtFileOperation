using CommandLine;
using LibFileOperation;
using LibMtDelete;

internal class Program
{
    class Options
    {
        [Value(0, HelpText = "File or directory to delete", Required = true)]
        public string Target { get; set; } = string.Empty;

        [Option('r', "recursive", HelpText = "Delete recursively")]
        public bool Recursive { get; set; } = false;

        [Option('t', "thread-count", HelpText = "Thread count for processingi (use -1 for not limited thread count, default: 5)")]
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
                    MtDelete - SlimeNull [Version 20230206]
                    (c) SlimeNull, All rights reserved.
                    """);
                Console.WriteLine();

                Console.WriteLine($" Target       : {o.Target}");
                Console.WriteLine($" Recursive    : {o.Recursive}");
                Console.WriteLine($" Thread count : {o.ThreadCount}");
                Console.WriteLine();
                
                try
                {
                    DateTime startTime = DateTime.Now;
                    MtDeleteCore.Process(o.Target, o.Recursive, o.ThreadCount, o.ShowTotalCount, (currentFile, index, totalCount) =>
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