using System.Collections.Generic;
using System.Text;

namespace RuleExtraction.Algo.Structures
{
    public class Rule
    {
        public Rule(BitSet columns, double support)
        {
            Columns = columns;
            Support = support;
        }

        public BitSet Columns { get; private set; }

        public string ToStringNamedColumns(List<string> columnNames)
        {
            var sb = new StringBuilder();
            sb.Append($"Support: {Support}| ");
            for (int i = 0; i < columnNames.Count; i++)
                if (Columns[i]) sb.Append(columnNames[i]).Append("| ");
            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is Rule rule)
                return Columns.Equals(rule.Columns);
            return false;
        }

        public override int GetHashCode() => Columns.GetHashCode();

        public static int CompareBySupport(Rule r1, Rule r2) => r1.Support.CompareTo(r2.Support);

        public double Support { get; private set; }
        public override string ToString() => $"Support:{Support};Cols:{Columns}";
    }
}
