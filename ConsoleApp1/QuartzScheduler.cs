using Quartz;
using Quartz.Impl;
using System;
using System.Threading.Tasks;


[DisallowConcurrentExecution]
public class SyncJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        LogHelper.Info($"🔹 scheduler任务开始执行：{DateTime.Now}");

        // 1️⃣ 读取外部传入的参数
        JobDataMap dataMap = context.JobDetail.JobDataMap;
        string csvFilePath = dataMap.GetString("CsvFilePath");
        string mysqlConnectionString = dataMap.GetString("MySQLConnectionString");
        DateTime startDate = dataMap.GetDateTime("startDate");
        DateTime endDate = dataMap.GetDateTime("endDate");
        // 2️⃣ 调用数据同步方法
        Dlt dlt = new Dlt();
        //dlt.SyncData();  // 执行 Oracle 到 MySQL 的同步
        static IEnumerable<DateTime> GetDateRange(DateTime start, DateTime end)
        {
            return Enumerable.Range(0, (end - start).Days + 1)
                             .Select(offset => start.AddDays(offset));
        }
        /// 实现多日导入数据
        foreach (var date in GetDateRange(startDate, endDate))
        {
            // Console.WriteLine(date.ToString("yyyy-MM-dd"));
            //dlt.SyncData(date.ToString("yyyy-MM-dd"));
        }
// 需要添加，自动下载数据

        dlt.ImportCsvToMySQL(csvFilePath, mysqlConnectionString); // 执行 CSV 导入

        LogHelper.Info($"✅ scheduler任务执行完成：{DateTime.Now}");
        return Task.CompletedTask;
    }
}

public class QuartzScheduler
{
    private static IScheduler? scheduler;  // 全局持久化 Scheduler 实例

    public static async Task Start(string csvFilePath,
         string mysqlConnectionString, DateTime startDate, DateTime endDate,
         string argcronExpression = "0 0 1 * * ? *")
    {
        if (QuartzScheduler.scheduler != null &&
            !QuartzScheduler.scheduler.IsShutdown)
        {
            LogHelper.Info("⚠ Scheduler 已经在运行中！");
            return;
        }
        // 1️⃣ 创建 Quartz 调度器工厂
        StdSchedulerFactory factory = new StdSchedulerFactory();
        IScheduler scheduler = await factory.GetScheduler();
        LogHelper.Info("⏳ scheduler 定时任务准备启动...");
        // 2️⃣ 启动调度器
        await scheduler.Start();

        // 3️⃣ 定义 Job，并传递外部参数
        IJobDetail job = JobBuilder.Create<SyncJob>()
            .WithIdentity("SyncJob", "Group1")
            .UsingJobData("CsvFilePath", csvFilePath)  // 传递 CSV 文件路径
            .UsingJobData("MySQLConnectionString", mysqlConnectionString)
            .UsingJobData("startDate", startDate.ToShortDateString())
            .UsingJobData("endDate", endDate.ToShortDateString())  // 传递 MySQL 连接字符串
            .Build();
        CrontabService crontabService = new CrontabService(mysqlConnectionString);
        string cronExpression = crontabService.GetCronExpression(1) ?? argcronExpression; // 默认30分钟执行

        // 4️⃣ 创建 Cron 触发器（每天凌晨 1:00 执行）
        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("SyncTrigger", "Group1")
            .WithCronSchedule(cronExpression)  // CRON 表达式：每天凌晨 1:00 执行
            .Build();

        // 5️⃣ 将任务和触发器加入调度器
        await scheduler.ScheduleJob(job, trigger);
        LogHelper.Info("⏳ scheduler 定时任务已启动...");
    }
    public static async Task StopScheduler()
    {
        if (scheduler == null || scheduler.IsShutdown)
        {
            LogHelper.Info("⚠ Scheduler 未启动，无法停止！");
            return;
        }

        await scheduler.Shutdown();
        LogHelper.Info("❌ Scheduler 已停止！");
    }

    public static bool IsSchedulerRunning()
    {
        return scheduler != null && !scheduler.IsShutdown;
    }
}


// // 调用示例
// static async Task Main(string[] args)
//     {
//         if (args.Length < 2)
//         {
//             Console.WriteLine("❌ 请输入 CSV 文件路径 和 MySQL 连接字符串！");
//             return;
//         }

//         string csvFilePath = args[0];
//         string mysqlConnectionString = args[1];

//         Console.WriteLine("🚀 Quartz 定时任务系统启动...");

//         // 启动 Quartz 任务调度，并传递参数
//         await QuartzScheduler.Start(csvFilePath, mysqlConnectionString,startDate,endDate);

//         await Task.Delay(-1);
//     }