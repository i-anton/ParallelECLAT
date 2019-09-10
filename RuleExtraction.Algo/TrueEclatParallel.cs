using RuleExtraction.Algo.Dataset;
using RuleExtraction.Algo.Structures;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RuleExtraction.Algo
{
    public class TrueEclatParallel : IRulesExtractor
    {
        private readonly int countRows;
        private readonly int countColumns;
        private readonly WorkingSet dataset;

        private BitSet[] tidArr;
        private HashSet<int>[] tidHashArr;
        private double minSupport;
        private readonly ConcurrentDictionary<BitSet, double> rules;
        private CancellationToken token;

        public event EventHandler<ProgressChangedArgs> ProgressReportEvent;

        public TrueEclatParallel(WorkingSet dataset)
        {
            rules = new ConcurrentDictionary<BitSet, double>();
            this.dataset = dataset;
            countRows = dataset.countRows;
            countColumns = dataset.tidList.Count;
        }

        public List<Rule> StartProcess(double minSupport, CancellationToken token = new CancellationToken())
        {
            rules.Clear();
            this.token = token;
            this.minSupport = minSupport * dataset.countRows;
            var tidList = new Dictionary<BitSet, HashSet<int>>();
            foreach (var item in dataset.tidList)
                if (item.Value.Count >= minSupport)
                    tidList.Add(item.Key, item.Value);
            tidArr = new BitSet[tidList.Count];
            tidHashArr = new HashSet<int>[tidList.Count];
            int itemTid = 0;
            foreach (var item in tidList)
            {
                tidArr[itemTid] = item.Key;
                tidHashArr[itemTid] = item.Value;
                itemTid++;
            }
            Parallel.For(0, tidArr.Length,
                (item, st) =>
                {
                    Process(tidArr[item], tidHashArr[item], item);
                    if (this.token.IsCancellationRequested)
                        st.Break();
                    ProgressReportEvent?.Invoke(this, new ProgressChangedArgs(1));
                });
            var list = new List<Rule>();
            foreach (var item in rules)
                list.Add(new Rule(item.Key, item.Value / countRows));
            return list;
        }
        private void Process(BitSet nodeKey, HashSet<int> nodeValue, int startIdx)
        {
            var key = new BitSet(countColumns);
            for (int i = startIdx + 1; i < tidArr.Length; i++)
            {
                key.SetFrom(nodeKey);
                key.Or(tidArr[i]);
                if (key.Equals(nodeKey) || key.Equals(tidArr[i]))
                    continue;
                var hashSet = new HashSet<int>(nodeValue);
                hashSet.IntersectWith(tidHashArr[i]);
                double support = hashSet.Count;
                if (support < minSupport)
                    continue;
                var keyNew = new BitSet(key);
                if (!rules.TryAdd(keyNew, support))
                    continue;
                if (token.IsCancellationRequested)
                    return;
                Process(keyNew, hashSet, i);
            }
        }
    }
}
