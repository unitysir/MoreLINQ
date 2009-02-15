namespace MoreLinq.Test.Pull
{
    using System;
    using System.Collections.Generic;
    using MoreLinq.Pull;
    using NUnit.Framework;
    using NUnit.Framework.SyntaxHelpers;

    [TestFixture]
    public class GroupingTest
    {
        private static Tuple<TFirst, TSecond> Tuple<TFirst, TSecond>(TFirst a, TSecond b)
        {
            return new Tuple<TFirst, TSecond>(a, b);
        }

        [Test]
        public void BothSequencesDisposedWithUnequalLengths()
        {
            var longer = DisposeTestingSequence.Of(1, 2, 3);
            var shorter = DisposeTestingSequence.Of(1, 2);

            longer.Zip(shorter, (x, y) => x + y).Exhaust();
            longer.AssertDisposed();
            shorter.AssertDisposed();

            // Just in case it works one way but not the other...
            shorter.Zip(longer, (x, y) => x + y).Exhaust();
            longer.AssertDisposed();
            shorter.AssertDisposed();
        }

        [Test]
        public void ZipWithEqualLengthSequences()
        {
            var zipped = Grouping.Zip(new[] {1, 2, 3}, new[] {4, 5, 6}, (x, y) => Tuple(x, y));
            Assert.That(zipped, Is.Not.Null);
            zipped.AssertSequenceEqual(Tuple(1, 4), Tuple(2, 5), Tuple(3, 6));
        }

        [Test]
        public void ZipWithFirstSequnceShorterThanSecond()
        {
            var zipped = Grouping.Zip(new[] { 1, 2 }, new[] { 4, 5, 6 }, (x, y) => Tuple(x, y));
            Assert.That(zipped, Is.Not.Null);
            zipped.AssertSequenceEqual(Tuple(1, 4), Tuple(2, 5));
        }

        [Test]
        public void ZipWithFirstSequnceLongerThanSecond()
        {
            var zipped = Grouping.Zip(new[] { 1, 2, 3 }, new[] { 4, 5 }, (x, y) => Tuple(x, y));
            Assert.That(zipped, Is.Not.Null);
            zipped.AssertSequenceEqual(Tuple(1, 4), Tuple(2, 5));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ZipWithNullFirstSequence()
        {
            Grouping.Zip(null, new[] { 4, 5, 6 }, BreakingFunc.Of<int, int, int>());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ZipWithNullSecondSequence()
        {
            Grouping.Zip(new[] { 1, 2, 3 }, null, BreakingFunc.Of<int, int, int>());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ZipWithNullResultSelector()
        {
            Grouping.Zip<int, int, int>(new[] { 1, 2, 3 }, new[] { 4, 5, 6 }, null);
        }

        [Test]
        public void ZipIsLazy()
        {
            Grouping.Zip<int, int, int>(
                new BreakingSequence<int>(), 
                new BreakingSequence<int>(), 
                delegate { throw new NotImplementedException(); });
        }
    }

    internal sealed class Tuple<TFirst, TSecond>
    {
        public Tuple(TFirst first, TSecond second) { First = first; Second = second; }

        public TFirst First { get; private set; }
        public TSecond Second { get; private set; }

        public override bool Equals(object value)
        {
            return value != null && Equals(value as Tuple<TFirst, TSecond>);
        }

        private bool Equals(Tuple<TFirst, TSecond> type)
        {
            return type != null
                && EqualityComparer<TFirst>.Default.Equals(type.First, First) 
                && EqualityComparer<TSecond>.Default.Equals(type.Second, Second);
        }

        public override int GetHashCode() { throw new NotImplementedException(); }
        
        public override string ToString()
        {
            return "{ First = " + First + ", Second = " + Second + " }";
        }
    }
}
