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
                rawArgs: new[] { "rawArg1", "rawArg2" });

            Assert.That(line.RawLine, Is.EqualTo("rawLine"));
            Assert.That(line.RawTimeStamp, Is.EqualTo("rawTimeStamp"));
            Assert.That(line.RawDb, Is.EqualTo("rawDb"));
            Assert.That(line.RawCommand, Is.EqualTo("rawCommand"));
            Assert.That(line.RawArgs.Length, Is.EqualTo(2));
            Assert.That(line.RawArgs[0], Is.EqualTo("rawArg1"));
            Assert.That(line.RawArgs[1], Is.EqualTo("rawArg2"));
        }
    }
}