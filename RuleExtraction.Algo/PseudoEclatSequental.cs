using RuleExtraction.Algo.Dataset;
using RuleExtraction.Algo.Structures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace RuleExtraction.Algo
{
    public class PseudoEclatSequental : IRulesExtractor
    {
        private readonly int countRows;
        private readonly int countColumns;
        private readonly WorkingSet dataset;

        private readonly Dictionary<BitSet, HashSet<int>> tidList;
        private CancellationToken token;
        private double minSupport;
        private readonly List<Rule> rules;

        public event EventHandler<ProgressChangedArgs> ProgressReportEvent;

        public PseudoEclatSequental(WorkingSet dataset)
        {
            rules = new List<Rule>();
            this.dataset = dataset;
            countRows = dataset.countRows;
            countColumns = dataset.tidList.Count;
            tidList = new Dictionary<BitSet, HashSet<int>>();
        }

        public List<Rule> StartProcess(double minSupport, CancellationToken token = new CancellationToken())
        {
            tidList.Clear();
            rules.Clear();
            this.token = token;
            this.minSupport = minSupport * countRows;
            foreach (var item in dataset.tidList)
            {
                double support = item.Value.Count;
                if (support >= minSupport)
                    tidList.Add(item.Key, item.Value);
            }
            Process();
            return rules;
        }
        private void Process()
        {
            var toAddDict = new Dictionary<BitSet, HashSet<int>>();
            do
            {
                toAddDict.Clear();
                var top = tidList.GetEnumerator();
                for (int i = 0; i < tidList.Count; i++)
                {
                    top.MoveNext();
                    var item = top.Current;

                    Debug.Assert(item.Key != null, "Top out");
                    var key = new BitSet(countColumns);
                    var inner = tidList.GetEnumerator();
                    for (int j = 0; j < i; j++)
                        inner.MoveNext();
                    for (int j = i; j < tidList.Count; j++)
                    {
                        inner.MoveNext();
                        var value = inner.Current;
                        Debug.Assert(value.Key != null, "Inner out");
                        key.SetFrom(item.Key);
                        key.Or(value.Key);

                        if (key.Equals(item.Key)
                            || key.Equals(value.Key)
                            || tidList.ContainsKey(key)
                            || toAddDict.ContainsKey(key))
                            continue;

                        var hashSet = new HashSet<int>(item.Value);
                        hashSet.IntersectWith(value.Value);

                        double support = hashSet.Count;
                        if (support < minSupport)
                            continue;
                        var keyNew = new BitSet(key);
                        toAddDict.Add(keyNew, hashSet);
                        rules.Add(new Rule(keyNew, support));
                    }
                }
                foreach (var item in toAddDict)
                    tidList.Add(item.Key, item.Value);

                if (token.IsCancellationRequested) return;
                ProgressReportEvent?.Invoke(this, new ProgressChangedArgs(1));
            } while (toAddDict.Count != 0);
        }
    }
}
