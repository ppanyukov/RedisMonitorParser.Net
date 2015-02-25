namespace RedisMonitorParser
{
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
        /// The regex to extract individual arguments.
        /// The regex reads like: quote, followed by zero or more of ((witespace char or word) or (backslash followed by any char))
        /// followed by quote.
        /// </summary>
        private const string RegexArgsString = @"(""([\s\w]|\\.)*"")";

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
        /// The compiled regex for extracting the arguments from the raw monitor line.
        /// </summary>
        private static readonly Regex RegexArgsCompiled = new Regex(RegexArgsString, RegexOptions.Compiled);

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

                // Extract the arguments separately
                var rawArgsMatches = RegexArgsCompiled.Matches(rawArgsFull);
                var rawArgs = new string[rawArgsMatches.Count];
                for (var i = 0; i < rawArgsMatches.Count; i++)
                {
                    var arg = rawArgsMatches[i].Value;
                    rawArgs[i] = Unquote(arg);
                }

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