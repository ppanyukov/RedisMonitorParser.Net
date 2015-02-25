namespace RedisMonitorParser
{
    /// <summary>
    /// Represents a raw parsed Redis monitor line. The "raw" here means that
    /// all elements like timestamp and arguments are in the form of raw strings
    /// as they came from the monitor line.
    /// </summary>
    /// <remarks>
    /// The example of the input monitor lines is as follows.
    /// <code>
    /// [timestamp]-------[db client_IP_port]-[command]-[0-N arguments]
    /// 1424186956.633238 [0 127.0.0.1:60475] "EXEC"
    /// 1424186956.633238 [0 127.0.0.1:60475] "SELECT" "0"
    /// 1424186956.633238 [0 127.0.0.1:60475] "MGET" "KEY1" "KEY2"
    /// 1424186956.633238 [0 127.0.0.1:60475] "HGET" "KEY" "FIELD"
    /// </code>
    /// In the example above, this is how the above will map onto properties:
    /// <code>
    ///     - rawLine      -> 1424186956.633238 [0 127.0.0.1:60475] "MGET" "KEY1" "KEY2"  (the entire line)
    ///     - rawTimeStamp -> 1424186956.633238 (string)
    ///     - rawDb        -> 0 (string)
    ///     - rawCommand   -> "MGET" (string, yes with quotes)
    ///     - rawArgs      -> "KEY1" "KEY2" (string, yes with quotes)
    /// </code>
    /// </remarks>
    public sealed class RawMonitorLine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RawMonitorLine" /> class.
        /// </summary>
        /// <param name="rawLine">The raw monitor line as-is, as produced by Redis.</param>
        /// <param name="rawTimeStamp">The raw UNIX time stamp, as produced by Redis.</param>
        /// <param name="rawDb">The raw database number as a string.</param>
        /// <param name="rawCommand">The raw command, as produced by Redis, e.g. GET, MGET.</param>
        /// <param name="rawArgs">The raw arguments, as produced by Redis. These would follow the command.</param>
        public RawMonitorLine(
            [NotNull] string rawLine, 
            [NotNull] string rawTimeStamp, 
            [NotNull] string rawDb, 
            [NotNull] string rawCommand, 
            [NotNull] string[] rawArgs)
        {
            this.RawLine = rawLine;
            this.RawTimeStamp = rawTimeStamp;
            this.RawDb = rawDb;
            this.RawCommand = rawCommand;
            this.RawArgs = rawArgs;
        }

        /// <summary>
        /// Gets the raw arguments for the command. This can be zero or more arguments as one string.
        /// </summary>
        /// <value>
        /// The raw arguments.
        /// </value>
        public string[] RawArgs { get; private set; }

        /// <summary>
        /// Gets the raw command, e.g GET, MGET etc.
        /// </summary>
        /// <value>
        /// The raw command.
        /// </value>
        public string RawCommand { get; private set; }

        /// <summary>
        /// Gets the raw database number, as a string.
        /// </summary>
        /// <value>
        /// The raw database.
        /// </value>
        public string RawDb { get; private set; }

        /// <summary>
        /// Gets the entire raw monitor line, as produced by Redis.
        /// </summary>
        /// <value>
        /// The raw line.
        /// </value>
        public string RawLine { get; private set; }

        /// <summary>
        /// Gets the raw time stamp as produced by Redis. This is a UNIX timestamp in format seconds.microseconds.
        /// </summary>
        /// <value>
        /// The raw time stamp.
        /// </value>
        public string RawTimeStamp { get; private set; }
    }
}