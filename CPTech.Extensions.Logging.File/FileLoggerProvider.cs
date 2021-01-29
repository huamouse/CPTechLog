using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CPTech.Extensions.Logging.File
{
    [ProviderAlias("File")]
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, FileLogger> loggers = new ConcurrentDictionary<string, FileLogger>();
        private readonly IOptionsMonitor<FileLoggerOptions> options;
        private readonly FileLoggerProcessor loggerProcessor = new FileLoggerProcessor();

        private IDisposable optionsReloadToken;
        private IExternalScopeProvider scopeProvider;

        public FileLoggerProvider(IOptionsMonitor<FileLoggerOptions> options)
        {
            this.options = options;

            ReloadLoggerOptions(options.CurrentValue);
            optionsReloadToken = options.OnChange(ReloadLoggerOptions);
        }

        public ILogger CreateLogger(string name)
        {
            return loggers.TryGetValue(name, out FileLogger logger) ?
                logger :
                loggers.GetOrAdd(name, new FileLogger(name, loggerProcessor)
                {
                    ScopeProvider = scopeProvider,
                });
        }

        public void Dispose()
        {
            optionsReloadToken?.Dispose();
            loggerProcessor.Dispose();
        }

        private void ReloadLoggerOptions(FileLoggerOptions options)
        {
            loggerProcessor.Options = options;
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            this.scopeProvider = scopeProvider;

            foreach (KeyValuePair<string, FileLogger> logger in loggers)
            {
                logger.Value.ScopeProvider = scopeProvider;
            }
        }
    }
}
