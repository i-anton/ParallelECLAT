using RuleExtraction.Algo.Structures;
using System;
using System.Collections.Generic;
using System.IO;

namespace RuleExtraction.Algo.Dataset
{
    public class ArffParser
    {
        public static WorkingSet Parse(string filename,
            int colLimit = 0, int rowLimit = 0)
        {
            rowLimit = (rowLimit == 0) ? int.MaxValue : rowLimit;
            colLimit = (colLimit == 0) ? int.MaxValue : colLimit;

            var tidList = new Dictionary<BitSet, HashSet<int>>();
            var columnNames = new List<string>();
            var relationName = string.Empty;
            int countRows = -1;
            using (var reader = File.OpenText(filename))
            {
                string line;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    var split = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length == 0)
                        continue;
                    switch (split[0])
                    {
                        case "@relation":
                            if (split.Length > 1)
                                relationName = split[1];
                            break;
                        case "@attribute":
                            if (split.Length > 1)
                                columnNames.Add(split[1]);
                            break;
                        case "@data":
                            countRows = ParseData(tidList, columnNames, reader, colLimit, rowLimit);
                            break;
                        default:
                            break;
                    }
                }
            }
            return new WorkingSet(tidList, countRows, columnNames, relationName);
        }

        private static int ParseData(Dictionary<BitSet, HashSet<int>> tidList,
            List<string> columnNames, StreamReader reader, int neededCountCol, int rowLimit)
        {
            while (neededCountCol < columnNames.Count)
                columnNames.RemoveAt(columnNames.Count - 1);
            var bits = columnNames.Count;
            var tidListArr = new HashSet<int>[bits];
            for (int i = 0; i < bits; i++)
                tidListArr[i] = new HashSet<int>();
            var rowId = 0;
            while (!reader.EndOfStream && rowId <= rowLimit)
            {
                string line = reader.ReadLine();
                var dataSplit = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (dataSplit.Length == 0) continue;
                var isAdded = false;
                for (int i = 0; i < bits; i++)
                    if (dataSplit[i] != "0")
                    {
                        isAdded = true;
                        tidListArr[i].Add(rowId);
                    }
                if (isAdded) ++rowId;
            }
            for (int i = 0; i < bits; i++)
            {
                var key = new BitSet(bits);
                key.Set(i, true);
                tidList.Add(key, tidListArr[i]);
            }
            return rowId;
        }
    }
}
