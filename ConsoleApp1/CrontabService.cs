using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;

public class CrontabService
{
    private readonly string _connectionString;

    public CrontabService(string connectionString)
    {
        _connectionString = connectionString;
    }

    // 🔍 查询 Cron 表达式
    public string GetCronExpression(int kind)
    {
        using (IDbConnection db = new MySqlConnection(_connectionString))
        {
            return db.QueryFirstOrDefault<string>("SELECT cronexpress FROM crontab WHERE kind = @kind", new { kind });
        }
    }

    // 📝 获取所有 Cron 任务
    public List<Crontab> GetAllCrontabs()
    {
        using (IDbConnection db = new MySqlConnection(_connectionString))
        {
            return db.Query<Crontab>("SELECT * FROM crontab").ToList();
        }
    }

    // ✏️ 更新 Cron 表达式
    public void UpdateCronExpression(int id, string newCronExpression)
    {
        using (IDbConnection db = new MySqlConnection(_connectionString))
        {
            db.Execute("UPDATE crontab SET cronexpress = @newCron WHERE id = @id", new { newCron = newCronExpression, id });
        }
    }

    // ➕ 添加新的 Cron 任务
    public void AddCrontab(string kind, string cronExpression)
    {
        using (IDbConnection db = new MySqlConnection(_connectionString))
        {
            db.Execute("INSERT INTO crontab (kind, cronexpress) VALUES (@kind, @cron)", new { kind, cron = cronExpression });
        }
    }

    // ❌ 删除 Cron 任务
    public void DeleteCrontab(int id)
    {
        using (IDbConnection db = new MySqlConnection(_connectionString))
        {
            db.Execute("DELETE FROM crontab WHERE id = @id", new { id });
        }
    }
}
public class Crontab
{
    public int Id { get; set; }
    public string Kind { get; set; }
    public string CronExpress { get; set; }
}
