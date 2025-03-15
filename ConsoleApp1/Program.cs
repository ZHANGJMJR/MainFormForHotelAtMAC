// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
Dlt dlt = new Dlt();

DateTime startDate = new DateTime(2025, 1, 1);
DateTime endDate = new DateTime(2025, 3, 11);

foreach (var date in GetDateRange(startDate, endDate))
{
    Console.WriteLine(date.ToString("yyyy-MM-dd"));
    dlt.SyncData(date.ToString("yyyy-MM-dd"));
}
    

    static IEnumerable<DateTime> GetDateRange(DateTime start, DateTime end)
{
    return Enumerable.Range(0, (end - start).Days + 1)
                     .Select(offset => start.AddDays(offset));
}

// Dlt dlt = new Dlt();
// dlt.SyncData("2025-03-02");
//dlt.DeleteDataByDate("2024-07-01",dlt.getMysqlConnectStr);