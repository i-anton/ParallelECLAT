using RuleExtraction.Algo.Structures;
using System.Collections.Generic;

namespace RuleExtraction.Algo.Dataset
{
    public class WorkingSet
    {
        public readonly Dictionary<BitSet, HashSet<int>> tidList;
        public readonly List<string> columnNames;
        public readonly string relationName;
        public readonly int countRows;

        public WorkingSet(Dictionary<BitSet, HashSet<int>> tidList,
            int countRows, List<string> columnNames = null,
            string relationName = null)
        {
            this.tidList = tidList;
            this.countRows = countRows;
            this.columnNames = columnNames;
            this.relationName = relationName;
            if (this.columnNames == null) columnNames = new List<string>();
            if (this.relationName == null) relationName = string.Empty;
        }
    }
}
