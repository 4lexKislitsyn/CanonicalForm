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
using System.Threading;
using System.Threading.Tasks;

namespace CanonicalForm.ConsoleApp
{
    class Program
    {
        private const int OptimalLinesPerTask = 3500;
        private static IServiceProvider _provider;
        private static CancellationTokenSource _cancelationTokenSource;

        static async Task Main(string[] args)
        {
            _provider = ConfigureDependencyInjection();

            if (args.Length > 0)
            {
                _cancelationTokenSource = new CancellationTokenSource();
                Console.CancelKeyPress += Console_CancelKeyPress;
                await TransformFiles(args);
            }
            else
            {
                InteractiveTransform();
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _cancelationTokenSource.Cancel();
        }

        private static async Task TransformFiles(string[] fileNames)
        {
            Console.WriteLine("File processing starts...");
            var former = _provider.GetRequiredService<CanonicalFormulaFormer>();
            foreach (var path in fileNames.Where(x=> File.Exists(x)))
            {
                try
                {
                    var lines = await File.ReadAllLinesAsync(path, _cancelationTokenSource.Token);
                    if (lines.Length == 0)
                    {
                        Console.WriteLine($"File '{path}' is empty.");
                        continue;
                    }

                    var parallelOptions = new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = Math.Max(lines.Length / OptimalLinesPerTask, 1),
                        CancellationToken = _cancelationTokenSource.Token
                    };

                    var resultCollection = new string[lines.Length];
                    var parallelResult = Parallel.ForEach(lines, parallelOptions, (formula, state, index) => 
                    {
                        try
                        {
                            resultCollection[index] = former.Transform(formula) ?? "Invalid formula";
                        }
                        catch (InvalidFormulaException ex)
                        {
                            resultCollection[index] = ex.Message;
                        }
                        catch (Exception ex)
                        {
                            resultCollection[index] = $"Cannot operate formula: {ex.Message}";
                        }
                    });
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
                    await File.WriteAllLinesAsync(resultPath, resultCollection, Encoding.UTF8, _cancelationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine($"Files operating was canceled.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cannot transform file: {ex.Message}");
                }
            }

            if (!_cancelationTokenSource.IsCancellationRequested)
            {
                Console.WriteLine("Press any key for exit...");
                Console.ReadKey();
            }
        }

        private static void InteractiveTransform()
        {
            var former = _provider.GetRequiredService<CanonicalFormulaFormer>();
            while (true)
            {
                Console.WriteLine("Enter formula:");
                var formula = Console.ReadLine();
                try
                {
                    var transfromed = former.Transform(formula, false);
                    Console.WriteLine(transfromed);
                }
                catch (InvalidFormulaException ex)
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ForegroundColor = color;
                }
            }
        }

        private static IServiceProvider ConfigureDependencyInjection()
        {
            var services = new ServiceCollection();

            services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.TryAddSingleton<ObjectPool<StringBuilder>>(provider =>
            {
                var poolProvider = provider.GetRequiredService<ObjectPoolProvider>();
                return poolProvider.Create(new StringBuilderPooledObjectPolicy());
            });

            services.AddSingleton<IVariableExpressionFactory, RegexVariableExpressionFactory>();
            services.AddTransient<IExpressionSearcher, ReversePolishSearcher>();
            services.AddTransient<IExpressionsRenderer, GroupsRenderer>();

            services.AddSingleton<CanonicalFormulaFormer>();

            return services.BuildServiceProvider(true);
        }
    }
}
