using System;
using System.Text.RegularExpressions;

namespace NMetrics.Filters
{
    public abstract class Condition
    {
        public abstract bool Matches(string text);

        public Condition And(Condition condition)
        {
            return new AndCondition(this, condition);
        }

        public Condition Or(Condition condition)
        {
            return new OrCondition(this, condition);
        }

        public Condition Not()
        {
            return new NotCondition(this);
        }

        public static Condition Eq(string pattern)
        {
            return new RegexCondition(pattern);
        }
    }

    public class NotCondition : Condition
    {
        private readonly Condition condition;

        public NotCondition(Condition condition)
        {
            this.condition = condition;
        }

        public override bool Matches(string text)
        {
            return !condition.Matches(text);
        }
    }

    public class OrCondition : Condition
    {
        private readonly Condition left;
        private readonly Condition right;

        public OrCondition(Condition left, Condition right)
        {
            this.left = left;
            this.right = right;
        }

        public override bool Matches(string text)
        {
            return left.Matches(text) || right.Matches(text);
        }
    }

    public class AndCondition : Condition
    {
        private readonly Condition left;
        private readonly Condition right;

        public AndCondition(Condition left, Condition right)
        {
            this.left = left;
            this.right = right;
        }

        public override bool Matches(string text)
        {
            return left.Matches(text) && right.Matches(text);
        }
    }

    public class RegexCondition : Condition
    {
        private readonly string pattern;

        public RegexCondition(string pattern)
        {
            this.pattern = pattern;
        }

        public override bool Matches(string text)
        {
            return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
        }
    }
}