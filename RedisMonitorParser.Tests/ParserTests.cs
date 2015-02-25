namespace RedisMonitorParser.Tests
{
    using NUnit.Framework;

    /// <summary>
    /// Tests for the parser.
    /// </summary>
    [TestFixture]
    public sealed class ParserTests
    {
        private static readonly ParserConfiguration defaultParserConfiguration = ParserConfiguration.Default;

        /// <summary>
        /// Expectations for results of parsing when using default config.
        /// </summary>
        public ParseExpectation[] ParseRawExpectations_DefaultConfig =
        {
            // 1424875771.497401 [0 [fe80::]:11759] "GET" "FOOBAR"

            // The client uses IPv6 address, make sure this gets parsed OK too.
            new ParseExpectation(
                name: "IPv6 client address - local", 
                parserConfiguration: defaultParserConfiguration,
                input: "1424875375.201784 [0 [::]:11707] \"GET\" \"FOOBAR\"", 
                expectedResult: new RawMonitorLine(
                    rawLine: "1424875375.201784 [0 [::]:11707] \"GET\" \"FOOBAR\"", 
                    rawTimeStamp: "1424875375.201784", 
                    rawDb: "0", 
                    rawCommand: "\"GET\"", 
                    rawArgs: "\"FOOBAR\"")), 

            // The client uses non-local IPv6 address, make sure this gets parsed OK too.
            new ParseExpectation(
                name: "IPv6 client address - non-local", 
                parserConfiguration: defaultParserConfiguration,
                input: "1424875375.201784 [0 [fe80::]:11707] \"GET\" \"FOOBAR\"", 
                expectedResult: new RawMonitorLine(
                    rawLine: "1424875375.201784 [0 [fe80::]:11707] \"GET\" \"FOOBAR\"", 
                    rawTimeStamp: "1424875375.201784", 
                    rawDb: "0", 
                    rawCommand: "\"GET\"", 
                    rawArgs: "\"FOOBAR\"")), 

            // Command, no args
            new ParseExpectation(
                name: "Command, no args", 
                parserConfiguration: defaultParserConfiguration,
                input: "1424186960.476348 [0 127.0.0.1:60475] \"EXEC\"", 
                expectedResult: new RawMonitorLine(
                    rawLine: "1424186960.476348 [0 127.0.0.1:60475] \"EXEC\"", 
                    rawTimeStamp: "1424186960.476348", 
                    rawDb: "0", 
                    rawCommand: "\"EXEC\"", 
                    rawArgs: string.Empty)), 

            // Command, 1 argument
            new ParseExpectation(
                name: "Command, 1 arg", 
                parserConfiguration: defaultParserConfiguration,
                input: "1424186960.663817 [0 127.0.0.1:60475] \"GET\" \"KEY1\"", 
                expectedResult: new RawMonitorLine(
                    rawLine: "1424186960.663817 [0 127.0.0.1:60475] \"GET\" \"KEY1\"", 
                    rawTimeStamp: "1424186960.663817", 
                    rawDb: "0", 
                    rawCommand: "\"GET\"", 
                    rawArgs: "\"KEY1\"")), 

            // Command, many arguments
            new ParseExpectation(
                name: "Command, many args", 
                parserConfiguration: defaultParserConfiguration,
                input: "1424186960.663817 [0 127.0.0.1:60475] \"MGET\" \"KEY1\" \"KEY2\" \"KEY3\"", 
                expectedResult: new RawMonitorLine(
                    rawLine: "1424186960.663817 [0 127.0.0.1:60475] \"MGET\" \"KEY1\" \"KEY2\" \"KEY3\"", 
                    rawTimeStamp: "1424186960.663817", 
                    rawDb: "0", 
                    rawCommand: "\"MGET\"", 
                    rawArgs: "\"KEY1\" \"KEY2\" \"KEY3\"")), 

            // Make sure the case is preserved as per the original input
            new ParseExpectation(
                name: "Command, many args, mixed case", 
                parserConfiguration: defaultParserConfiguration,
                input: "1424186960.663817 [0 127.0.0.1:60475] \"MgEt\" \"KeY1\" \"KeY2\" \"KeY3\"", 
                expectedResult: new RawMonitorLine(
                    rawLine: "1424186960.663817 [0 127.0.0.1:60475] \"MgEt\" \"KeY1\" \"KeY2\" \"KeY3\"", 
                    rawTimeStamp: "1424186960.663817", 
                    rawDb: "0", 
                    rawCommand: "\"MgEt\"", 
                    rawArgs: "\"KeY1\" \"KeY2\" \"KeY3\"")), 
        };

        /// <summary>
        /// All sorts of malformed monitor lines.
        /// </summary>
        public ParseExpectation[] ParseRawExpectations_Malformed_DefaultConfig =
        {
            new ParseExpectation(
                name: "Just timestamp",
                parserConfiguration: defaultParserConfiguration,
                input: "1424186960.663817",
                expectedResult: null),
            new ParseExpectation(
                name: "Just timestamp, db, and address",
                parserConfiguration: defaultParserConfiguration,
                input: "1424186960.663817 [0 127.0.0.1:60475]",
                expectedResult: null),

            // We expect quotes around command and the args. This case is when they are missing.
            new ParseExpectation(
                name: "Missing quotes",
                parserConfiguration: defaultParserConfiguration,
                input: "1424186960.663817 [0 127.0.0.1:60475] GET KEY1",
                expectedResult: null),

            // Non-command. We sometimes get these
            new ParseExpectation(
                name: "Non-command", 
                parserConfiguration: defaultParserConfiguration,
                input: "OK", 
                expectedResult: null), 
        };

        [Test]
        [TestCaseSource("ParseRawExpectations_DefaultConfig")]
        public void DefaultConfig_ParseRaw_CorrectLines(ParseExpectation expectation)
        {
            var expected = expectation.ExpectedResult;

            var parser = new Parser(expectation.ParserConfiguration);
            var parsed = parser.ParseRaw(expectation.Input);

            Assert.That(parsed, Is.Not.Null);
            Assert.That(parsed.RawArgs, Is.EqualTo(expected.RawArgs));
            Assert.That(parsed.RawCommand, Is.EqualTo(expected.RawCommand));
            Assert.That(parsed.RawDb, Is.EqualTo(expected.RawDb));
            Assert.That(parsed.RawLine, Is.EqualTo(expected.RawLine));
            Assert.That(parsed.RawTimeStamp, Is.EqualTo(expected.RawTimeStamp));
        }

        [Test]
        [TestCaseSource("ParseRawExpectations_Malformed_DefaultConfig")]
        public void DefaultConfig_ParseRaw_MalformedLines(ParseExpectation expectation)
        {
            var expected = expectation.ExpectedResult;

            var parser = new Parser(expectation.ParserConfiguration);
            var parsed = parser.ParseRaw(expectation.Input);
            Assert.That(parsed, Is.Null);
        }


        public sealed class ParseExpectation
        {
            public ParseExpectation(string name, ParserConfiguration parserConfiguration, string input, RawMonitorLine expectedResult)
            {
                this.ParserConfiguration = parserConfiguration;
                this.Name = name;
                this.Input = input;
                this.ExpectedResult = expectedResult;
            }

            public ParserConfiguration ParserConfiguration { get; private set; }

            public string Name { get; private set; }

            public string Input { get; private set; }

            public RawMonitorLine ExpectedResult { get; private set; }

            public override string ToString()
            {
                return this.Name;
            }
        }
    }
}