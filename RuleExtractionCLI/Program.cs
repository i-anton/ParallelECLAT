using RuleExtraction.Algo;
using RuleExtraction.Algo.Dataset;
using System;
using System.Diagnostics;

namespace RuleExtraction.CLI
{
    class Program
    {
        private static void Measure(IRulesExtractor extractor, double percent)
        {
            Console.WriteLine(extractor.ToString());
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            var results1 = extractor.StartProcess(percent);
            sw.Stop();
            Console.WriteLine($"Time: {sw.ElapsedMilliseconds}");
            Console.WriteLine($"Item.Count: {results1.Count}");
            GC.Collect();
        }
        static void Main(string[] args)
        {
            double percent = 0.15;
            var dataset = ArffParser.Parse("supermarket.mod.arff");
            for (int i = 0; i < 10; i++)
            {
                Measure(new TrueEclatSequental(dataset),percent);
                Measure(new TrueEclatParallel(dataset), percent);
            }
            Console.ReadKey();
        }
    }
}
