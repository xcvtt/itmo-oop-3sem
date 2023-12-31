﻿using Backups.Extra.Models;

namespace Backups.Extra.Loggers;

public class ConsoleLogger : ILogger
{
    private bool _prefixEnabled;

    public ConsoleLogger(bool prefixEnabled)
    {
        _prefixEnabled = prefixEnabled;
    }

    public LoggerType LoggerType => LoggerType.ConsoleLogger;

    public void Log(string message)
    {
        if (_prefixEnabled)
        {
            message = $"{DateTime.Now} : {message}";
        }

        Console.WriteLine(message);
    }
}