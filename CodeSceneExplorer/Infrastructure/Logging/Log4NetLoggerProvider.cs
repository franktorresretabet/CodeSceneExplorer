using log4net;
using Microsoft.Extensions.Logging;

namespace CodeSceneExplorer.Infrastructure.Logging;

public sealed class Log4NetLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new Log4NetLogger(categoryName);

    public void Dispose()
    {
    }

    private sealed class Log4NetLogger(string categoryName) : ILogger
    {
        private readonly ILog logger = LogManager.GetLogger(categoryName);

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel) =>
            logLevel switch
            {
                LogLevel.Trace => logger.IsDebugEnabled,
                LogLevel.Debug => logger.IsDebugEnabled,
                LogLevel.Information => logger.IsInfoEnabled,
                LogLevel.Warning => logger.IsWarnEnabled,
                LogLevel.Error => logger.IsErrorEnabled,
                LogLevel.Critical => logger.IsFatalEnabled,
                _ => false
            };

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);

            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    logger.Debug(message, exception);
                    break;
                case LogLevel.Information:
                    logger.Info(message, exception);
                    break;
                case LogLevel.Warning:
                    logger.Warn(message, exception);
                    break;
                case LogLevel.Error:
                    logger.Error(message, exception);
                    break;
                case LogLevel.Critical:
                    logger.Fatal(message, exception);
                    break;
            }
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}
