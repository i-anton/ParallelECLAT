using RuleExtraction.Algo.Structures;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RuleExtraction.Algo
{
    public class ProgressChangedArgs : EventArgs
    {
        public int ProgressAdd { get; private set; }
        public ProgressChangedArgs(int increment) => ProgressAdd = increment;
    }

    public interface IRulesExtractor
    {
        List<Rule> StartProcess(double minSupport, CancellationToken token = new CancellationToken());
        event EventHandler<ProgressChangedArgs> ProgressReportEvent;
    }
}
