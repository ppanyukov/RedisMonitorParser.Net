namespace RedisMonitorParser.Tests
{
    using NUnit.Framework;

    /// <summary>
    /// Tests for the parser.
    /// </summary>
    [TestFixture]
    public sealed class ParserTests
    {
        /// <summary>
        /// Expectations for results of parsing when using default config.
        /// </summary>
        public static ParseExpectation[] ParseRawExpectations_DefaultConfig()
        {
            return new[]
            {
                // The client uses IPv6 address, make sure this gets parsed OK too.
                new ParseExpectation(
                    name: "IPv6 client address - local", 
                    input: @"1424875375.201784 [0 [::]:11707] ""GET"" ""FOOBAR""", 
                    expectedResult: new RawMonitorLine(
                        rawLine: @"1424875375.201784 [0 [::]:11707] ""GET"" ""FOOBAR""", 
                        rawTimeStamp: "1424875375.201784", 
                        rawDb: "0", 
                        rawCommand: "GET", 
                        rawArgs: new[] { "FOOBAR" })), 

                // The client uses non-local IPv6 address, make sure this gets parsed OK too.
                new ParseExpectation(
                    name: "IPv6 client address - non-local", 
                    input: @"1424875375.201784 [0 [fe80::]:11707] ""GET"" ""FOOBAR""", 
                    expectedResult: new RawMonitorLine(
                        rawLine: @"1424875375.201784 [0 [fe80::]:11707] ""GET"" ""FOOBAR""", 
                        rawTimeStamp: "1424875375.201784", 
                        rawDb: "0", 
                        rawCommand: "GET", 
                        rawArgs: new[] { "FOOBAR" })), 

                // Command, no args
                new ParseExpectation(
                    name: "Command, no args", 
                    input: @"1424186960.476348 [0 127.0.0.1:60475] ""EXEC""", 
                    expectedResult: new RawMonitorLine(
                        rawLine: @"1424186960.476348 [0 127.0.0.1:60475] ""EXEC""", 
                        rawTimeStamp: "1424186960.476348", 
                        rawDb: "0", 
                        rawCommand: "EXEC", 
                        rawArgs: new string[0])), 

                // Command, 1 argument
                new ParseExpectation(
                    name: "Command, 1 arg", 
                    input: @"1424186960.663817 [0 127.0.0.1:60475] ""GET"" ""KEY1""", 
                    expectedResult: new RawMonitorLine(
                        rawLine: @"1424186960.663817 [0 127.0.0.1:60475] ""GET"" ""KEY1""", 
                        rawTimeStamp: "1424186960.663817", 
                        rawDb: "0", 
                        rawCommand: "GET", 
                        rawArgs: new[] { "KEY1" })), 

                // Command, many arguments
                new ParseExpectation(
                    name: "Command, many args", 
                    input: @"1424186960.663817 [0 127.0.0.1:60475] ""MGET"" ""KEY1"" ""KEY2"" ""KEY3""", 
                    expectedResult: new RawMonitorLine(
                        rawLine:  @"1424186960.663817 [0 127.0.0.1:60475] ""MGET"" ""KEY1"" ""KEY2"" ""KEY3""", 
                        rawTimeStamp: "1424186960.663817", 
                        rawDb: "0", 
                        rawCommand: "MGET", 
                        rawArgs: new[] { "KEY1", "KEY2", "KEY3" })), 

                // Make sure the case is preserved as per the original input
                new ParseExpectation(
                    name: "Command, many args, mixed case", 
                    input: @"1424186960.663817 [0 127.0.0.1:60475] ""MgEt"" ""KeY1"" ""KeY2"" ""KeY3""", 
                    expectedResult: new RawMonitorLine(
                        rawLine: @"1424186960.663817 [0 127.0.0.1:60475] ""MgEt"" ""KeY1"" ""KeY2"" ""KeY3""", 
                        rawTimeStamp: "1424186960.663817", 
                        rawDb: "0", 
                        rawCommand: "MgEt", 
                        rawArgs: new[] { "KeY1", "KeY2", "KeY3" })), 

                // Really weird escape chars and spaces in the args.
                // E.g: 1424186960.663817 [0 127.0.0.1:60475] "MGET" "KEY \" 1" "K\\E\\Y2" "KEY3"
                // We should get back 3 args UNESCAPED:
                // - KEY " 1
                // - K\E\Y2
                // - KEY3
                new ParseExpectation(
                    name: "Command, escaped args #1", 
                    input: @"1424186960.663817 [0 127.0.0.1:60475] ""MGET"" ""KEY \"" 1"" ""K\\E\\Y2"" ""KEY3""", 
                    expectedResult: new RawMonitorLine(
                        rawLine: @"1424186960.663817 [0 127.0.0.1:60475] ""MGET"" ""KEY \"" 1"" ""K\\E\\Y2"" ""KEY3""", 
                        rawTimeStamp: "1424186960.663817", 
                        rawDb: "0", 
                        rawCommand: "MGET", 
                        rawArgs: new[] { @"KEY "" 1", @"K\E\Y2", "KEY3" })), 

                // BUG: The args with colons don't get parsed for some reason, make sure they do.
                // 1424365673.874009 [0 127.0.0.1:28025] "GET" "FOO:.&%39:BAR" "ZOO:.&%39:XXX"
                new ParseExpectation(
                    name: "Command, arg with colon", 
                    input: @"1424365673.874009 [0 127.0.0.1:28025] ""GET"" ""FOO:.&%39:BAR"" ""ZOO:.&%39:XXX""",
                    expectedResult: new RawMonitorLine(
                        rawLine: @"1424365673.874009 [0 127.0.0.1:28025] ""GET"" ""FOO:.&%39:BAR"" ""ZOO:.&%39:XXX""",
                        rawTimeStamp: "1424365673.874009", 
                        rawDb: "0", 
                        rawCommand: "GET", 
                        rawArgs: new[] { "FOO:.&%39:BAR", "ZOO:.&%39:XXX" })),

                // Make sure double-escaped chars get parsed correctly into unescaped chars.
                new ParseExpectation(
                    name: "Command, arg with double-escapes", 
                    input: @"1424365673.874009 [0 127.0.0.1:28025] ""GET"" ""FOO \\ BAR"" ""ZOO \\ XXX""",
                    expectedResult: new RawMonitorLine(
                        rawLine: @"1424365673.874009 [0 127.0.0.1:28025] ""GET"" ""FOO \\ BAR"" ""ZOO \\ XXX""",
                        rawTimeStamp: "1424365673.874009", 
                        rawDb: "0", 
                        rawCommand: "GET", 
                        rawArgs: new[] { @"FOO \ BAR", @"ZOO \ XXX" })),

                // Edge-case of quote following double escape.
                new ParseExpectation(
                    name: "Command, quote following double-escape", 
                    input: @"1424365673.874009 [0 127.0.0.1:28025] ""GET"" ""FOO \\"" ""ZOO \\""",
                    expectedResult: new RawMonitorLine(
                        rawLine: @"1424365673.874009 [0 127.0.0.1:28025] ""GET"" ""FOO \\"" ""ZOO \\""",
                        rawTimeStamp: "1424365673.874009", 
                        rawDb: "0", 
                        rawCommand: "GET", 
                        rawArgs: new[] { @"FOO \", @"ZOO \" })),

                // Binary strings and escaped chars like tabs.
                // These should be converted to the real chars
                new ParseExpectation(
                    name: "Command, binary strings and escapes", 
                    input: "1424365673.874009 [0 127.0.0.1:28025] \"GET\" " + @"""FOO \x04 \x00 \n \t \r \xff \\ BAR"" ""ZOO \x04 \x00 \n \t \r \xff \\ XXX""",
                    expectedResult: new RawMonitorLine(
                        rawLine: "1424365673.874009 [0 127.0.0.1:28025] \"GET\" " + @"""FOO \x04 \x00 \n \t \r \xff \\ BAR"" ""ZOO \x04 \x00 \n \t \r \xff \\ XXX""",
                        rawTimeStamp: "1424365673.874009", 
                        rawDb: "0", 
                        rawCommand: "GET", 
                        rawArgs: new[] { "FOO \x04 \x00 \n \t \r \xff \\ BAR", "ZOO \x04 \x00 \n \t \r \xff \\ XXX" })),
            };
        }

        /// <summary>
        /// All sorts of malformed monitor lines.
        /// </summary>
        public static ParseExpectation[] ParseRawExpectations_Malformed_DefaultConfig()
        {
            return new[]
            {
                new ParseExpectation(
                    name: "Just timestamp", 
                    input: "1424186960.663817", 
                    expectedResult: null), 
                new ParseExpectation(
                    name: "Just timestamp, db, and address", 
                    input: "1424186960.663817 [0 127.0.0.1:60475]", 
                    expectedResult: null), 

                // We expect quotes around command and the args. This case is when they are missing.
                new ParseExpectation(
                    name: "Missing quotes", 
                    input: "1424186960.663817 [0 127.0.0.1:60475] GET KEY1", 
                    expectedResult: null), 

                // Non-command. We sometimes get these
                new ParseExpectation(
                    name: "Non-command", 
                    input: "OK", 
                    expectedResult: null), 
            };
        }

        /// <summary>
        /// Test for the <see cref="Parser.ParseRaw"/> when we expect the input to be correctly formed and valid.
        /// </summary>
        [Test]
        [TestCaseSource("ParseRawExpectations_DefaultConfig")]
        public void ParseRaw_CorrectLines(ParseExpectation expectation)
        {
            var expected = expectation.ExpectedResult;

            var parser = new Parser();
            var parsed = parser.ParseRaw(expectation.Input);

            Assert.That(parsed, Is.Not.Null);
            Assert.That(parsed.RawArgs, Is.EqualTo(expected.RawArgs));
            Assert.That(parsed.RawCommand, Is.EqualTo(expected.RawCommand));
            Assert.That(parsed.RawDb, Is.EqualTo(expected.RawDb));
            Assert.That(parsed.RawLine, Is.EqualTo(expected.RawLine));
            Assert.That(parsed.RawTimeStamp, Is.EqualTo(expected.RawTimeStamp));
        }

        /// <summary>
        /// Test for the <see cref="Parser.ParseRaw"/> when we expect the input to malformed or not valid.
        /// </summary>
        [Test]
        [TestCaseSource("ParseRawExpectations_Malformed_DefaultConfig")]
        public void ParseRaw_MalformedLines(ParseExpectation expectation)
        {
            var expected = expectation.ExpectedResult;

            var parser = new Parser();
            var parsed = parser.ParseRaw(expectation.Input);
            Assert.That(parsed, Is.Null);
        }

        /// <summary>
        /// The expectation for the result of the parse operation.
        /// </summary>
        public sealed class ParseExpectation
        {
            public ParseExpectation(string name, string input, RawMonitorLine expectedResult)
            {
                this.Name = name;
                this.Input = input;
                this.ExpectedResult = expectedResult;
            }

            /// <summary>
            /// Gets the expected result of the parse operation.
            /// </summary>
            /// <value>
            /// The expected result.
            /// </value>
            public RawMonitorLine ExpectedResult { get; private set; }

            /// <summary>
            /// Gets the input raw Redis monitor line given to the parser.
            /// </summary>
            /// <value>
            /// The input.
            /// </value>
            public string Input { get; private set; }

            /// <summary>
            /// Gets the name of this expectation. NUnit will use this name when reporting results.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; private set; }

            public override string ToString()
            {
                return this.Name;
            }
        }
    }
}