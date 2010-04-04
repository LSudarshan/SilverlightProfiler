using System;
using System.Collections.Generic;
using System.Linq;
using NMetrics.Filters;

namespace SilverlightProfilerUnitTest
{
    public class AssembliesPatternParser
    {
        public Condition Parse(string text)
        {
            if(!text.Contains("("))
                return ParseAtomicCondition(text);
            return null;
        }

        private Condition ParseAtomicCondition(string text)
        {
            if(text.Contains("||"))
            {
                return ParseAtomicOrCcondition(text);
            }
            return Condition.Eq(text);
        }

        private Condition ParseAtomicOrCcondition(string text)
        {
            List<string> orArguments = text.Split(new[]{"||"}, StringSplitOptions.RemoveEmptyEntries).ToList();
            Condition condition = Condition.Eq(orArguments[0].Trim());
            orArguments.RemoveAt(0);
            orArguments.ForEach(arg => condition = condition.Or(Condition.Eq(arg.Trim())));
            return condition;
        }
    }
}