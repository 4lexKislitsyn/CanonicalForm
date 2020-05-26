using CanonicalForm.Core;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanonicalForm.ConsoleApp
{
    class Program
    {
        private static ObjectPool<StringBuilder> _stringBuilderPool;
        static CanonicalFormulaFormer _former;

        static async Task Main(string[] args)
        {
            var poolProvider = new DefaultObjectPoolProvider();
            _stringBuilderPool = poolProvider.CreateStringBuilderPool();
            _former = new CanonicalFormulaFormer(new RegexGroupSearcher(), new GroupsDictionaryBuilder(), new GroupsDictionaryRenderer(_stringBuilderPool));
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
            Console.WriteLine("File processing starts...");
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
                        MaxDegreeOfParallelism = lines.Length / 3500
                    };

                    ConcurrentBag<string> resultCollection = new ConcurrentBag<string>();
                    var parallelResult = Parallel.ForEach(lines, parallelOptions, (formula, state) => resultCollection.Add(_former.Transform(formula)));
                    if (!parallelResult.IsCompleted)
                    {
                        var stringBuilder = _stringBuilderPool.Get();
                        stringBuilder.Append("Cannot transform file");
                        if (parallelResult.LowestBreakIteration.HasValue && lines.Length > parallelResult.LowestBreakIteration.Value)
                        {
                            stringBuilder.Append("Maybe problem in ").Append(lines[parallelResult.LowestBreakIteration.Value]);
                        }
                        Console.WriteLine(stringBuilder.ToString());
                        _stringBuilderPool.Return(stringBuilder);
                        continue;
                    }
                    var resultPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".out");
                    await File.WriteAllLinesAsync(resultPath, resultCollection, Encoding.UTF8);
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
                Console.WriteLine("Enter formula:");
                var formula = Console.ReadLine();
                Console.WriteLine(_former.Transform(formula) ?? "Invalid formula");
            }
        }
    }
}
