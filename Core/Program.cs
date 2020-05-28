using CanonicalForm.Core;
using CanonicalForm.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        private static IServiceProvider _provider;

        static async Task Main(string[] args)
        {
            _provider = ConfigureDependencyInjection();

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
            var former = _provider.GetRequiredService<CanonicalFormulaFormer>();
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
                    var parallelResult = Parallel.ForEach(lines, parallelOptions, (formula, state) => resultCollection.Add(former.Transform(formula)));
                    if (!parallelResult.IsCompleted)
                    {
                        var pool = _provider.GetRequiredService<ObjectPool<StringBuilder>>();
                        var stringBuilder = pool.Get();
                        stringBuilder.Append("Cannot transform file");
                        if (parallelResult.LowestBreakIteration.HasValue && lines.Length > parallelResult.LowestBreakIteration.Value)
                        {
                            stringBuilder.Append("Maybe problem in ").Append(lines[parallelResult.LowestBreakIteration.Value]);
                        }
                        Console.WriteLine(stringBuilder.ToString());
                        pool.Return(stringBuilder);
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
            var former = _provider.GetRequiredService<CanonicalFormulaFormer>();
            while (true)
            {
                Console.WriteLine("Enter formula:");
                var formula = Console.ReadLine();
                Console.WriteLine(former.Transform(formula) ?? "Invalid formula");
            }
        }

        static IServiceProvider ConfigureDependencyInjection()
        {
            var services = new ServiceCollection();

            services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.TryAddSingleton<ObjectPool<StringBuilder>>(provider =>
            {
                var poolProvider = provider.GetRequiredService<ObjectPoolProvider>();
                return poolProvider.Create(new StringBuilderPooledObjectPolicy());
            });

            services.AddTransient<IGroupsSearcher, CompositeRegexGroupSearcher>();
            services.AddTransient<IGroupsDictionaryBuilder, GroupsDictionaryBuilder>();
            services.AddTransient<IGroupsRenderer, GroupsDictionaryRenderer>();

            services.AddSingleton<CanonicalFormulaFormer>();

            return services.BuildServiceProvider(true);
        }
    }
}
