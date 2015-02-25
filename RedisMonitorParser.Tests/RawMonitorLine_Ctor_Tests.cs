namespace RedisMonitorParser.Tests
{
    using NUnit.Framework;

    /// <summary>
    /// Basic unit tests for <see cref="RawMonitorLine"/>
    /// </summary>
    [TestFixture]
    public sealed class RawMonitorLineTests
    {
        /// <summary>
        /// Verifies that all properties are set by the constructor.
        /// </summary>
        [Test]
        public void Ctor_AllPropertiesGetSet()
        {
            var line = new RawMonitorLine(
                rawLine: "rawLine",
                rawTimeStamp: "rawTimeStamp",
                rawDb: "rawDb",
                rawCommand: "rawCommand",
                rawArgs: "rawArgs");

            Assert.That(line.RawLine, Is.EqualTo("rawLine"));
            Assert.That(line.RawTimeStamp, Is.EqualTo("rawTimeStamp"));
            Assert.That(line.RawDb, Is.EqualTo("rawDb"));
            Assert.That(line.RawCommand, Is.EqualTo("rawCommand"));
            Assert.That(line.RawArgs, Is.EqualTo("rawArgs"));
        }
    }
}