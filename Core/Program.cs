using CanonicalForm.Core;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanonicalForm.ConsoleApp
{
    class Program
    {
        static CanonicalFormulaFormer _former = new CanonicalFormulaFormer(new RegexFormulaValidator(), new RegexGroupSearcher(), new GroupsDictionaryBuilder(), new GroupsDictionaryRenderer());

        static async Task Main(string[] args)
        {
            if (args.Length > 0)
            {
                await TransformFiles(args);
            }
            else
            {
                InteractiveTransform();
            }
        }

        static async Task TransformFiles(string[] fileNames)
        {
            Console.WriteLine("File proccessing starts...");
            var stringBuilder = new StringBuilder();
            foreach (var path in fileNames.Where(x=> File.Exists(x)))
            {
                try
                {
                    var lines = await File.ReadAllLinesAsync(path);
                    if (lines.Length == 0)
                    {
                        Console.WriteLine($"File '{path}' is empty.");
                        continue;
                    }

                    var parallelOptions = new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = lines.Length % 100
                    };

                    ConcurrentBag<string> resultCollection = new ConcurrentBag<string>();
                    var parallelResult = Parallel.ForEach(lines, parallelOptions, (formula, state) => resultCollection.Add(_former.Transform(formula)));
                    if (!parallelResult.IsCompleted)
                    {
                        stringBuilder.Clear().Append("Cannot transform file");
                        if (parallelResult.LowestBreakIteration.HasValue && lines.Length > parallelResult.LowestBreakIteration.Value)
                        {
                            stringBuilder.Append("Maybe problem in ").Append(lines[parallelResult.LowestBreakIteration.Value]);
                        }
                        Console.WriteLine(stringBuilder.ToString());
                        continue;
                    }

                    var resultPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".out");
                    //if (File.Exists(resultPath))
                    //{
                    //    Console.WriteLine($"Do you want rewrite file '{resultPath}'? Press PLUS to overwtire.");
                    //    if (Console.ReadKey().Key != ConsoleKey.OemPlus)
                    //    {
                    //        Console.WriteLine($"Please write path to save file:");
                    //        var newPath = Console.ReadLine();
                    //        Path.
                    //    }
                    //}
                    await File.WriteAllLinesAsync(resultPath, resultCollection);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cannot transform file: {ex.Message}");
                }
            }

            Console.WriteLine("Press any key for exit...");
            Console.ReadKey();
        }

        static void InteractiveTransform()
        {
            while (true)
            {
                var formula = Console.ReadLine();
                Console.WriteLine(_former.Transform(formula) ?? "Invalid formula");
            }
        }
    }
}
