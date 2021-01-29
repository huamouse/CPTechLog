using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace CPTech.Extensions.Logging.File
{
    [ProviderAlias("File")]
    internal class FileLogger : ILogger
    {
        private readonly string name;
        private readonly FileLoggerProcessor loggerProcessor;
        static protected string delimiter = "\t";

        internal IExternalScopeProvider ScopeProvider { get; set; }

        public FileLogger(string name, FileLoggerProcessor loggerProcessor)
        {
            this.name = name;
            this.loggerProcessor = loggerProcessor;
        }

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var msg = formatter(state, exception);
            Write(logLevel, eventId, msg, exception);
        }

        void Write(LogLevel logLevel, EventId eventId, string message, Exception ex)
        {
            var log = string.Concat(DateTime.Now.ToString("HH:mm:ss"), '[', logLevel.ToString(), ']', '[',
                  Thread.CurrentThread.ManagedThreadId.ToString(), ',', eventId.Id.ToString(), ',', eventId.Name, ']',
                  delimiter, message, delimiter, ex?.ToString(), delimiter, name);

            loggerProcessor.EnqueueMessage(log);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
