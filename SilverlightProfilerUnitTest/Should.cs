using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace SilverlightProfilerUnitTest
{
    public static class Should
    {
        public delegate void AssertDelegate<T>(T actual, T expected);

        public static void ShouldNotBe<T>(this T actual, T notExpected)
        {
            Assert.That(actual, Is.Not.EqualTo(notExpected));
        }

        public static void ShouldBeOfSize(this IEnumerable actual, int expected)
        {
            int count = 0;
            foreach (object item in actual)
                ++count;
            Assert.That(count, Is.EqualTo(expected), "The count was wrong!!");
        }

        public static void ShouldBeEmpty(this IEnumerable actual)
        {
            Assert.That(actual, Is.Empty);
        }

        public static void ShouldNotBeEmpty(this IEnumerable actual)
        {
            Assert.That(actual, Is.Not.Empty);
        }

        public static void ShouldBeA<T>(this T actual, Type expected)
        {
            Assert.That(actual, Is.InstanceOfType(expected));
        }

        public static void ShouldBe<T>(this T actual, T expected)
        {
            Assert.AreEqual(expected, actual);
        }

        public static void ShouldBe<T>(this T actual, T expected, AssertDelegate<T> assertion)
        {
            assertion(actual, expected);
        }

        public static void ShouldBe(this DateTime actual, DateTime expected, int minutes)
        {
            Assert.That(actual, Is.EqualTo(expected).Within(TimeSpan.FromMinutes(minutes)));
        }

        public static void ShouldBeSameAs<T>(this T actual, T expected)
        {
            Assert.That(actual, Is.SameAs(expected));
        }

        public static void ShouldNotBeSameAs<T>(this T actual, T expected)
        {
            Assert.That(actual, Is.Not.SameAs(expected));
        }

        public static void ShouldBeTrue(this bool actual)
        {
            Assert.AreEqual(true, actual);
        }

        public static void ShouldBeFalse(this bool actual)
        {
            Assert.AreEqual(false, actual);
        }

        public static void ShouldBeNull(this object actual)
        {
            Assert.AreEqual(null, actual);
        }

        public static void ShouldBeNullOrEmpty(this string actual)
        {
            Assert.IsTrue(string.IsNullOrEmpty(actual), string.Format("Not Empty! Value is {0}", actual));
        }

        public static void ShouldNotBeNull(this object actual)
        {
            ShouldNotBeNull(actual, "");
        }

        public static void ShouldNotBeNull(this object actual, object message)
        {
            Assert.AreNotEqual(null, actual, message.ToString());
        }

        public static void ShouldContain<T>(this IEnumerable<T> collection, T expected)
        {
            Assert.That(collection, Has.Member(expected));
        }

        public static void ShouldNotContain<T>(this IEnumerable<T> collection, T expected)
        {
            Assert.That(collection, Has.No.Member(expected));
        }

        public static void ShouldContain<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            ShouldContain(collection, predicate, "Couldn't find anything which matched the condition!!");
        }

        public static void ShouldContain<T>(this IEnumerable<T> collection, Func<T, bool> predicate, object message)
        {
            T firstOrDefault = collection.FirstOrDefault(predicate);
            Assert.That(firstOrDefault, Is.Not.EqualTo(default(T)), message.ToString());
        }

        public static void ShouldNotContain<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            T firstOrDefault = collection.FirstOrDefault(predicate);
            Assert.That(firstOrDefault, Is.EqualTo(default(T)),
                        "Shouldn't have contained " + firstOrDefault + " but it did!!");
        }

        public static void ShouldNotContain<T>(this ICollection<T> collection, T expected)
        {
            Assert.That(collection, Has.No.Member(expected));
        }

        public static void ShouldContain(this string actual, string expected)
        {
            Assert.That(actual, Text.Contains(expected));
        }

        public static void ShouldBeGreaterThan<T>(this IComparable<T> actual, T expected)
        {
            Assert.That(actual.CompareTo(expected) > 0);
        }

        public static void ShouldBeLessThan<T>(this IComparable<T> actual, T expected)
        {
            Assert.That(actual.CompareTo(expected) < 0);
        }

        public static void ShouldNotContain(this string actual, string expected)
        {
            Assert.That(actual, !Text.Contains(expected));
        }

        public static void ShouldBeIgnoringCase(this string actual, string expected)
        {
            actual.ToLower().ShouldBe(expected.ToLower());
        }
    }
}