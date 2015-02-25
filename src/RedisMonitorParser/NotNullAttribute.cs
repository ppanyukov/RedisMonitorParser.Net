namespace RedisMonitorParser
{
    using System;

    /// <summary>
    /// Support attribute for Resharper.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    internal sealed class NotNullAttribute : Attribute
    {
    }
}