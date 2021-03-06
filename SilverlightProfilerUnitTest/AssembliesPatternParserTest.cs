﻿using NUnit.Framework;
using SilverlightProfiler;
using SilverlightProfiler.Filters;

namespace SilverlightProfilerUnitTest
{
    [TestFixture]
    public class AssembliesPatternParserTest
    {
        ConditionPatternParser parser = new ConditionPatternParser();

        [Test]
        public void SingleWordShouldMapRoEqualsCondition()
        {
            Condition condition = parser.Parse("IMD");
            condition.ShouldBeA(typeof(RegexCondition));
            condition.Matches("IMD.Blah.SomethingElse").ShouldBeTrue();
            condition.Matches("IM.Blah.SomethingElse").ShouldBeFalse();
        }

        [Test]
        public void WordsSeparatedByOrOperator()
        {
            Condition condition = parser.Parse("IMD || MContact");
            condition.ShouldBeA(typeof(OrCondition));
            condition.Matches("IMD.Blah.SomethingElse").ShouldBeTrue();
            condition.Matches("MContact.Blah.SomethingElse").ShouldBeTrue();
            condition.Matches("MConta.Blah.SomethingElse").ShouldBeFalse();
        }

        [Test]
        public void NotOperator()
        {
            Condition condition = parser.Parse("!IMD");
            condition.ShouldBeA(typeof(NotCondition));
            condition.Matches("ABC").ShouldBeTrue();
            condition.Matches("IMD").ShouldBeFalse();
        }
    }
}