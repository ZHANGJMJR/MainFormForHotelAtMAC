using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;

public static class LogHelper
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    static LogHelper()
    {
        // 读取 log4net 配置文件
        var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.Configure(logRepository, new FileInfo("log.config"));
    }

    // 记录信息日志
    public static void Info(string message)
    {
        log.Info(message);
    }

    // 记录警告日志
    public static void Warn(string message)
    {
        log.Warn(message);
    }

    // 记录错误日志
    public static void Error(string message, Exception ex = null)
    {
        log.Error(message, ex);
    }

    // 记录调试日志
    public static void Debug(string message)
    {
        log.Debug(message);
    }
}
