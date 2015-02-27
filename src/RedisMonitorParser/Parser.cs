namespace RedisMonitorParser
{
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// A parser of the Redis monitor line.
    /// Lines look like this:
    /// <code>
    /// [timestamp]-------[db client_IP_port]-[command]-[0-N arguments]
    /// 1424186956.633238 [0 127.0.0.1:60475] "EXEC"
    /// 1424186956.633238 [0 127.0.0.1:60475] "SELECT" "0"
    /// 1424186956.633238 [0 127.0.0.1:60475] "MGET" "KEY1" "KEY2"
    /// 1424186956.633238 [0 127.0.0.1:60475] "HGET" "KEY" "FIELD"
    /// </code>
    /// </summary>
    public sealed class Parser
    {
        /// <summary>
        /// The regular expression we use to parse the line.
        /// This was borrowed and adjusted from here: https://github.com/facebookarchive/redis-faina/blob/master/redis-faina.py
        /// </summary>
        /// <remarks>
        /// For Redis version 2.6+, fixed version with:
        /// <para>
        ///     <code>
        ///     - correct args: command can have zero or more args. Not all commands do have them
        ///     and it's also incorrect to imply that each command would have a single key
        ///     because some commands have multiple keys (MGET) and some commands have
        ///     args which are non-keys (MIGRATE, OBJECT, SCAN, PUB/SUB)
        ///     - correct client IP (IPv6 can be in form "[::]:21452"), but we don't use this at the moment.
        ///     - the command is quoted
        ///     </code>
        /// </para>
        /// </remarks>
        private const string RegexString = @"^(?<timestamp>[\d\.]+)\s\[(?<db>\d+)\s\S+]\s(?<command>""\w+"")((?<args>\s.*))*$";

        /// <summary>
        /// The quote chars we use in unquoting strings. See <see cref="Unquote" />.
        /// </summary>
        private static readonly char[] QuoteChars = { '"' };

        /// <summary>
        /// The compiled regular expression for parsing.
        /// </summary>
        private static readonly Regex RegexCompiled = new Regex(RegexString, RegexOptions.Compiled);

        /// <summary>
        /// Parses the raw redis monitor line (as produced by Redis) into <see cref="RawMonitorLine" />.
        /// The <see cref="RawMonitorLine" /> preserves all information and just provides access to the
        /// elements of the monitor line.
        /// </summary>
        /// <param name="rawLine">The raw line.</param>
        /// <returns>An instance of <see cref="RawMonitorLine" />.</returns>
        public RawMonitorLine ParseRaw(string rawLine)
        {
            // Trim whitespace so that our regex does not get into trouble.
            var rawLineTrimmed = rawLine.Trim();

            var match = RegexCompiled.Match(rawLineTrimmed);
            if (match.Success)
            {
                var groups = match.Groups;
                var timeStamp = groups["timestamp"];
                var db = groups["db"];
                var command = groups["command"];

                // All the args as one string
                var rawArgsFull = groups["args"].Value;

                var rawArgs = ExtractRawArgs(rawArgsFull);

                var monitorLine = new RawMonitorLine(
                    rawLine: rawLine, 
                    rawTimeStamp: timeStamp.Value, 
                    rawDb: db.Value, 
                    rawCommand: Unquote(command.Value), 
                    rawArgs: rawArgs);
                return monitorLine;
            }
            else
            {
                // TODO: What to return if there is no match?
                return null;
            }
        }

        /// <summary>
        /// Extracts arguments from the full argument string.
        /// </summary>
        /// <param name="rawArgsFull">The full arguments string.</param>
        /// <returns>Array of extracted arguments.</returns>
        private static string[] ExtractRawArgs(string rawArgsFull)
        {
            //// The input is like this: "arg1" "arg2" "argN"
            //// The args are separated by space and surrounded in quotes.
            //// The quote starts new arg.
            //// The quote also ends the arg unless it's escaped like this "FOO \" BAR".
            //// The escape char itself follows double-escape like this: "FOO \\ BAR"
            //// There could be various other escapes like \xFF or \t.

            const char QuoteChar = '"';
            const char EscapeChar = '\\';
            const char SpaceChar = ' ';

            var results = new List<string>();
            var argChars = rawArgsFull.ToCharArray();

            var buffer = new StringBuilder();

            for (var i = 0; i < argChars.Length; i++)
            {
                var c = argChars[i];

                // If buffer is empty, next char starts new argument, unless this char is space or a quote.
                if (buffer.Length == 0)
                {
                    if (c == SpaceChar || c == QuoteChar)
                    {
                        continue;
                    }
                    else
                    {
                        buffer = new StringBuilder();
                    }
                }

                if (c == EscapeChar)
                {
                    // expect either a \xFF form or \c where c is any other char other than 'x'.
                    var miniBuffer = new StringBuilder();
                    miniBuffer.Append(c);

                    i++;
                    var nextChar = argChars[i];
                    miniBuffer.Append(nextChar);

                    if (nextChar == 'x')
                    {
                        i++;
                        miniBuffer.Append(argChars[i]);
                        i++;
                        miniBuffer.Append(argChars[i]);
                    }

                    // This one seems to work correctly with Redis-escaped chars.
                    var unescaped = Regex.Unescape(miniBuffer.ToString());
                    buffer.Append(unescaped);
                    continue;
                }

                if (c == QuoteChar)
                {
                    results.Add(buffer.ToString());
                    buffer = new StringBuilder();
                    continue;
                }

                // Plain char, just add
                buffer.Append(c);
            }

            if (buffer.Length > 0)
            {
                results.Add(buffer.ToString());
            }

            return results.ToArray();
        }

        /// <summary>
        /// Unquotes the specified string by removing leading and trailing quotes.
        /// </summary>
        /// <param name="s">The string to unquote.</param>
        /// <returns>Unquoted string</returns>
        private static string Unquote(string s)
        {
            return s.Trim(QuoteChars);
        }
    }
}