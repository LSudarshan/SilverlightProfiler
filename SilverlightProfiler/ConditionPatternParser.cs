using System;
using System.Collections.Generic;
using System.Linq;
using SilverlightProfiler.Filters;

namespace SilverlightProfiler
{
    public class ConditionPatternParser
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
            } else if(text.Contains("!"))
            {
                return ParseAtomicNotCcondition(text);
            }
            return Condition.Eq(text);
        }

        private Condition ParseAtomicNotCcondition(string text)
        {
            string textWithNot = text.Trim().Remove(0, 1);
            return Condition.Eq(textWithNot).Not();
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