namespace RedisMonitorParser
{
    /// <summary>
    /// Configuration options for the <see cref="Parser" />
    /// </summary>
    public sealed class ParserConfiguration
    {
        /// <summary>
        /// The default configuration.
        /// </summary>
        private static readonly ParserConfiguration DefaultConfiguration = new ParserConfiguration();

        public ParserConfiguration(bool unquoteCommand = false, bool unquoteArgs = false)
        {
            this.UnquoteCommand = unquoteCommand;
            this.UnquoteArgs = unquoteArgs;
        }

        /// <summary>
        /// Gets the default parser configuration. The defaults are to leave both command and arguments as-is with quotes.
        /// TODO: Change the default to unquote.
        /// </summary>
        /// <value>
        /// The default parser configuration.
        /// </value>
        public static ParserConfiguration Default
        {
            get
            {
                return DefaultConfiguration;
            }
        }

        public bool UnquoteArgs { get; private set; }

        public bool UnquoteCommand { get; private set; }
    }
}