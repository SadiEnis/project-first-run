using NUnit.Framework;
using ProjectFirstRun.Rewards;

namespace ProjectFirstRun.Tests.EditMode.Rewards
{
    public sealed class UnityRandomSourceTests
    {
        [Test]
        public void Next_WithValidRange_ReturnsValueInsideRange()
        {
            UnityRandomSource randomSource =
                new UnityRandomSource();

            int result =
                randomSource.Next(
                    10,
                    20);

            Assert.That(
                result,
                Is.GreaterThanOrEqualTo(10));

            Assert.That(
                result,
                Is.LessThan(20));
        }

        [Test]
        public void Next_WithEmptyRange_Throws()
        {
            UnityRandomSource randomSource =
                new UnityRandomSource();

            Assert.That(
                () =>
                    randomSource.Next(
                        10,
                        10),
                Throws.TypeOf<
                    System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Next_WithReversedRange_Throws()
        {
            UnityRandomSource randomSource =
                new UnityRandomSource();

            Assert.That(
                () =>
                    randomSource.Next(
                        20,
                        10),
                Throws.TypeOf<
                    System.ArgumentOutOfRangeException>());
        }
    }
}