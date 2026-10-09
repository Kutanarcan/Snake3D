using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class ResultTests
    {
        [Test]
        public void ASuccessCarriesNoError()
        {
            Result result = Result.Success();

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.Null);
        }

        [Test]
        public void AFailureCarriesItsReason()
        {
            Result result = Result.Failure("The snake would start outside the grid.");

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo("The snake would start outside the grid."));
        }

        [Test]
        public void AFailureWithoutAReasonIsRejected()
        {
            Assert.Throws<ArgumentException>(() => Result.Failure(null));
            Assert.Throws<ArgumentException>(() => Result.Failure(string.Empty));
            Assert.Throws<ArgumentException>(() => Result.Failure("   "));
        }
    }
}
