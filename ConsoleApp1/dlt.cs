// dlt.cs (修正后的代码)
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Globalization;
using System.Transactions;
using Dapper; 
using Oracle.ManagedDataAccess.Client;
using MySql.Data.MySqlClient;
using System.Reflection;
using OfficeOpenXml;
using System.Security.Cryptography;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
// ========== 主表模型 GuestCheckHist ==========
public class GuestCheckHist
{
    public long guestcheckid { get; set; }
    public DateTime busdate { get; set; }
    public long locationid { get; set; }
    public long revenuecenterid { get; set; }
    public long checkNum { get; set; }
    public DateTime openDateTime { get; set; }
    public decimal checkTotal { get; set; }
    public long numItems { get; set; }
    public string firstName { get; set; }
    public string lastName { get; set; }
}

// ========== 从表模型 GuestCheckDetailHist ==========
public class GuestCheckDetails
{
    public DateTime transTime { get; set; }
    public long serviceRoundNum { get; set; }
    public long lineNum { get; set; }
    public long guestCheckLineItemID { get; set; }
    public int detailType { get; set; }
    public string itemName { get; set; }
    public string itemName2 { get; set; }
    public string itemchname { get; set; }
    public string rvcName { get; set; }
    public string firstName { get; set; }
    public string lastName { get; set; }
    public string reasonVoidText { get; set; }
    public string returnText { get; set; }
    public long recordID { get; set; }

    public decimal salesTotal { get; set; }
    public int salesCount { get; set; }
    public string salesCountDivisor { get; set; }

    public long locationID { get; set; }
    public int doNotShow { get; set; }
    public long guestCheckID { get; set; }
    public long checkNum { get; set; }
}

public class GuestCheckDetailsSumRow
{
    public long organizationID { get; set; }
    public long checkNum { get; set; }
    public string tableRef { get; set; }
    public DateTime openDatetime { get; set; }
    public decimal duration { get; set; }
    public long numGuests { get; set; }
    public string checkRef { get; set; }
    public string locName { get; set; }
    public string rvcName { get; set; }
    public string otName { get; set; }
    public string firstName { get; set; }
    public string lastName { get; set; }
    public long guestCheckID { get; set; }

}
public class Dlt
{
    //private static readonly ILogHelper LogHelper = LogHelperManager.GetLogHelperger("DltLogHelperger");
    private static readonly string oracleConnStr = "User Id=sys;Password=Orcl$1mph0ny;Data Source=172.16.139.12:1521/mcrspos;DBA Privilege=SYSDBA;";
    protected static readonly string mysqlConnStr = "Server=127.0.0.1;Port=3306;Database=hotel;User=root;Password=root;";
    public static string getMysqlConnectStr() => mysqlConnStr;
    static Dlt()
    {
       // XmlConfigurator.Configure(new FileInfo("LogHelper.config"));
    }

    public void DeleteDataByDate(string dateString, string connectionString)
    {
        DateTime date;
        if (!DateTime.TryParse(dateString, out date))
        {
            LogHelper.Info("=== Invalid date format clean datum ===");
            throw new ArgumentException("Invalid date format");
        }

        string[] tables = { "guestcheckdetailssumrow", "guestcheckdetails", "guestcheck" };
        LogHelper.Info($"=== Start cleaning the data for {date:yyyy-MM-dd}.  ===");

        using (IDbConnection dbConnection = new MySqlConnection(connectionString))
        {
            dbConnection.Open();
            using (IDbTransaction transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    foreach (var table in tables)
                    {
                        string query = $"DELETE FROM {table} WHERE DATE(openDatetime) = @date";
                        int affectedRows = dbConnection.Execute(query, new { date = date.Date }, transaction);
                        LogHelper.Info($" {table} cleaned，affected rows：{affectedRows}");
                    }
                    transaction.Commit();
                    LogHelper.Info("=== The data cleaning was successful.! ===");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    LogHelper.Error("=== The data cleaning failed. ===", ex);
                    throw;
                }
            }
        }
        LogHelper.Info("=== The data cleaning is complete. ===");
    }

    public void SyncData(String argCurrentDateStr = "2025-03-14")
    {
        string currentDateStr = argCurrentDateStr;
        DeleteDataByDate(currentDateStr, getMysqlConnectStr());
        string masterQueryE = "K4B9iRit2/VhVRdkP5ewWZCYaC0BBv3PDjPY/GSG5mtjwe7/oRIGkwNvBsmgDnnAGePhf7qWlDQMum7IU8dOzJ3NokUhQo+SCIyrn3AdfJCR/fF+h9gUh6WcYYV8Sn8qPTh4wKiu+E1r87BHKDbqjTdqlDcQ6l0tyfEThm8OxlGJbhF44557kidX8RuW5M211Ag5wLu84iYNQelyYBHstylJoRwRRrjZo2N3PQPWiHbPGY1DV30t1E94r+BSBhptFHzYtwskudRrxncDTL6irqc8Q7a+g5m8a1aCknz7Xgok0SZmHplmJsVCaiD9kH7mMhBFdVHguAJH60UmSO2aHiIACYYCYAGefev7+0Qw0UqwHvguZasI9Wb5bzOJMmFrPEDr1RSEdIbZpoGWWOX2mwtsYaF1F/PUyyexRCuLyVQPaERUh9tO0M2MKBbIUOd6HRtzAXjWi4mOAtDreDri8heEw8XDWUs8FhK6S7so4K7mao3DGak1HX4uhv4cC3uTQGk78ONhfviIZIYqkKZBhjHtbOHA2JniTCQupr9n+gmoGczYr025dHQTjMealefb02OWT/0Lk0wqLJ6FhLyNTXiT+t2w2WVOkoUMQI396CKC8F054sPb9LGOKGMf/D+fu0OZWNWmQdLN4HrldoNu+1kKzo9rMnitjCwDI8swrvR0JcVc7elpsY3kvyVparTig23ZCWZDrDbdzWXH3IYGIXSfV0QPgex+tmKF+KJ6Yj3Qf0hTVPAHRrWBQSfRZaHDkKY+QFNKKti6M0o9+KblKg3a2w4UpJ7CbyX99Hcv2HVLn2bx5ziDuS0+6g9y+qKYoNyl8OSfXiczDDRNAg+OYxedl5yu2duVszcQaj7aB8nupzlvJb1rcJlJzSwsHRZVs3tx1IWwK+OTRRuA6bm2pQ==";
        string detailQueryE = "2PEclgMLR5IC92DRvm8IsII43m6M1L5DKMkaXUzSYeEuaTVrBC2PWUd2EfbE+mBBAMwOPc89ICUSowPhuPtPkgOrPXBVdhGZA58zPCF5dKEPyLF4s76CehP3OsHYE59nUlV96i3u5LBqbAuRgBdf+axpbc/R5xo0FIYU2sepzFfBkMVdHVdxqDuJUHbR0p4T6//JT5bJPkvoqVZMahqpGhtakOPlyAPz1Mebbjd8EIgV2peTxtIttbjom+K+5yLhcsN6KVEb7m8feHKaWqtGUR4YZWe7aCooi/qAhHEQr8STVYNeENPqF1nPMAHjyqEcHUuzEYvRGX7yjlyyOpdScxyQPtm+c+Eea/Jb9Mvx4khWJfj7kYQPk05TcL38G8FY6pG+OAcjDH1uFt+5yFuZb9WCpB2nv6uZB8sICmgfMx+NUVoSrSSq6SakErHhnGoEzgwEM8QbbgBfTEWRBJbG9I/wMvRHSodCykFaEQ6AWOJ4c7nJK1YIUObV0NET5Xp3JvTtky4RLxBymjXg/8kiiTrlgE8i0ZuBonEGdpZmOdnjYmO2VLEPT1bJ50qwNQpndS30DswVt07I4LBHSJ1kh+jxiwp9yIWndKRdlzqBjI1+XizEXx5HUnFFuCdXJWXltuBtgX1mtjlNEm0bCKO9lHTNuQfPQcrTOG4q45HaEaerA92O2Kx21QE8oWVhJuUuebQcpCkmWVwG9/75O3Dm53RgPOScIHf7cYJMemLmP3XvZanFe4Zt7VVGyhv+DFgqPUXAqPBhsBWsiWBw7Z1v8EKmIVrjyfSW9lu9Oz9ek3HJHzpiJm+Z42ATf2JF7o4hf8utumiJNmsvuzXnSEemb0NA3rkr4kzyIU1fEMleJ/9prm5rdL2T5VJG5CmlTYHvR3M6yo021G3SsGnUeeGhgTUQkBxuleE1GQriZjdaqBhykqVjd2JJVrcXoryRaI+kMtQ/gY3yoiG5JwPWY+mt0FTQC3hDDztbzVk+HR/hMhDLrHT2CqDI5zY6SKDeo0x9qg69DVoP1bQCr0mGuHNRDrq76spg8keWsow7RbY9Hhz/FjJCdvxckAtOoSKdKc42PB0aaaaJlJMPlV7d2+/Go/0ptz+x54T4GXhTIKs8sSEdsoyg+X4t7Cln5K6Jkm3E5k4+nIG8KtLE8c1DyUecBC5DZe3w7wNRUOi51j89eckNIkzOIF8+hpvkzAgn5P1flPTgxaLIklcvL3NgDqA4BQwep00gdwF/LvopMi211CzP3yUPwU1+o3j6B26tKrGS0+zVn0oBov+ursgUk2cBKtlt4Mt5xBoaesr3aoMu87vQANjp1OaWVGcf9+zhZaxmspgqsr1SQI/Exuytt74j1dyN8itvAmjgcPTL4LMKGNlElrD45/4RD3a0txPSKoLvb/hzF8Eit66N0oJpKU3RkHG7kzrlS4kHZyJ0IRYuUq7N6owjhggVo3yyAlstc5D2XYmhTQttx5rh9trM2ZauhhSverejcKn8T2+ZsUFGh8aGsVqJnoMbX+XvqX+sU/BM4me20jwcinN9vQSxaZrH/9tMgOvTEwkSe88IKr6WSDqy2AGLz5ANe17LUOIy+TeoRYTSmivWLLPxBR8+Xf6VVrjMKC+bkIJvkAqoChYoBB/IhIWDNkgjVb5AMJ0otclU1vb9YS4cQ4JlAZhqH51E6eIjhBwsPWt5wx1WN1bm3jzMg4C0EUAgsSxO564n1s1zS8meV/AvNNSsrHjmrU5M/IOcWO3jpELHrjAsBnEKcqDGJ3U0nz3TLB4C7jwgHh4WWHkL7KroSsqYwqvBA63cY0wjyMxQp3q9PiuynCc+CNeZPFiW/mjtOq4m7KOzeCALhCVjhOglrtE9hFszbETmB8t1AEOMiLNUHGY3kmK3AU+K5j3zoC5bNgLeBQpGrSlXqN0Zwz6gfM00BDB+a3HOKOZYbW/c3oy40MsaXRU7GK9dOpF1QAoeIg4Tx/5U8K8QCz8iuXZvnQ1aVOs78OY7FKwgqk9DppH5yvPwVlJOTC+uYOQxPhAJZ6Se/oYyhO4ITtV8fpHEdVDlCGjlF429koEc9Q0e/tv0j6I10anjyJ+UIt9a+G5IoyDPSHOYcKQYTT/tE9dnpoYrjtG05uIUmuYoRUfkfpzDGv5dNzurLw1vwxmE4QKPKLChsoekZJmeMMNS9T55CNjV275u41vZcK7T6Ur4DLlqSrA8DesRZ76lDbrcUhn2YmHyrUTG/EKAcPhaVNAkD9MQnyqBMhK9NDIL4IIKln/c45Jb7k8/nz1FvritwYGEMIDz9rE16RTzMQtgbYtsI4fNzPqKzJXJeMkGVfEOyc/gw6hFRhUFqyiN6bQS9RiGY3CwvSDyFOkHKHLGmtHE7BpR1lWZbtdSEheRfwW1QonFol355ToRsbM7cT+6TeA5yNjxP8k3bX1Avohl0lT2U1TfKhHJafNwQZstVUPs6idoVOGAyKstHUHBEyEHdJqhMv675K1B74IAAXOW11eratlHnGi701rs/2ELG3QVAQ/D9xbvCvSWm7e3PeG8CKg9Kta22EU0ltJgvOUf+iRRncxTMJovm0iCA3+IUKBdP4iKt/1nVqvYaoA19Zx1d2Mq3aVHN4MLLvb2bTWylR2N6nF8UemTZk51j0b9aiwul2te+ILX2bgvdh+coaazJk2d8KfrVD/M7N9tli2bbsSlQ1XQZCfnLBH6RCmjpLjY106tOmv5YYDuz+2MSYbP0BC/BoQ1O7c+ov5ay3IDEE1KtX+Zp/Pj15/d5GlTWFdDS+GTO/gotmI43ZZwHo9E9wIgpmZo8dL8uSY2fwYDzbVOpdWzRhRNh2efg2NmINjz/4kqAFRC1VO9R/0aLo535r5IG2KFlfIGfFzVI2imN1UE9tO6vVL5ZXiyqqSRc0jvpL/7VF9GTOtTD7lOn6J8UcjvG0xq35PX/PS+oU5NqXlb+FaecaVb+NlstovR//oKACivECEdLrCSbIavV9s35VGm3j6byoRL2McMWzc05Ul92vm/D+AriLo87sk8BwFee6fa85HrRHMyVOH0JjOSaNtqZ8WEPxfH6W/921V8XwBl+gc/AF+RoNJ3wX8E+QReiNwHo1kpCAwAmb/rjz6Oj9ZNRTS853llzndJ4U4tt//GlhrDdmGqDtUStrhnTH2bHO0zHLy8YDXkmXPN10NTSSd2KDNSMtjnzDHsFHvOaKPkJGjBI6p4vnKeb7aNO8vTPCiPczQ3VDzff7NahM67TrqtFVwv3fMmnfS0iHGYiX8+LPyFyjTZXgd2vgyR35/q0NF8k+907R1C0QeCaNd7b5OPTwTivX48xXJSZcSjUMCGszfT3OCLAlzfQFGuZK32sd5VSPoNvTeMONOE+H5gl88rQsHVhQ+nTUFjF63qwpp0oGtbs0T6C52dEDhEarEnB5YzaWKeKIhnVXSoggT3iLb9gAHP9XhgxkKGVl6tDfnWkU2ZtedpRU6PG0UnJpOJrvUCLw1/daC6OcuipQrhQ7jcEMh9HAHuM89NO8P0+Ws0pXUtUJPBlpQesU/houl7/s7UocJG5XsQEwFh2jMZcJ3gODsPoALmHaLKb18ZbnzpNgex/XF+bHe4VWH6QjEoN1cHCm1A+ehI/K7ypT+8yk9+VGk43rnFkIXcYnGh4apV53BbFWZeOBpUzm19qrUg5yPDaOIKLvqprlJP/L9NmiGgsIyQGcxVP2S753zcVNln5PWzMHv/ltNumyDOGlBa6Rn4+hIREceyzU9enqz6VJVtEMNx1MysTUvQ1S4WXwpNOXVrkSI7/em2HQ03Gsb9ckCNyNqzhFaoBCsaapUL00ECiiOCrlWieALbMd9yGSZc4s17n1hjf1Y8pqn9SitRxTc3HVw+SLRABQAiVkImSd/P0sgDFcIT47BN6Qvh7P0jrMsY2Zu8grFyvRB3ezv/xfH8MX8FHPzH9b2RrabSU+JhXYVSsE8OWtEDQdgE7+MPItVCMVbXJKJr/jzcp3hTIVFeUyxwBj4ynvzYgA2tk2VFVCsfN15gMUpd9dCfEWCq/djlBpSvGDx+G+TCrs1sI9gGSYJxKUAOlfy6cs0pvfaCEOeRtVGbg6OA8vIwyJx9KBi8bwsXucAkJGtb/mevGBjOsUNTh44pd8QtSU8QkJbCYSzP88SvKVUcFIXio6EAR1cj+vRR0moDMzKu5yGLbfFML5k0tphe1EH4sxJE9CiTas/boci88CK9QM6CJJFgARnNvX3166RlWVdwDRJkpufFMxO6kLyaOPAYuDAk0MlPp47IqyPDb5O6Xzvey163+q4HQpjylWPI9M3/WwMeJ8YcG4B3/Y7FYdSU1YrPXrla6lUaii7mIwktS2b4DgPpLeSJookZdCfREV5FSIv77uUDRRuNKbwlYKkTtPerzcdj6rdd46TNBRVxvCBixYj+m2trjp4yttCkG/SxGWDC3WBkm/jiXQhIqW9Q1FVYe4fAc/qwYcvp9V6RcrUuugV9oIC0BhwINtx9fLfRjUwxePTyEFoiyUQcMWijq9w+NJSkcxohFCpVMrh1BBvsdYFTbEedoeLuCzN3CnUencNXpFWAkX9K0/D54SqdWXKAo7N94hDtzChjfklE07HQKAh+rOgClwkn5c3qfNwVFMeyjZeASRXvJG2CnniDdvJqIGXofq4lbkQpCUK1t2FNNmqfznqqc+QiuJfgX21YWuEVZUFZlsdJzEZ47vL0diP9F2sk3QwnsVUw+BRgP7/EHO4ekTcXWaP8XFymaWEt9odiyE50nMNMvwRNlEc25Edd5T29z2F9GG7FeyiA9k1O62HV6y4GKvq9BzqmG6IislVG79vkd2Oe15fjBgYJDAZ5puzTeohenJ1jBpHUvMskCbZN2dexsXck3bsqFaGhaLAMWshJm/65elE9EQG4W/r9X8rTclnbrQjpzjQ3pUeSOuu+HQo4EBPGaoivq+KjaZ6KpYCAhkWV/7Fre0xKeVEoRANUcFARxgkXVUP7EeOnchBTOFMNFBcTvHQaKGE9VuBZdJpDnO/hsuU8eGPML7md0nAl6/WQXUwaCGupfVSJdhWzFfocln3OWbGP3DCyBD9WIkUaz8IqT3pjvv9Ktv8Vc8rcAN/T20QxU30FGeYZ2jnNAlN27vx5jMgkf1jJse0yfYj+LA0j8RAE8xQXuX0is3G3eiH7zkviFn27XpNcfMT8SLLyCT5yJ+Q/Zf/BbIRDCs2gBe9aMnR7GjuMqD4tgpmS0hS6fcoTdP/m/Uw2vg3oSKKzn7/G7pAA7jaP+RLflyg7ni6B6LBivXbPo8gNWaJxj/3wg6TvZ4iZrtYbN2C4n2eOOJ/P/2VyRmH8HinpVmnJrBPDUlN/kpS1rL81Et/QirqcBRjPYCzUaTVRYfDQvVGLyZMhtp/gIGZVa6nfGloqzvSDZqQ7uy90icDgMJaZIW5CgcbolaPycU+frRfGFfI1YsnVOckEkgVNQmEkseJGlvx4Sd9F8kj2RZt45vJrApOoMgjzz8bWdYBRDcSHuOYBz3cZioY52ktWQJIPeA/G7sfDsMsZlRjsE1rM8v1jU1PcpO6WnRq4MHcjvUzLH3Nbk95YJQJzYYaWtj4H86borktbGEOHa2jXaTNMWvEfGtd80h9Ld0dox9KKbHAyQJCBEtBxMkcf6RzTz1cG+/cqBP8AXAcrI6UhpPTV2uhPYDwvxn1gQ0bXBpqzZTX2fLrumgkOGfqkWEcqSzA0XS73VZiUjmUomVymUS2i9KApE5MZ9uqVOqPB5Sowm4Wup/pdRRA0WCUFuN+B7DGp0bnE4xJmu1/NXkceMntcmI1958BQevqYoJGetLmsTE48kdED/cEMlBtt0p13w6HJ4n+qh/xlcbLzjz10gg14FFtj/tdnqvczpbSgfW3SmFaMPlLDKmkmUsvCRdyfKFIE5tlGH8PJHeSLxCZnKjsBsEa/2paHtqGmodfdw1j2iiGZPLdLhTgx3zKKRpYnPlIE6VeOEtZI5TEAiH/VuMMVNJFu6P0IzfIZ1HXl51SsLvaPYMcYJfQYjBIw4zNxf3WnCoiGaVGb/vdIn6FsRu6a3axeqBqw3YNZdBRGfZyCTVl3c9kgoNVibteI1IrftEa09GqeJfBcYhTi3cWcfeulKdAZJTdXrXdXAA8r4xzIn++nkwD2Y/3/MIXNdZjL5TbKqIbjwAXGjsGX/9aDn2gjbHSrp0AIjEUkCGKTfq9hD8dyVDU6bryEWjECucFnb1MITgzNmDBo7Zp25tP6JEokTDeCrmOW18wZinfZWA/6jOipBt7VLNW95plIHOb2/MwZ4I6U8MEAhhp8JbmXhiMwTBO8b7HmHDoxEQQdysf1QCLI+ShrFvRvgk5j07bNWEltuJG31bpO7Gf8bb5ATn8iUvcQYDXLqtALHGrxNioXDq/oXjOLrIOcWLN/FP//ALdDN/rxDOn3q1C3InfOV5EsPuyFie0yLTrCs1oyGUKOxno6hM0NwMOzA1pzbpB43Q3oD2g5763Hr2IeqJnqUaPGvC1clwI9TVUNEoAXGx2Bwvbo2rgh/D719iBKfL3fnUZ7mBYwtx4pzoacq4PPpGRF/h6+qHoGSKBpJ9JbTElOpJBKCSTqVnMN+O1tMnJcrKymnoKjcO1lLz051VCWSk22jQlziSOr6Lc8ufLo0aPHlDdPZts5cqXtR54NxRe07F5Nk6C4/6GYWv4NAVYf2ZsquRj+3tsZ5hEzT74AXAxx46qWZlYE67U5Eb7p96XiXI7VRpqJvdtvF82Xua1XTpcXGoHvfmW/kwKiQSnpnwlMnxUPIeZzJi+XsX1UAWU/aJ2CiXKGqsMXSZbSoZCm+VCUIHjSMZ5yHXZ7YvAq64ZxBMdDtWZPQoc0b1uEvovPylJJkfKzO4T6f2iekH2FPPfZlQDLS6fHLBjVo5lo4b+wF6qztts8putkvo+JwWkFq0vliVRff2tug3Wiwzm3KENm5Y8ozDj3HdqpvtNUWc3amx4pMA/ijQw328uFkREfoRRHfeaYwok1R/R3bnAmUJJygJhLqLLCM0aQHZHnDx4/HzGIJF2h9WDlEuad+/bO8bsnH8ZZICCrnbs5zc3J0o4Q65Cc7YGFNPR/gjR4h9k8p4SDtJWviWrcVs9cji3H0KP3DwWdjLPBlI5yzT5GpX/NF7ybZH646S7a6xaAPy8Qy3dit27S35Pk5kTAtxV9APcEgkL0EiFDiuBBos8FanoSX//Eqa2fDBzpTaBHgMHUhosyICB2XYdqW3zgZVRO973wLERt+B+zhZhUUizNF3Lbt6hTxt8jIm+Egc5pC1LBoaoOXBkyPklfX5YWcQfmbcfli5r7rt+TeOtLCfuVg5074NmJDZFLwYqRX0DjVIWmm+K6sqwkcOoP42xxupW+NCEKBUQYMD/+Nh7zSyUF75ViFMnaO0AkT2VbwPzVMJw7Vt4Kp0DZzceb/uWTpX9Sf1LWUVqty8RrcOHmXPVtr0IS6Uz1F2w78zFKc3RSNkSpYaPwpcCX7VDDTixn8Irfoq7GlwhOYigmO8u1AWCSYkHQLBs0jYaci3QjbrbKr39Lq664bjvUv79vFcbc4BpdIZaQrTZjFo4VFpxuvCicGZ+hvFr52Qj+0kKInKQPEhB+Fml/kUJyL860O8dGMM55+msICHQ8a0mLa8ovmZDKJUbCFummqq7t/duOp1HdWDGRFqeI4bhO8F5cnhxmgNYIHXvl6G+XUu40HRytaZi4EKEX1JKjEG8qM3GJEO1UQfLWXmMikzLRVWBNTLCGGXVd9GLyWBPgkyuCF6N47D15oet2A/j1c2LHeFDoCD/5LyvVPxa7902QaJ27yXYFqBwt0TGolezOd1kbeSh/HXtmcahLUb+h9lJ1br+AdPSBB2UybobzAlLFEF5QwI+1/kgWNQ6nQBMuIBYaiF1u2yWE8IFDMQGO2BQGkgvkynEAqOVaqlvMEwrwmWnnvg6/a00iP9tLgP3A0cgBy8a8PMUChdxGk2u2n6BgBYcQ0mJ4L00nnBZ2ILl0i+T0RJUxRpGnnIEadGwF7ceCHoV37RPgSt9bSynksWAlOvbPSq36BcVQCdvkRgsWhw1GEdyNk8Vwo8vz6J23kJKeU24nWCTOttMs4KfXNPe0Q0gfpM3WNbr99ywCbrvZUZPDKitWVOi6uQmXRmKE4G2Hp83eP1fvAYJeJbGfRc2w2nieMhDN0OTJo/wHZ+n//L7249qis3sdf/qV5Wd0paH8hFMkEkQqaQTUpiTVUHO2eeKhybqW4vNdhh54NYjsGfZmEPhc68DaeRqaslLuoPR/c95Y2KBGeaZ7u7tQjZL+uOgQaz4QP0rrBVUexAzvAYb0vIZWss9+PGDrhHaUs58t7vC7wcKy/qsbVokmlo9v3XIrqhLcuovawTY+FeIk/qHaKQaHUC7BlPy+vKDJBlpwveY0bRpmUmm3kv/QT6CKeKlDhPIEWy3aOntuGakZSRvL595FRV+IF5l79pMHYOIoZZ8ZNIeBU8jZnRxPMGVziogAfVY/zwE1zYOqR6nQ6f1ixrHsIgGjdqqvm9HSd8nnZdX+C+l4YzmyH1Nt4hgAUwQ9CD4KcA8EXPy6jtkD+hLaYBX85jPGNLJ5jf430U8cmW8r5bv7EVvCx7Gjh8669QOgc8N48StNq71CSHtIhZw0R2nsF7bo+78GJC48cJ27fgnFxNC6CyNQ+7Sl8Y1Wosx/X6CyEu46u25gh4p+AIA9OPNvCan2c0W2nqoy53ROaNzrZvcqqAeR5dG0N6NwNZALSD/t3UeCD0Dt9vvS9CcyCwujPBi3DXmV5p3gCXwo3OER/a6SjZLfnpUm1imQnBB9F5J4fC1SmavWnnev9ZpVwjxOxttKDccgEFZefstGBTR9w0H16a2oMZvOXlFoWqAwHsAEtlSCn/4Ta+6id4+HDbuuQtMqJWXpprGQ4LIfzH9zMeD63AWSzmDPD15Pas8rRuN5/5nAONCQgwcGF74BqtGxB7OBvLcugUcC8Z71t4/P+3WHqJRW2UDrrEHx3iXbzv5863Ct1Zn0efnEM0Im7YOvaXX7tZZUecgKKZes0fc2O2xJs/nWpBNR5NhkQtsdwo62Eaw3TeIeak3ZIAdL1LG//KzkMfmHI9BwBfIFgNGtsvbkwPTKNe65U/ObiXcGGF4mU4Wc6wU6TyIBSEjYUwYeUOD/acyE5mbf8mBT4wxiQ2oy2YHlOSbHZmjCRX2qcBJxoVY5IEDDGQFr4rbT2Irces7GkvuSP1KTQs6PCTGKJuZ3P/eNLdCtymGRMmj5bi720uATU09HhJ9IllvsADcMNMp5RhniZ8HI5BcI6+ZlHilpjR0kJ4J+FPlk+7y0PI10az2lf9S8GVRlEfewfSLN36K5TAIiDe7uu+IxCKu1j0q6tXBAy5bv0wFd15BfGWKbRRWNWvqzwJmZg9RzFTsaNWXPhcORBahBLVsuSvWbkhVz7UvJK9ZwKavKhWMlX1bwC6Ayd3fudn1o7Cu7DEm09gIpqDJS0og5Vn5D4AYogNbNMosviAhpFtIp+bGGEllrwEEByWUCGw81eFf0BNlikJaWAcMyxJZZjDivblXhwaWQxx0FMx2HdqNfmxNwkC9aKLp1c/rn0qs1HEb5KyQIhA9k9iSB4KtG7karM4sYsWp2hyohJAaorTOdrMiKehs1U0MEZIF2Wxj9dT0hC6u79fBTuo4kmTzlAUQ90wYSXWtqumN6+BMJMhCHHCb+D34c64WdVO9nuyCvpoNN04KKhSAhYodA+1HGmbU/5a9c7kroz794Dpnv+RIv5SA+KpIrdHDwnfXd4A3WXLpXFOKk8co5kfSsAk3hWz0xUVkpZBufsgdJklzD/pRDedKAEJZRLtcik4uJmUYDrj9r+rIfpD9chWd1SqyOvlsHwurV8qopU3Q3QLxGkEH5Tt0LpkNrMQheFLzUnmnHoH5JjHM1qPe8yr/z2edGYRO12Xp0lbXnAY1d86rpLLLPlkYasboQWvTB1VfylbJikiy0X/ZARM0iBJrySGgzU05Md/rHsLksP5QvqnBnFKsy1lCUc3Gpy27d8SpweIB+CcA63EGn/2DO2LbMSprmp4FZhRmQpogr3LRk+ZJYjp6pPiHa5yyT7mRLxg/HJ5a1tl3pkn+pamSleW897K/tPtZewfVaKwtgZ+yPcw8OaPC2VinG+wwonEI2cwrJWdbi+CkmZiW/iEEx8O71uyqlNG7s6XVofxsmIjzlUeYYWuEHr6dZmH+HTmxniQVugOU1c7hKr86AFI0KwK7mAj554YgDwuUZU4XvdlPaq6yG+Dv55Qwv5kNlupkzbaiLXrZAtitz7veSr0jsi28MvMQlejJXQPv00mCY5XA+k9g9QR+GAKaYPRWqaqcjjMefUAq/IfdRGpgJuAIt0NzqAAdhvqGX5o+LU7VB0Y80q4mbP0vUQVw/kOgdvdezhCvWlFmS2iEB7nanpkYCLlCKJtnL/OADPQf79ECLZjTOuHguQrLwCfTdgHE/zEq6WzKOelHHQNG1nhGijkLHP5Lb+Tp0IV4G5874VtlhwkD6VqCk+Y/Z83Mv9jCdfCqXxtJ9oW6g4lwGkrovG0b8TeXsWvt8rRxxHxxNXISc2iK3tWmrPqLl85R8+smOVZIJRYo49EVh2UjlboFvJTVRLBklUS48OEaWyn5Gs7lGbe2PoSS7/tD1i0TXCzUok5r5JzKm3TyjEABDbKH8xO5I81k8TZIMNZ8pJkuNjf2INTeURfB867dFFLW/wL3uOcZtQjXFUvRsMqSZtSfaKHH+eUbAP70vurO+uWCHiQ/uWos+9m/PpWpsRwlyrJa0/u6FpkC+iYIhscvZNKJSNYi1CdttOfQm2kZeHcwgT6C7wBY7GoeGCy46DwTpWKI2w4IsiVvWNjx65+6DC2mXvMi2mc81cUfySjln8mIT6R4Vdx1CiQIXeZ/JV5XQH0pXAsszII99UTMa/Dz3oXD8zGCr7W8ijGRk5/2Zty2bPIHNyWjyA1ox6saPIs/mKzUQtJImstnotvvGfbBxAtS3fy3MylkGA3Xa441Xiz1eDJy6lSpCuuIMxKIcsappixDMpz4afaL9fNmW7Ef3ymtDe8TeGKerVPN2oGjVGbfVj1wuSdtLtSmYWrJmq22cf6kWCEOKTpZoA6Jr2EztcxC7Yq9QXncmrdRQyGAUPRt1682LX7mqpgVgberXX1jSCTR44k5ESSLBBTPOcISgfv6QVVscGPSDOyTlKAfLJWffypJdHF1VVrY76jxNn+SmMzWieguj4x10PU8aJKC124KmCcIRMdcLpiSOpHvcp3jcx3AFcEUEg15jiEpUMuG1LcP4JyJspwIKObT6nGGK8oNQJeUg6YCctyI6x3WwGigVbZFCy+xQrIi2ffwJ3dlzT5jWG+tlUl5H7dpTtsB65vQwBaPGHDhcMHFinLbLsylLF43muc7muwZCLWcOlL8/xeLgaYd5xVUU6SdvAR/tpHsKxGjR1Dc4UviN2pA1zMMl62uno5Mn3o2BP+hpcjKTx2fkDP5oEwfVJvpp7fEmSgaOdmLlod5vl08t1FFUBs91IahDgBL19NL4G2Ce5089LsrvdFIeyxvZQhKAvxvTkwO1ndfnkj6wZkCTZxh9NDJ7+0lJwwVqTX6h1hinwz1sWwoIvFdIluZDxq6ynzwEMgbbgK3+ii5iaXyKnF1lF52o4iIXUs1eHi4jOkmzLv1k6RlxUQpv6mQSUXLegbZB52Dl/P5+Orcr4HjxSHN1z9u7XeocmBGeAN2HNnG3gAZuPUjGE8m2+m4zLYfp/tN8HW0LNHON03uOq6hIv7zCEfNW6ChtSTXy4FLGnaKfYBG/63LKdI/lVK3XoE7kJ8cXIOHP200KagBIOH3vY3KcaC5gMG5EahVETn03pZMgJQQaA7N0yciRE6FHnCIpRFwCcS3pwcwLZrFsRjghPz09zAL7LiPK6kYbCkJekuL6Fz3DrlIxYdXdXkzyOhSJvRS/+yf+E19A/Nrl3fpr92JlMYx0vhQhfCDAeYBKohEDJC1z0VK7IVIdqOR7LvyY5mIBIU5ylFcQUXR8hSQeaJHnbvAYmPOsCESgpC3kWmbfm0K/cw97ZM1iZyoXftLhhmtJ3gOUeoMOUqc6M1W/nKrkAzxwZtr/TFCQi9d9pfg6ikzAimedc1bJcOEw/wc22pDEkAtjPe6vBSm2DYTZf4DgGEvrnDTz1LfUmwg2VkTLhYaHiOFVVYpFzIr5oJY/aanIVZygwfuJSpnTDe7/coXH8k0h/qlYDzaHX4h3BxwavBhmBOTgXXq2ichVoj5oYWK0ey40GyoUFZcLo44PP4ZCCLifv8uP06q38Pxd8ZTh201nE1xTCnVbmtYHu1NkqptpX2hUuWO2BzDWcXyK8URGii4PNVUVYj4UDKSxhGlCR9d4w+iUXMohqWC4D7ATMrCFG/wj2hpO/9m0sG/cvhlMR99vCgeMHbXnleF/cX5VRM+fmcTGoIvOOAg1bv7+118kfb6oCQTAvpupcHWb/mn+58ttcBK/BcB0UTN1DrNHPUQNi4giuyUjRAEtNK739SYT6kaIM1LYnzxg6i0+Gb5o3020ibo0C/aHQmxSuaG94l7Muzjxs/aRY53DgpHvfbs7YasEhtwnXyMXhwBsO+MgCSXXjHof0vL9PhRx15o77m1NiT6qzYLqbxFR5PJs1kkhE9YG+K/olHYaN9nl07C2Peh4eXevIlhBs3T/CZgK2kf3Z2uygdf0MyirqGJDyGW3H1ElPxIcYNgTi0h1iE31TAKqmWHgmN+a1i4FtDfCXT79TIed61jchNF/gd3Px1tTumTYP79MW805pbdKMjccXRwZwxozJIhbUzlgevaFsAtXEA9QQqT7J4T+ehKlRRa1HRZlJP1tNsdcJkv0dFvZlbnSRcgcVvaCFk5lBaLUwEiZuitcRFPQ7H+jEVd8u1tkp8xcHP0e92ucWqW7ODXMS+MOTSlrEMV2LECdSrErhitAIQQFGXo4LbO/IgmpaOyPPC09oz8k/ClzzAa+jp7RDM1VFU+VYlejJtrc+vkDLRX2D51d2AwWVCJrAecby+/QKsGodZlyTZsEM+qf06jwpN7MPjM1xyeji6PBAlRXW1Cj3Ni09fzoV12HAPDNt/YgI2MrqTpGsWrdhqoFCpgt7pOV9Q5adgK5+Q8l2uYkB0N04X8fYsHDX5EpkggSGQUg1KyvOyfgUt8ravZh1W9BkEWnLbDhVA/zEoVnppEqjJG80CLSd7tpJnzGULHboaLDZvYBH3mVvBh7kzNjEg8sB+FMZn078b4yOnbVJdWHiAAj22y3bYilG2fY4v8gmYj/mNWKnPvSwsBMb/IysM4wRX2GfXD0331BGIOjG/83wJsLYuBbdIpX1tCLde5uqBSakEsFqYf2o9aBwZYRi/0+IgElflaX3VqES12NCCU6Le9lpHRXiFoXby/fK5XVOZIBj3reRXnPrgX7NNGjnpjNLZBGwqcH51NgAvQSl8YTYv6LgNJAwm0UMhdAgrWNiztOIgtzQySSc6sNAFNvtG2dF/6jW3ue2YJY93QpOEVtyjBPqiylp3Cz23v8b7wiKNPhNSVEdmfWN8JjukSZRV9ZQyZQex0aUoCZVzZnlGl1cwV+mkNxtUGYxkI8no7zOYd1GDbRgNBuNIPdW9nbki9WWz7Ln2ak9ip3ux2gLL3d1kTkgngdZYK0sz/nK5OFy6OImYGIFGNuYv25VHSxz4AyOu+wAZvecZDDZ7BjQhPBJFmkp2aW3anPWLhRLUo+6jcmotl4+/IuSdQ8PqPnJ8JdqgwGraEURbJgDqsE4I4qx4CTgepyOIh1pwRH2CB77XSnCrw0BRzwS7Kbhh0a8iCElzTxVFbw8mSIVjp10cRvjJmIq0/J/8Pw5xnLOddd//aKefWOypUIXgCm6+GLHG7Gld271zNQOdmssqsO3OVlrIUw0rfRH4ggXF0YRS3i0o+7Ulu+yiLFDsIyY0/G0fxLz96+Ghwg9P5klN4iUR6uBn8DoZ+XvwwldZ2OvfCJvUEsY75hjXTP/FXLToh2QaxxaFlQlLxYxZLcxXckGweHGv3bgxM125tqmu09Z3Rg50xIhX3ybCiuw24+s5ObGuMcWcP5fHEjUii7DZo03tdqQaRJmCElN3CiOisb+XFrv6E7reY/UNFiExhea1KnMSifwl1DR7bFjLLe6aHRZo+uryRt9piii672l5QRKm8CWl43+sLjvx6OFxnT7GnwiyJ24UfeJiT2wGv5N5XkHPvz4oyQLAuT6sp1cOynoL8c+0IJjs1C6zuNTPLw/zLMCZ8oaaS4B3LdUKXPuJjwHqXwMvZS2pHagDWEFAlFNpD9RX+wKNXKG/pojPl6azyfOC3FoDC+F+hU3f6WtnZj47o4ZbQ95LYXD0/okoMtR5macQivuQkoMJqpSqTRTCU4wJjz+O16LrZr/Hu4OGf9I/GxokvjQ0dRyI3uE9GCuVOR+qgAQdHacSqU3Q0waHtRvC1psx6vrTkMCh+D+I1/X19xB5TFUQlp9bbUOqNqYC6amVn6o4WvKZ190CVs2fY23Zx0zmrNmNx0o/6aEy6oguikpz5iXFyT0J1nE8v02pGIdNi5Q9T3a/dCmip0c/aj51Gke2YxzjG2bFwrvSaq17QxbKAdSz+3Uz2pfFPw9UKkiF9vqdkBpr5NuTEHSvWhE9t/usZutskCcj6OuMNBTcPS0z+UTJjtcOA/vKAeHVBNbZIhSOBgvLQ1YKleyubu8ll3lijbcgGpWBhBCIwNofU4vYzwYMWmDTB57ZuCMzA0Ke1l52YtHAXy4UDOPEm6ZXWhKulPNwA3LD5uH1EgTOn1xMvQpsqwctAPemwBw4+z+rIgZ7FDAFH8lp2tledRnZ4YVFRfdmXsCUE/Apzapbqvx9IIK1SyrJQHp872K4o+o8GxxutfKhRrwPCLOxc1Oh/hAqUlckFpw1T0qFK2W6j26LDLdT8tLd7EzZkKyB2/bcov3H6ExmBYTAQpLm+fB5uk6Fj78wndKTSXtZr2yKcDSotkSYcpNsrGYxWcEM/lOwibXVe/l4sXl4mDMnrlpFHAvg0bBQ7g5123JJYTEeKSP1WnG0wXyDHYjCS0Z7oxnNN+fuQhxWkm5C+Y0YNn+nhSPTUhDtW15i+Dh4fidJr976KK4Md+IUI0cERYBruN2EsVksf/jxZ47UXAO2Ye3f8+3AiUVnJcb1Rp65YuUTsnYOgquXOT8EJSkl2IFpIILhrsjEZO859VMvWIjia3PQk3z5w9CMaZtoqnFE61TKj0lYTzDdnSK+NgW+a3WQ2bB749TpyHwaVIOtiyAwRbigJTBRttVUZ5cMAHa1S8ekGTgVpivZYibiFuH0wIAkIhju2QSUGd/ntWja+ITbvQFcPxMXEjdKkQJAf+N/dsJYjnrCVo7ZWiRAIDqav1d/16VF+BbAkmMpY458mTVp06d3pezoF+LMfFpRxcHkirSFePdKXPMIpcuVH6f24u7mOzq9VAkSAHGVMfFJGhFwoC5BASLIJHNRp8SLNqYGB2yqAgK/Dc5tKq1dEm2KPFyE5sd9oKoTrpePtiP6zFsDRO2VA1jBzbQWSWxnNrO5Ae9oJK702J4SDYtDPe98TV3klPcYLzUpCQgLfRI0LdkAKLnpul04noHFzCI9pEyBSBdMxg/HbnroaZ3Y/cS1VBv7+CwQIuCipS9U3Mk4eLciWDEFSCbdlW0+Lv2VCKEdD5ofP+KDO2BUfrBlaun9KHSg4jiKmR3Ib/TLTalhw6jNhvXvODd1X6D6CDMGv7Am/qhT8ETJXIDZz857gGVaMXBE3NZjwxDX8X9bXq+uv7zK8i6Mktp243Jckf990pdqn5NRGiCL32fw/BNE+zO33ypWfsQsD6Xpz1LopwYdHUwS2Onll3OMGsw23ANj169VD6kvZhcajAeaW5jfb8WcIhGAr5usfM5hXZyp13aHhyUuQApaNKh3TTXLvs9zovwDC7aAgofaAxIK/8rQUJXOF0bbLZm2swxMCk7J7qsV8nb33vFmqgLnxIuZKGQ2QKnxvLduNo+Lkk6q1j5Q1sf4UP/o7ja7CPRHcgrmi0stNjG1VlKHPgMDbiiViHRUeieDU0nwWSgHd30+oWdngR3hx5nUNyV9Ok/PtkhOwBe8ajiOwhOsnnqQMUZXAb/0j0ohU/36+Jyi7oh5TtUpjajsxWOtAXRl1rZq9+KOXsH4SbVKCZZ+z80TmVW776sVbZL/RMaZMaapD73oM+rCZQ9S68AgmX2HzIbj+fQfr2fyV1hR3rKSRXnedaVQT4I5fOHvg1SOzW5QwWmCfLm0y+7NzcaBZi9iLbAOrvmzX6lybUYLtKPIWnzMZjWDlZU+ft7/spkL3Vx+gX/bCLfExSD/KgBC1Aq7ymN7SINtS+aYKaw9G8gaZYJq7XgtLnWoXfqgQlAZG9wkkH+5+Y7GMQBtNYPnED3NrTXKJGFdNZLtBvHe9K1a0Tsp2ahcfPjwN29JyTYE6M1TABDlok7jVkaYlqOtV3e0OUUMkXDWFJAkHz0Id8PdKNn5ys8DN4o+0fOxr5PGfpP86hyGZ2pOeMvIaFMk4jTbDlm6CaPaXoHGFEhMhl3sV2bZqxJcmPVROAJmwRyEtClqnrfKFZ50Ze/xD5K5fkSPMZHL7DBH0Tbgj6D8v1hltAfevqVpmjTE19CVllyGX2IeTJYsYElzr9gA4tnTQwygF/2X3IqvIqcQrGaW3aiW0DhVjE1giDVKdAI0i6QRI/oajB67R/HeogPeloormAXqBzKKprH6SfN18bLi+eQx7idKeY1lXiB5kWF8DmzGV28bJgVCQcLUyKkYF+mnqa2Kmw9pMGtAdyPGHhjQCxJCLvYU3bw5Hy6S5RPxb/1791jSKzlPuUuI2RoJ3vj1Zyy4r2d/UThvQ9m8Gw5U+y7kLHmqWVInElCWKLnWYkWoHwdL24UPIpMF1klZMgbQuhhQ7Yo3NV6dlEdg4rxkRrzJQMnGWv6rJQsE/dRDHxmlB4+Zdvsgo77FC64g+C6Ofa3M34UyeL291waKTSsN8+7XTy2cXeFH/JY3mlaiPGDXn4E2ire39Ags9AqJ4Vyjpu2MSubrtY3Lfj5hEkd01msVS/Jya+8rnBpmeEiXnY0iIZMYt9LtATKdFPo7LN+uYtacQCjdfBQzNHTqZchk1NfpxTd2V3oOxGq7wVvUEvUAg5tAaJtSXCIOuS8SWpVUMBUk4q3CX+mDgW4msYirSgWmwTBScjLP/UtfFZNafyf41LzBDbOjK6cv+IlIKlSAiXwucMg8LJb4VxIRUEOd5X6vNS9F2Qyp69uliTud7k3r23wZTWbyHMxZsvhxb39qO8OfhixseUkoYjniFMQvwwJzQKawKrnCt2HXgOPk7f6Dbr1ftq1LjNdu00bGpnr6wW12V/G4XmJv00kMLzvxWYfkWtMmWlZFuSivEic5t1uN663kpFXoy8KYn1qUjYCaM8rJWUfag45Zq+HLM9yP7v5nms8IolmJjkFaJz/dBM6jGBLEdWq7MG+7e9yrR6nY63hfkPTqK6UdJZTaMJJfJqsuuGFcu8sBxHZRDD1s/FBqoml2m8PPGMvvMDicVw3vHAw/SjKWDOSef9W+8NGti4vNaT/VAcajcllC6tkVNxQ+DulpW4/5a01e9cx1a0Qh2+pLoDdS50vc4YNAtS+MRTRYcaaJFpQzw+kzQkdi/foK2BgHGY1qE3mBlmBqtbg9C+RA8Ok+LEdh1lzH9pQ4xhwwTxfqPip+1zN5NJ5SJxC7j3NZSmrjRY4NWIZZmyA/kIVwJGrjAKNewlyKpzr5MAy/+q32+zfCYRVODP9l+EaLZoXTD5T+bYXV6kyUxKujuZFKNYwQhQNarh8nLTJOyGvZF2MZuDmLtr6ay8jIjROVBGHPi43Tntc7T14zScDwFgioOQlcNQJZF/jhgCCuQ0ksxxq4ysPKG45IqPW4qnkpQKdPxZRu7WX0ZztNl17Lk1l+3SgP1E4Hegl04UaeB1QWP6n8Zd3EMFP4BXvSakcmOy+xaY1OiPCzMToEM0FAxdu1E4oVEtzGSCJS4UxhtaIQnUNEX+GZ/Z3lDZd7ThaMU5JLNzBG2XDy4k1CZNEHpcMCt8eCGkJ7qB8mq4jjae3m5xiyED14u1r2htcgVMlKfQn5CtjX4qP7JaQ1oJ+CMGme+NqBO4OO1hsOb8nUG00JPPDeMYE+vnZ3R0D0cNGZFvcAYvqGbY1+1WAexLncA2PnVRbOKfISfHOt/oVeXGkfA0EENBahj0Qc12h36WT1uU789MrLKTZaK/Oj8aa+CwExebwFAFDBEIzXSO5Z/jVxk/DsWdSuwRlIfUTQGn4v43SNbd6q4xW/r3iCY9fyGeLqk+lE1IFNGw6K3ZsLfuazAUrAJ+9tkyFQTMx9UYgfOSk+5enqu18cMfR3L7w73tO/ABMfV8xRRWOplC9U/c5s8P7kdg8VP+9LW0puUIYlQqQ8olp6kdWGzKlWFRk/wPvQlVVs5iJxYTcVOG2+YJ8AqPGUVh1WAYTztrCD1gdnoNOhs0Ba55lbsoVVab7Q9dm//8HA6OnK6ZYmFqLB7bwXuWaKYFe7gg3Frx6k82PdvauO0IHsfT5TC6Txl4kzOyyqsFCye988s3QhFGKuecMF5wjKZJs3HGuxFexkgXPaU5Nwggu7aEt7B9F9KPwy7N/bIlnZoPiHFvNMSh/7rgChEekwas2tfO4trs+QqcZkdQ6JD86RHXsInQb43dUjGJvn9ktFMTJ6ZF6n4xM5DluNjHxP+r4UZFXvnUhEosCjSm3zAv43hITdbCPq8wCRzp9+vsT6XvSWBXtflxZAs5dWzICBtFoHLaAXVnY/dymvSUfKFOfKPBj6+PXxfVKQF7Epqb0rVdhNHBjB5kZJb/UgPMsZCuK3tQPCVLcoXM60zbDI0TQn13iGyhQWskwfNCzzynYo2vAUmjXDwdirvrisyg87XxfLdi0JqRsGZuzJJEC9iEhgmVP2YJRdtQp9zet9/0vkKaUocjUgv5ORmjVIP8OVmbJR8S6xvdCmV6yMXw/UApxK51Hs4+A9drppvudvnxK/VD23cymrQ16lhASk1xseNS/zhwKh59SOzXLAAq7qVeqWjQMxMETzNF+9qNRMoKlUF4Nq6k35JR7ryYitJA7bp8iwFrt9c/tDhzeFuo2lHiLvxu1sqe2uGxCxul3OvUktrrw2ohysfmjj712pGzpHMj53t1EttjQV/DJHeDoYFMj5PxoS9qD76i7FuTt6k4rjksrVTV9RxCIl2MU+I25Cv1CrOIITCMrA1usVr/J1NYJUx4NNxIptAnWeteGJMoV0aog97Iymm/IAfG/kNTX0aNSBkt25Ke6agB2O+uvtLrjQ5Jg6sD9j4zwPyOviZ67gAJ9PacoSl1GqCRJypAP2yVM6wdRXaNltqhldxNlsqhByw/jNQLSOTMCEirDNukFH6dI986JnkIbPnXSBAXWbsvMduEdVA8IoLKgf30ND6sKNGGqwi4X+IAXSXbAmEPlB7Dzc6iE6xzH89hiF8fZGgzJYQYkYLfmP6U1nqS9EROk9fZQFfnVzsdwjZO2Z/d9+2kb1ci1yLu9F3aui37ejQGYK+PHgBdX/n+7PpgKKlU2s3klivHrkHp4H3uWxlrDr4zn3Aeq1RBnFYEs/BPM/ZkLB9yfV/DUvAFRuFc4nb6fUv0d+OyK3B9js25n7MoFzz0inWiojGytHwQ1JbDEu12Hnt4q2ESzSEneHJLLhI6UPSNJQ/CvHpWMlgBOWofUp22ZdFNqvZd3nbYA3Qk0W8KDm7bxRQKIqXKDBcJOJj/njpoNuH+0gudffwWQxIjCR7mbLeaoS2os/J/huu5pzaaUQ2QvlXbYi6wiLT5/AAeg9ANY9x/KkSohpZS5vTHsjvrY4n30YGfJoBg4gWzCOwkV3pqABJmBiVPMPQS0JqNCj6wJknRt4krpGuZ2MWXqzjsc7vBAtqkzKcP/um8eJSlXuCsoFIfK0L5Nz1Gpj0/g/avjfgwplePOZyeIPUWqYyyNW1IybjmL82x/xxF9Whrr7xa8EybNqdyMLcfQrwIw9Lo5XNKyvnSz0mHOgh1r0+83UBDNSnCCYqxeoG6L43gFC0/iiJf5uzHTsZxxlj91Mtp9VY7uvW1V/6nRgYAgw7Leg==";
        string detailQuerySumRowE = "sWcpFGVmAB9U9bvmsUOplTSSqHKmm2eHph8nr3P8/WOoePUvMorYm9DBZ9sQdEyrr4K+/0fg3miB2BGv8QTnl6qHJON5JS0g584aS8AtMI/llwAlpa/lcPabNOwbPLMq5/IwTxUrOWF8RbEQ37t/Ns9x4Umsz+VI3GXa5dJSPufUpkZohgvI6tMJrLW/+8fPZPJdvzlymcQdkqPs01X3rdLqiaZhXpvM8N6gGelvCdfVE9+SXdwSn2QZs1WQ/JCCYc4U/L1PlAaAy4CWQbbfOH2YMAnU/rYZ+ZD0cFpIUQjPgZA6uct5jcs/n+5rZd9Kskk57VyrlDYspCjIp5mbl8W3yf+NR1BnJcgD3iJyoBKE7UWzP16h4ajJpc4B4uzk0PWWl+XgGNKoKRibnvvx2L2eQU39NI+MvoevjBghm+PbB1+6hHpT9/nh72ThqRxvG5hGwOZ6n2qP/t6jZw0IFq2CdyVrJVMC4U5vVBjl4SBTCc9dm04eCaJUGQXETcg+jjXrjZbm30QYy/pzcqXPUQdgJkosBwHCgkg7mY0tAe5/YJzF9d2O3CWunBjGGuMFndMYIts9CyqnRfg36CNsDCjnBGgUW46rMPH4MZ7sSuwNv9wfOYHwoTEsbMiBi4LfdDVCGVL1XIAfF9wwhNdJc6KavyJgep0p2lV1m44rlYoJ1nz9Q8V4lId9p/ea8DveHXZO8+KWss0RSGuMIZ2uBzUdwK7NKWjgoXkFyZrTNO2UKoBtFaZS13jYg4RWKopCKeIET9fBDxn8/zt9HqdM1/Ejm7gaP3p2xzLiY11I3XSQo1XwG5KXtXuRvE0MzY4Oe09JgbioukZZyosE2EYZwmYWXvv2CltoI9AoDjjPJ+oG7Wmpn0CFm2fSYIdgRKt4QXnw9EkdphojmqGYnfUwLxgQJrIuVFmY4luW/QJjm1Nny/zk1C49IEGzC7hvuv0gJz5w+72yniiHyRc47V4hdv4RmfhyKyM2pq6moc/vzbER/0HuFJ4687yVxvK7+QgzSaMxIh5uTAyEibwzbpmIrLQmVEFg8yRI6nTbVG+Q+HAwrD4ZLl7X+iigxAh/L06MnGb6n3vMqZMDi6Vwc0xuEwtlscDW4GKdFJXQ51N4z0J0MaK70dFjhHrjlfaTcXD9kNpezcEj9pOXtCyD3wS/ddDufkQ+5aKpvuClXK/5lqekn6zeNkxVdLrjYH1UtLZOebX15XArU4A0UvKA4NGt2KN/X94nWzYqQyDDIcj1PFz87G/YEOTYRxd1VfTeCeohkyipCgC2Rp97S8ZdGb70SkKjvzO5arXYdBKeMK+9NRJuMnTmnq9Vnqm8aqCKms9tLJcyEhZCQrWxWhgMlf0gBldWMtf+Y3qW2FiqBxUySQL8D9C6ircKyNdhjh/noY5j65bofP/XvZS8s1rgcKybngd8xwpmKfj53Eyhf0r2nmNTKFbEalVZz8XPuoAlBqvQ7KCGQw02kvSwcLaGorxKDFUPLaXSfZ67P0lLvY/wduw4ESGZXXFrADUCnczTwCqP7sk+v4i37y39x/F7EXDIjJvdKE/VfZ+gkNab3iRSwWFvgDoMRcZyJfy0XO9gljhKCTPlGc6Uw6kfQ+SBDjwhBG5CoitbH3agA6842E/+o+YI5AqCOeX20QVGQktBeflyUoW0LApMWW2KlDbOFwoP1QprDoIxmxb5RuGXXnhQy23HgZ+0UU2MIAOBhc+junZjVlD3Wbrpzd7i8SMsQ3O96ICfpZzLZ7hVrxnazxslwRNpr4TzA27d+9Sdmwyog8a/";


        LogHelper.Info($"=== 开始同步数据 {currentDateStr}===");

        using (var oracleConn = new OracleConnection(oracleConnStr))
        using (var mysqlConn = new MySqlConnection(mysqlConnStr))
        {
            try
            {
                oracleConn.Open();
                LogHelper.Info("Oracle 数据库连接成功");

                mysqlConn.Open();
                LogHelper.Info("MySQL 数据库连接成功");


                // string masterQuery = @"SELECT gch.guestCheckID, gch.openBusinessDate as busDate, gch.locationID, gch.revenuecenterid,
                //        gch.checknum AS checkNum, gch.opendatetime AS openDateTime, gch.checktotal AS checkTotal,
                //        gch.numitems AS numItems, e.firstname AS firstName, e.lastname AS lastName 
                // FROM guest_check_hist gch  
                // LEFT JOIN employee e ON gch.employeeid = e.employeeid 
                // WHERE gch.organizationID = 10260 AND gch.locationID = 2041 
                //   AND gch.openbusinessdate >= TO_DATE(:currentDateStr, 'YYYY-MM-DD')
                //   AND gch.closebusinessdate <= TO_DATE(:currentDateStr, 'YYYY-MM-DD')";
                string masterQuery = SqlEncryptor.DecryptSql(masterQueryE);
                var masterRecords = oracleConn.Query<GuestCheckHist>(masterQuery, new { currentDateStr }).ToList();
                if (!masterRecords.Any())
                {
                    LogHelper.Warn("主表无数据，不执行从表查询");
                    return;
                }

                LogHelper.Info($"从 Oracle 获取 {masterRecords.Count} 条主表记录");
                string masterInsertQuery = @"INSERT INTO guestcheck (guestCheckID, busDate, locationid, revenuecenterid, checkNum, openDateTime, checkTotal, numItems, firstName, lastName) 
                VALUES (@guestCheckID, @busDate, @locationid, @revenuecenterid, @checkNum, @openDateTime, @checkTotal, @numItems, @firstName, @lastName);";
                using (var transaction = new TransactionScope())
                {
                    mysqlConn.Execute(masterInsertQuery, masterRecords);
                    transaction.Complete();
                }

                LogHelper.Info("主表数据同步完成");
                //                 string detailQuery = @"SELECT * FROM (SELECT GUEST_CHECK_LINE_ITEM_HIST.transDatetime AS transTime,
                // GUEST_CHECK_LINE_ITEM_HIST.serviceRoundNum,
                // (GUEST_CHECK_LINE_ITEM_HIST.lineNum*10) AS lineNum,
                // GUEST_CHECK_LINE_ITEM_HIST.guestCheckLineItemID,
                // GUEST_CHECK_LINE_ITEM_HIST.detailType,
                // MENU_ITEM.menuItemName1Master AS itemName,
                // STTEXT.stringtext AS itemchname,
                // rcs.name AS rvcName,
                // EMPLOYEE.firstName,
                // EMPLOYEE.lastName,
                // CASE WHEN GUEST_CHECK_LINE_ITEM_HIST.voidFlag = 1 THEN 'Void'
                // ELSE 'blank' END AS reasonVoidText,
                // CASE WHEN GUEST_CHECK_LINE_ITEM_HIST.genFlag1 = 1 THEN 'Return'
                // ELSE 'blank' END AS returnText, GUEST_CHECK_LINE_ITEM_HIST.recordID,
                // GUEST_CHECK_LINE_ITEM_HIST.lineTotal AS salesTotal,
                // GUEST_CHECK_LINE_ITEM_HIST.lineCount AS salesCount,
                // CASE WHEN GUEST_CHECK_LINE_ITEM_HIST.denominator > 0 
                // THEN CONCAT('/ ', GUEST_CHECK_LINE_ITEM_HIST.denominator)  ELSE ''
                // END AS salesCountDivisor,
                // GUEST_CHECK_LINE_ITEM_HIST.guestCheckID
                // FROM GUEST_CHECK_LINE_ITEM_HIST left join REVENUE_CENTER
                // on REVENUE_CENTER.revenueCenterID = GUEST_CHECK_LINE_ITEM_HIST.revenueCenterID
                // left join Revenue_Center_String RCS on REVENUE_CENTER.Revenuecenterid=RCS.Revenuecenterid  
                // and rcs.poslanguageid=3 left join EMPLOYEE on  
                // EMPLOYEE.employeeID = NVL(GUEST_CHECK_LINE_ITEM_HIST.managerEmployeeID,
                // GUEST_CHECK_LINE_ITEM_HIST.transEmployeeID) left join mENU_ITEM 
                // on mENU_ITEM.menuItemID = GUEST_CHECK_LINE_ITEM_HIST.recordID 
                // left join MCRSPOSDB.menu_item_master MIM on MIM.objectnumber = mENU_ITEM.menuitemposref 
                // left join MCRSPOSDB.string_table STTEXT on MIM.nameid = STTEXT.stringnumberid and
                // (STTEXT.langid = 2) where (GUEST_CHECK_LINE_ITEM_HIST.locationID =2041) AND 
                // (GUEST_CHECK_LINE_ITEM_HIST.detailType = 1) AND 
                // (GUEST_CHECK_LINE_ITEM_HIST.doNotShow IS NULL OR GUEST_CHECK_LINE_ITEM_HIST.doNotShow = 0) AND 
                // (GUEST_CHECK_LINE_ITEM_HIST.guestCheckID =:guestCheckID) 
                // and mENU_ITEM.menuitemposref not in (19999997,19999998,19999999)
                // UNION
                // SELECT GUEST_CHECK_LINE_ITEM_HIST.transDatetime AS transTime,
                //        GUEST_CHECK_LINE_ITEM_HIST.serviceRoundNum,
                //        ((GUEST_CHECK_LINE_ITEM_HIST.lineNum*10)+1) AS lineNum,
                //        0 AS guestCheckLineItemID,
                //        GUEST_CHECK_LINE_ITEM_HIST.detailType,
                //        REASON_CODE.name AS itemName,
                //        chr('') AS itemchname,
                //        REVENUE_CENTER.nameMaster AS rvcName,
                //        EMPLOYEE.firstName,
                //        EMPLOYEE.lastName,
                //        'Reason' AS reasonVoidText,
                //        'blank' AS returnText,
                //        GUEST_CHECK_LINE_ITEM_HIST.recordID,
                //        0 AS salesTotal,
                //        0 AS salesCount,
                //        '' AS salesCountDivisor,
                //        GUEST_CHECK_LINE_ITEM_HIST.guestCheckID
                // FROM     REASON_CODE  RIGHT OUTER JOIN
                //          EMPLOYEE RIGHT OUTER JOIN
                //          REVENUE_CENTER  RIGHT OUTER JOIN
                //          GUEST_CHECK_LINE_ITEM_HIST
                //          ON REVENUE_CENTER.revenueCenterID = GUEST_CHECK_LINE_ITEM_HIST.revenueCenterID
                //          ON EMPLOYEE.employeeID = NVL(GUEST_CHECK_LINE_ITEM_HIST.managerEmployeeID,GUEST_CHECK_LINE_ITEM_HIST.transEmployeeID)
                //          ON REASON_CODE.reasonCodeID = GUEST_CHECK_LINE_ITEM_HIST.reasonCodeID
                // WHERE (GUEST_CHECK_LINE_ITEM_HIST.organizationID =10260) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.locationID =2041) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.detailType = 1) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.voidFlag = 1) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.doNotShow IS NULL OR GUEST_CHECK_LINE_ITEM_HIST.doNotShow = 0) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.guestCheckID =:guestCheckID)
                // UNION
                // SELECT GUEST_CHECK_LINE_ITEM_HIST.transDatetime AS transTime,
                //        GUEST_CHECK_LINE_ITEM_HIST.serviceRoundNum,
                //        ((GUEST_CHECK_LINE_ITEM_HIST.lineNum*10)+1) AS lineNum,
                //        0 AS guestCheckLineItemID,
                //        GUEST_CHECK_LINE_ITEM_HIST.detailType,
                //        REASON_CODE.name AS itemName,
                //        chr('') AS itemchname,
                //        REVENUE_CENTER.nameMaster AS rvcName,
                //        EMPLOYEE.firstName,
                //        EMPLOYEE.lastName,
                //        'Reason' AS reasonVoidText,
                //        'blank' AS returnText,
                //        GUEST_CHECK_LINE_ITEM_HIST.recordID,
                //        0 AS salesTotal,
                //        0 AS salesCount,
                //        '' AS salesCountDivisor,
                //        GUEST_CHECK_LINE_ITEM_HIST.guestCheckID
                // FROM     REASON_CODE  RIGHT OUTER JOIN
                //          EMPLOYEE RIGHT OUTER JOIN
                //          REVENUE_CENTER  RIGHT OUTER JOIN
                //          GUEST_CHECK_LINE_ITEM_HIST
                //          ON REVENUE_CENTER.revenueCenterID = GUEST_CHECK_LINE_ITEM_HIST.revenueCenterID
                //          ON EMPLOYEE.employeeID = NVL(GUEST_CHECK_LINE_ITEM_HIST.managerEmployeeID,GUEST_CHECK_LINE_ITEM_HIST.transEmployeeID)
                //          ON REASON_CODE.reasonCodeID = GUEST_CHECK_LINE_ITEM_HIST.reasonCodeID
                // WHERE (GUEST_CHECK_LINE_ITEM_HIST.organizationID =10260) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.locationID =2041) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.detailType = 1) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.genFlag1 = 1) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.doNotShow IS NULL OR GUEST_CHECK_LINE_ITEM_HIST.doNotShow = 0) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.guestCheckID =:guestCheckID)
                // UNION
                // SELECT GUEST_CHECK_LINE_ITEM_HIST.transDatetime AS transTime,
                //        GUEST_CHECK_LINE_ITEM_HIST.serviceRoundNum,
                //        (GUEST_CHECK_LINE_ITEM_HIST.lineNum*10) AS lineNum,
                //        GUEST_CHECK_LINE_ITEM_HIST.guestCheckLineItemID,
                //        GUEST_CHECK_LINE_ITEM_HIST.detailType,
                //        DISCOUNT.nameMaster AS itemName,
                //        TO_NCHAR(DISS.name)  AS itemchname,
                //        RCS.name AS rvcName,
                //        EMPLOYEE.firstName,
                //        EMPLOYEE.lastName,
                //        CASE
                //            WHEN GUEST_CHECK_LINE_ITEM_HIST.voidFlag = 1 THEN 'Void'
                //            ELSE 'blank' END AS reasonVoidText,
                //        'blank' AS returnText,
                //        GUEST_CHECK_LINE_ITEM_HIST.recordID,
                //        GUEST_CHECK_LINE_ITEM_HIST.lineTotal AS salesTotal,
                //        0 AS salesCount,
                //        '' AS salesCountDivisor,
                //        GUEST_CHECK_LINE_ITEM_HIST.guestCheckID
                // FROM     DISCOUNT  RIGHT OUTER JOIN
                //          EMPLOYEE RIGHT OUTER JOIN
                //          REVENUE_CENTER  RIGHT OUTER JOIN GUEST_CHECK_LINE_ITEM_HIST
                // ON REVENUE_CENTER.revenueCenterID = GUEST_CHECK_LINE_ITEM_HIST.revenueCenterID
                // ON EMPLOYEE.employeeID = NVL(GUEST_CHECK_LINE_ITEM_HIST.managerEmployeeID,GUEST_CHECK_LINE_ITEM_HIST.transEmployeeID)
                // ON DISCOUNT.discountID = GUEST_CHECK_LINE_ITEM_HIST.recordID
                // left join Revenue_Center_String RCS on REVENUE_CENTER.Revenuecenterid=RCS.Revenuecenterid  and rcs.poslanguageid=3
                // left join DISCOUNT_String DISS on DISS.discountid= DISCOUNT.discountid and DISS.poslanguageid=3
                // WHERE (GUEST_CHECK_LINE_ITEM_HIST.organizationID =10260) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.locationID =2041) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.detailType = 2) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.doNotShow IS NULL OR GUEST_CHECK_LINE_ITEM_HIST.doNotShow = 0) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.guestCheckID = :guestCheckID)
                // UNION
                // SELECT GUEST_CHECK_HIST.openDateTime  AS transDatetime,
                //        100 AS serviceRoundNum,
                //        NULL AS lineNum,
                //        NULL AS guestCheckLineItemID,
                //        NULL AS detailType,
                //        SERVICE_CHARGE.nameMaster AS itemName,
                //        chr('') AS itemchname,
                //        REVENUE_CENTER.nameMaster AS rvcName,
                //        EMPLOYEE.firstName,
                //        EMPLOYEE.lastName,
                //        'blank' AS reasonVoidText,
                //        'blank' AS returnText,
                //        NULL AS recordID,
                //        GUEST_CHECK_HIST.autoServiceChargeTotal AS salesTotal,
                //        0 AS salesCount,
                //        '' AS salesCountDivisor,
                //        GUEST_CHECK_HIST.guestCheckID
                // FROM     EMPLOYEE RIGHT OUTER JOIN
                //          SERVICE_CHARGE   RIGHT OUTER JOIN
                //          REVENUE_CENTER  RIGHT OUTER JOIN
                //          GUEST_CHECK_HIST
                //          ON GUEST_CHECK_HIST.revenueCenterID = REVENUE_CENTER.revenueCenterID
                //          ON  SERVICE_CHARGE.serviceChargePosRef = REVENUE_CENTER.autoServiceChargePosref  AND
                //              SERVICE_CHARGE.locationID = REVENUE_CENTER.locationID
                //          ON GUEST_CHECK_HIST.employeeID = EMPLOYEE.employeeID
                // WHERE GUEST_CHECK_HIST.organizationID =10260 AND
                //     GUEST_CHECK_HIST.locationID =2041 AND
                //     GUEST_CHECK_HIST.autoServiceChargeTotal > 0 AND
                //     GUEST_CHECK_HIST.guestCheckID =:guestCheckID
                // UNION
                // SELECT GUEST_CHECK_LINE_ITEM_HIST.transDatetime AS transTime,
                //        GUEST_CHECK_LINE_ITEM_HIST.serviceRoundNum,
                //        (GUEST_CHECK_LINE_ITEM_HIST.lineNum*10) AS lineNum,
                //        GUEST_CHECK_LINE_ITEM_HIST.guestCheckLineItemID,
                //        GUEST_CHECK_LINE_ITEM_HIST.detailType,
                //        TENDER_MEDIA.nameMaster AS itemName,
                //        TO_NCHAR(TMS.name) AS itemchname,
                //        RCS.name  AS rvcName,
                //        EMPLOYEE.firstName,
                //        EMPLOYEE.lastName,
                //        CASE
                //            WHEN GUEST_CHECK_LINE_ITEM_HIST.voidFlag = 1 THEN 'Void'
                //            ELSE 'blank' END AS reasonVoidText,
                //        'blank' AS returnText,
                //        GUEST_CHECK_LINE_ITEM_HIST.recordID,
                //        GUEST_CHECK_LINE_ITEM_HIST.lineTotal AS salesTotal,
                //        0 AS salesCount,
                //        '' AS salesCountDivisor,
                //        GUEST_CHECK_LINE_ITEM_HIST.guestCheckID
                // FROM     TENDER_MEDIA  RIGHT OUTER JOIN
                //          EMPLOYEE RIGHT OUTER JOIN
                //          REVENUE_CENTER  RIGHT OUTER JOIN GUEST_CHECK_LINE_ITEM_HIST
                // ON REVENUE_CENTER.revenueCenterID = GUEST_CHECK_LINE_ITEM_HIST.revenueCenterID
                // left join Revenue_Center_String RCS on REVENUE_CENTER.Revenuecenterid=RCS.Revenuecenterid  and rcs.poslanguageid=3
                // ON EMPLOYEE.employeeID = NVL(GUEST_CHECK_LINE_ITEM_HIST.managerEmployeeID,GUEST_CHECK_LINE_ITEM_HIST.transEmployeeID)
                // ON TENDER_MEDIA.tenderMediaID = GUEST_CHECK_LINE_ITEM_HIST.recordID
                // RIGHT JOIN  TENDER_MEDIA_string TMS on TMS.TENDERMEDIAID=TENDER_MEDIA.tenderMediaID and TMS.poslanguageid=3
                // WHERE (GUEST_CHECK_LINE_ITEM_HIST.organizationID =10260) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.locationID =2041) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.detailType = 4) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.doNotShow IS NULL OR GUEST_CHECK_LINE_ITEM_HIST.doNotShow = 0) AND
                //     (TENDER_MEDIA.typeMaster = 1) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.guestCheckID =:guestCheckID )
                // UNION
                // SELECT GUEST_CHECK_LINE_ITEM_HIST.transDatetime AS transTime,
                //        GUEST_CHECK_LINE_ITEM_HIST.serviceRoundNum,
                //        (GUEST_CHECK_LINE_ITEM_HIST.lineNum*10) AS lineNum,
                //        GUEST_CHECK_LINE_ITEM_HIST.guestCheckLineItemID,
                //        GUEST_CHECK_LINE_ITEM_HIST.detailType,
                //        GUEST_CHECK_LINE_ITEM_HIST.referenceInfo AS itemName,
                //        TO_NCHAR(GUEST_CHECK_LINE_ITEM_HIST.referenceInfo) AS itemchname,
                //        RCS.name  AS rvcName,
                //        EMPLOYEE.firstName,
                //        EMPLOYEE.lastName,
                //        CASE
                //            WHEN GUEST_CHECK_LINE_ITEM_HIST.voidFlag = 1 THEN 'Void'
                //            ELSE 'blank' END AS reasonVoidText,
                //        'blank' AS returnText,
                //        GUEST_CHECK_LINE_ITEM_HIST.recordID,
                //        0 AS salesTotal,
                //        0 AS salesCount,
                //        '' AS salesCountDivisor,
                //        GUEST_CHECK_LINE_ITEM_HIST.guestCheckID
                // FROM     EMPLOYEE RIGHT OUTER JOIN
                //          REVENUE_CENTER RIGHT OUTER JOIN GUEST_CHECK_LINE_ITEM_HIST
                // ON REVENUE_CENTER.revenueCenterID = GUEST_CHECK_LINE_ITEM_HIST.revenueCenterID
                // left join Revenue_Center_String RCS on REVENUE_CENTER.Revenuecenterid=RCS.Revenuecenterid  and rcs.poslanguageid=3
                // ON EMPLOYEE.employeeID = NVL(GUEST_CHECK_LINE_ITEM_HIST.managerEmployeeID,GUEST_CHECK_LINE_ITEM_HIST.transEmployeeID)
                // WHERE (GUEST_CHECK_LINE_ITEM_HIST.organizationID =10260) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.locationID =2041) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.detailType = 5) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.doNotShow IS NULL OR GUEST_CHECK_LINE_ITEM_HIST.doNotShow = 0) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.guestCheckID =:guestCheckID )
                // UNION
                // SELECT GUEST_CHECK_LINE_ITEM_HIST.transDatetime AS transTime,
                //        GUEST_CHECK_LINE_ITEM_HIST.serviceRoundNum,
                //        (GUEST_CHECK_LINE_ITEM_HIST.lineNum*10) AS lineNum,
                //        GUEST_CHECK_LINE_ITEM_HIST.guestCheckLineItemID,
                //        GUEST_CHECK_LINE_ITEM_HIST.detailType,
                //        CASE
                //            WHEN TENDER_MEDIA.hideAcctNum = 1
                //                THEN  CONCAT('xxxx-', SUBSTR(TRIM(GUEST_CHECK_LINE_ITEM_HIST.referenceInfo), -4, 4))
                //            ELSE referenceInfo
                //            END itemName,
                //        CASE
                //            WHEN TENDER_MEDIA.hideAcctNum = 1
                //                THEN TO_NCHAR( CONCAT('xxxx-', SUBSTR(TRIM(GUEST_CHECK_LINE_ITEM_HIST.referenceInfo), -4, 4)))
                //            ELSE TO_NCHAR(referenceInfo)
                //            END AS itemchname,
                //        rcs.name AS rvcName,
                //        EMPLOYEE.firstName,
                //        EMPLOYEE.lastName,
                //        CASE
                //            WHEN GUEST_CHECK_LINE_ITEM_HIST.voidFlag = 1 THEN 'Void'
                //            ELSE 'blank' END AS reasonVoidText,
                //        'blank' AS returnText,
                //        GUEST_CHECK_LINE_ITEM_HIST.recordID,
                //        0 AS salesTotal,
                //        0 AS salesCount,
                //        '' AS salesCountDivisor,
                //        GUEST_CHECK_LINE_ITEM_HIST.guestCheckID
                // FROM     TENDER_MEDIA  RIGHT OUTER JOIN
                //          EMPLOYEE RIGHT OUTER JOIN
                //          REVENUE_CENTER  RIGHT OUTER JOIN GUEST_CHECK_LINE_ITEM_HIST
                // ON REVENUE_CENTER.revenueCenterID = GUEST_CHECK_LINE_ITEM_HIST.revenueCenterID
                // left join Revenue_Center_String RCS on REVENUE_CENTER.Revenuecenterid=RCS.Revenuecenterid  and rcs.poslanguageid=3
                // ON EMPLOYEE.employeeID = NVL(GUEST_CHECK_LINE_ITEM_HIST.managerEmployeeID,GUEST_CHECK_LINE_ITEM_HIST.transEmployeeID)
                // ON TENDER_MEDIA.tenderMediaID = GUEST_CHECK_LINE_ITEM_HIST.recordID
                // WHERE (GUEST_CHECK_LINE_ITEM_HIST.organizationID =10260) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.locationID =2041) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.detailType = 6) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.doNotShow IS NULL OR GUEST_CHECK_LINE_ITEM_HIST.doNotShow = 0) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.guestCheckID =:guestCheckID )
                // union
                // SELECT GUEST_CHECK_LINE_ITEM_HIST.transDatetime AS transTime,
                //        GUEST_CHECK_LINE_ITEM_HIST.serviceRoundNum,
                //        (GUEST_CHECK_LINE_ITEM_HIST.lineNum*10) AS lineNum,
                //        GUEST_CHECK_LINE_ITEM_HIST.guestCheckLineItemID,
                //        GUEST_CHECK_LINE_ITEM_HIST.detailType,
                //        GUEST_CHECK_LINE_ITEM_HIST.referenceInfo AS itemName,
                //        TO_NCHAR(GUEST_CHECK_LINE_ITEM_HIST.referenceInfo) AS itemchname,
                //        REVENUE_CENTER.nameMaster AS rvcName,
                //        EMPLOYEE.firstName,
                //        EMPLOYEE.lastName,
                //        CASE
                //            WHEN GUEST_CHECK_LINE_ITEM_HIST.voidFlag = 1 THEN 'Void'
                //            ELSE 'blank' END AS reasonVoidText,
                //        'blank' AS returnText,
                //        GUEST_CHECK_LINE_ITEM_HIST.recordID AS RECORDID,
                //        GUEST_CHECK_LINE_ITEM_HIST.lineTotal AS salesTotal,
                //        0 AS salesCount,
                //        '' AS salesCountDivisor,
                //        GUEST_CHECK_LINE_ITEM_HIST.guestCheckID
                // FROM     EMPLOYEE RIGHT OUTER JOIN
                //          REVENUE_CENTER  RIGHT OUTER JOIN GUEST_CHECK_LINE_ITEM_HIST
                // ON REVENUE_CENTER.revenueCenterID = GUEST_CHECK_LINE_ITEM_HIST.revenueCenterID
                // ON EMPLOYEE.employeeID = NVL(GUEST_CHECK_LINE_ITEM_HIST.managerEmployeeID,GUEST_CHECK_LINE_ITEM_HIST.transEmployeeID)
                // WHERE (GUEST_CHECK_LINE_ITEM_HIST.organizationID =10260) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.locationID =2041) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.detailType = 7) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.doNotShow IS NULL OR GUEST_CHECK_LINE_ITEM_HIST.doNotShow = 0) AND
                //     (GUEST_CHECK_LINE_ITEM_HIST.guestCheckID =:guestCheckID ))
                // ORDER BY guestCheckID ASC,serviceRoundNum ASC, lineNum ASC, guestCheckLineItemID ASC
                // ";
                string detailQuery = SqlEncryptor.DecryptSql(detailQueryE);
                string detailInsertQuery = @"INSERT INTO guestcheckdetails 
  (transTime, serviceRoundNum, lineNum, guestCheckLineItemID, detailType,itemName,itemName2,
   itemchname, rvcName,firstName,lastName,reasonVoidText,returnText,recordID,salesTotal,
    salesCount,salesCountDivisor,guestCheckID,checkNum,openDatetime) 
VALUES 
  (@transTime, @serviceRoundNum, @lineNum, @guestCheckLineItemID, @detailType,@itemName,@itemName2,
   @itemchname, @rvcName,@firstName,@lastName,@reasonVoidText,@returnText,@recordID,@salesTotal,
    @salesCount,@salesCountDivisor,@guestCheckID,@checkNum,@transTime);";

                //                 string detailQuerySumRow = @" SELECT GUEST_CHECK_HIST.organizationID,
                //        GUEST_CHECK_HIST.checkNum,
                //        GUEST_CHECK_HIST.tableRef,
                //        GUEST_CHECK_HIST.openDatetime,
                //        round((NVL(GUEST_CHECK_HIST.closeDatetime, GUEST_CHECK_HIST.openDatetime) - GUEST_CHECK_HIST.openDatetime)*1440,2) AS duration,
                //        GUEST_CHECK_HIST.numGuests,
                //        GUEST_CHECK_HIST.checkRef,
                //        LOCATION_HIERARCHY_ITEM.name AS locName,
                //        RCS.name AS rvcName,
                //        OTMS.name AS otName,
                //        EMPLOYEE.firstName,
                //        EMPLOYEE.lastName
                // FROM     EMPLOYEE RIGHT OUTER JOIN
                //          ORDER_TYPE RIGHT OUTER JOIN
                //          REVENUE_CENTER RIGHT OUTER JOIN
                //          GUEST_CHECK_HIST LEFT OUTER JOIN LOCATION_HIERARCHY_ITEM
                // ON GUEST_CHECK_HIST.locationID = LOCATION_HIERARCHY_ITEM.locationID
                // ON GUEST_CHECK_HIST.revenueCenterID = REVENUE_CENTER.revenueCenterID
                // ON GUEST_CHECK_HIST.orderTypeID = ORDER_TYPE.orderTypeID
                // ON GUEST_CHECK_HIST.employeeID = EMPLOYEE.employeeID
                // LEFT JOIN   Revenue_Center_String RCS on RCS.Revenuecenterid = REVENUE_CENTER.Revenuecenterid 	and rcs.poslanguageid=3 
                // LEFT JOIN   ORDER_TYPE_STRING OTMS on OTMS.ordertypeid = ORDER_TYPE.ordertypeid 	and OTMS.poslanguageid=3 
                // WHERE (GUEST_CHECK_HIST.organizationID = 10260) AND
                //     (GUEST_CHECK_HIST.locationID =2041) AND
                //     (GUEST_CHECK_HIST.guestCheckID = :guestCheckID )";
                string detailQuerySumRow = SqlEncryptor.DecryptSql(detailQuerySumRowE);
                string detailInsertQuerySumRow = @"INSERT INTO guestcheckdetailssumrow 
  (organizationID, checkNum, tableRef, openDatetime, duration,numGuests,checkRef,
   locName, rvcName,otName,firstName,lastName,guestCheckID) 
VALUES 
  (@organizationID, @checkNum, @tableRef, @openDatetime, @duration,@numGuests,@checkRef,
   @locName, @rvcName,@otName,@firstName,@lastName,@guestCheckID);";
                foreach (var masterRecord in masterRecords)
                {

                    long guestCheckID = masterRecord.guestcheckid;
                    var detailRecords = oracleConn.Query<GuestCheckDetails>(detailQuery, new { guestCheckID }).ToList();
                    var detailSumRowRecords = oracleConn.Query<GuestCheckDetailsSumRow>(detailQuerySumRow, new { guestCheckID }).ToList();
                    detailRecords.ForEach(record => record.checkNum = masterRecord.checkNum);
                    detailSumRowRecords.ForEach(record => record.guestCheckID = masterRecord.guestcheckid);
                    if (!detailRecords.Any())
                    {
                        LogHelper.Warn($"G_C_ID = {guestCheckID}-{masterRecord.checkNum} 没有对应的从表数据");
                    }
                    else
                    {
                        LogHelper.Info($"G_C_ID = {guestCheckID}-{masterRecord.checkNum}从表数据共 {detailRecords.Count} 条");
                        if (detailRecords.Count > 1)
                        {
                            LogHelper.Info($"检测处理从表数据-donotshow");
                            for (int i = 1; i < detailRecords.Count; i++)
                            {
                                GuestCheckDetails cur_dr = detailRecords[i];
                                GuestCheckDetails pre_dr = detailRecords[i - 1];
                                if ((cur_dr.detailType == 5 && cur_dr.salesCountDivisor == null &&
                                pre_dr.detailType == 1) ||
                                (cur_dr.detailType == 6 && pre_dr.detailType == 4) ||
                                (cur_dr.detailType == 5 && pre_dr.detailType == 2))
                                {
                                    cur_dr.doNotShow = 1;
                                    pre_dr.itemName2 = cur_dr.itemName;
                                }
                            }
                            LogHelper.Info($"delete从表数据-donotshow");
                            detailRecords.RemoveAll(record => record.doNotShow == 1);
                        }
                        using (var transaction = new TransactionScope())
                        {
                            mysqlConn.Execute(detailInsertQuery, detailRecords);
                            transaction.Complete();
                        }
                    }
                    if (!detailSumRowRecords.Any())
                    {
                        LogHelper.Warn($"G_C_ID = {guestCheckID}-{masterRecord.checkNum} 没有对应的从表SUM数据");
                    }
                    else
                    {
                        LogHelper.Info($"G_C_ID = {guestCheckID}-{masterRecord.checkNum}从表SUM数据共 {detailSumRowRecords.Count} 条");
                        using (var transaction = new TransactionScope())
                        {
                            mysqlConn.Execute(detailInsertQuerySumRow, detailSumRowRecords);
                            transaction.Complete();
                        }
                    }

                }
                LogHelper.Info("从表数据同步完成");
            }
            catch (Exception ex)
            {
                LogHelper.Error("数据同步失败: " + ex.Message, ex);
            }
            finally
            {
                if (oracleConn.State == ConnectionState.Open) { oracleConn.Close(); LogHelper.Info("Oracle 数据库连接关闭"); }
                if (mysqlConn.State == ConnectionState.Open) { mysqlConn.Close(); LogHelper.Info("MySQL 数据库连接关闭"); }
            }
        }
        LogHelper.Info("=== 数据同步完成 ===1");
    }

    public static void ImportExcelToMySQL(string excelFilePath, string connectionString)
    {
        // 打开 Excel 文件
        FileInfo fileInfo = new FileInfo(excelFilePath);
        using (var package = new ExcelPackage(fileInfo))
        {
            // 获取第一个工作表
            var worksheet = package.Workbook.Worksheets[0];

            // 获取行数和列数
            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;

            // 打开 MySQL 连接
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // 遍历 Excel 文件的每一行
                for (int row = 2; row <= rowCount - 1; row++) // 假设第一行是标题行
                {
                    // 构造插入 SQL 语句
                    string query = @"INSERT INTO fuzhanggui 
                                    (storename, storeid, itemname, paymethod, subitemname, property, barcode, itemgroup, finitemgroup, ordernum, 
                                    orderdate, ordertime, paydate, paytime, price, qty, unit, orgprice, disprice, actualmount, refund, discount, 
                                    channel, pickcode, tableref, realinventory, member, membername, memberlevel, membercardno, orderstaff, opentable, 
                                    itemmemo, ordermemo)
                                    VALUES 
                                    (@storename, @storeid, @itemname, @paymethod, @subitemname, @property, @barcode, @itemgroup, @finitemgroup, @ordernum, 
                                    @orderdate, @ordertime, @paydate, @paytime, @price, @qty, @unit, @orgprice, @disprice, @actualmount, @refund, @discount, 
                                    @channel, @pickcode, @tableref, @realinventory, @member, @membername, @memberlevel, @membercardno, @orderstaff, @opentable, 
                                    @itemmemo, @ordermemo)";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        // 获取每列的值
                        cmd.Parameters.AddWithValue("@storename", worksheet.Cells[row, 1].Text);  // 第一列
                        cmd.Parameters.AddWithValue("@storeid", Convert.ToInt64(worksheet.Cells[row, 2].Text));  // 第二列，转换为 long 类型
                        cmd.Parameters.AddWithValue("@itemname", worksheet.Cells[row, 3].Text);  // 第三列
                        cmd.Parameters.AddWithValue("@paymethod", worksheet.Cells[row, 4].Text);  // 支付类型
                        cmd.Parameters.AddWithValue("@subitemname", worksheet.Cells[row, 5].Text);  // 子规格商品名
                        cmd.Parameters.AddWithValue("@property", worksheet.Cells[row, 6].Text);  // 商品属性
                        cmd.Parameters.AddWithValue("@barcode", worksheet.Cells[row, 7].Text);  // 商品条形码
                        cmd.Parameters.AddWithValue("@itemgroup", worksheet.Cells[row, 8].Text);  // 商品一级分组
                        cmd.Parameters.AddWithValue("@finitemgroup", worksheet.Cells[row, 9].Text);  // 财务分组

                        // 订单号 (long)
                        cmd.Parameters.AddWithValue("@ordernum", Convert.ToInt64(worksheet.Cells[row, 10].Text));

                        // 订单日期和时间
                        DateTime orderDate;
                        if (DateTime.TryParse(worksheet.Cells[row, 11].Text, out orderDate))
                        {
                            cmd.Parameters.AddWithValue("@orderdate", orderDate);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@orderdate", DBNull.Value);  // 如果无法转换，插入 NULL
                        }

                        TimeSpan orderTime;
                        if (TimeSpan.TryParse(worksheet.Cells[row, 12].Text, out orderTime))
                        {
                            cmd.Parameters.AddWithValue("@ordertime", orderTime);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@ordertime", DBNull.Value);  // 如果无法转换，插入 NULL
                        }

                        // 支付日期和时间
                        DateTime payDate;
                        if (DateTime.TryParse(worksheet.Cells[row, 13].Text, out payDate))
                        {
                            cmd.Parameters.AddWithValue("@paydate", payDate);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@paydate", DBNull.Value);  // 如果无法转换，插入 NULL
                        }

                        TimeSpan payTime;
                        if (TimeSpan.TryParse(worksheet.Cells[row, 14].Text, out payTime))
                        {
                            cmd.Parameters.AddWithValue("@paytime", payTime);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@paytime", DBNull.Value);  // 如果无法转换，插入 NULL
                        }

                        // 价格、数量、原单价、优惠金额等
                        cmd.Parameters.AddWithValue("@price", Convert.ToDecimal(worksheet.Cells[row, 15].Text));
                        cmd.Parameters.AddWithValue("@qty", Convert.ToDecimal(worksheet.Cells[row, 16].Text));
                        cmd.Parameters.AddWithValue("@unit", worksheet.Cells[row, 17].Text);
                        cmd.Parameters.AddWithValue("@orgprice", Convert.ToDecimal(worksheet.Cells[row, 18].Text));
                        cmd.Parameters.AddWithValue("@disprice", Convert.ToDecimal(worksheet.Cells[row, 19].Text));
                        cmd.Parameters.AddWithValue("@actualmount", Convert.ToDecimal(worksheet.Cells[row, 20].Text));
                        cmd.Parameters.AddWithValue("@refund", Convert.ToDecimal(worksheet.Cells[row, 21].Text));
                        cmd.Parameters.AddWithValue("@discount", Convert.ToDecimal(worksheet.Cells[row, 22].Text));
                        cmd.Parameters.AddWithValue("@channel", worksheet.Cells[row, 23].Text);
                        cmd.Parameters.AddWithValue("@pickcode", worksheet.Cells[row, 24].Text);
                        cmd.Parameters.AddWithValue("@tableref", worksheet.Cells[row, 25].Text);
                        cmd.Parameters.AddWithValue("@realinventory", Convert.ToDecimal(worksheet.Cells[row, 26].Text));

                        // 会员信息
                        cmd.Parameters.AddWithValue("@member", Convert.ToInt32(worksheet.Cells[row, 27].Text));
                        cmd.Parameters.AddWithValue("@membername", worksheet.Cells[row, 28].Text);
                        cmd.Parameters.AddWithValue("@memberlevel", worksheet.Cells[row, 29].Text);
                        cmd.Parameters.AddWithValue("@membercardno", worksheet.Cells[row, 30].Text);
                        cmd.Parameters.AddWithValue("@orderstaff", worksheet.Cells[row, 31].Text);
                        cmd.Parameters.AddWithValue("@opentable", worksheet.Cells[row, 32].Text);

                        // 商品备注和订单备注
                        cmd.Parameters.AddWithValue("@itemmemo", worksheet.Cells[row, 33].Text);
                        cmd.Parameters.AddWithValue("@ordermemo", worksheet.Cells[row, 34].Text);

                        // 执行 SQL 插入
                        cmd.ExecuteNonQuery();
                    }
                }

                connection.Close();
            }

            Console.WriteLine("数据导入完成！");
        }


    }


    public  void ImportCsvToMySQL(string csvFilePath, string connectionString)
    {
        uploadfile(csvFilePath,mysqlConnStr);
        LogHelper.Info("csv changing encoding to utf8");
        CsvConverter.ConvertCsvEncoding(csvFilePath, csvFilePath, true);
        LogHelper.Info("csv encoding changed to utf8 .");
        uploadfile(csvFilePath,mysqlConnStr);
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            var records = new List<FuzhangguiRecord>();
            using (var reader = new StreamReader(csvFilePath, System.Text.Encoding.GetEncoding("GB2312")))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null,  // ❌ 取消表头校验，防止 `HeaderValidationException`
                IgnoreBlankLines = true
                //Encoding = Encoding.GetEncoding("GB2312")
            }))
            {
                csv.Read();
                csv.ReadHeader();
                // Console.WriteLine("🔹 CSV 表头字段：");
                // foreach (var header in csv.HeaderRecord)
                // {
                //     Console.WriteLine(header);  // 打印 CSV 文件的表头，看看是否符合 C# 类的属性
                // }
                //csv.Configuration.RegisterClassMap<CsvRecordMap>();
                LogHelper.Info("===csv retrieving data===");
                records = csv.GetRecords<FuzhangguiRecord>().ToList();
                LogHelper.Info($"===csv data {records.Count}===");
                foreach (var record in records)
                {
                    LogHelper.Info($"===csv details {record.Ordernum}===");
                    string query = @"INSERT INTO fuzhanggui 
                                    (storename, storeid, itemname, paymethod, subitemname, property, barcode, itemgroup, finitemgroup, ordernum, 
                                    orderdate, ordertime, paydate, paytime, price, qty, unit, orgprice, disprice, actualmount, refund, discount, 
                                    channel, pickcode, tableref, realinventory, member, membername, memberlevel, membercardno, orderstaff, opentable, 
                                    itemmemo, ordermemo)
                                    VALUES 
                                    (@storename, @storeid, @itemname, @paymethod, @subitemname, @property, @barcode, @itemgroup, @finitemgroup, @ordernum, 
                                    @orderdate, @ordertime, @paydate, @paytime, @price, @qty, @unit, @orgprice, @disprice, @actualmount, @refund, @discount, 
                                    @channel, @pickcode, @tableref, @realinventory, @member, @membername, @memberlevel, @membercardno, @orderstaff, @opentable, 
                                    @itemmemo, @ordermemo)";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@storename", record.Storename);
                        cmd.Parameters.AddWithValue("@storeid", record.Storeid);
                        cmd.Parameters.AddWithValue("@itemname", record.Itemname);
                        cmd.Parameters.AddWithValue("@paymethod", record.Paymethod);
                        cmd.Parameters.AddWithValue("@subitemname", record.Subitemname);
                        cmd.Parameters.AddWithValue("@property", record.Property);
                        cmd.Parameters.AddWithValue("@barcode", record.Barcode);
                        cmd.Parameters.AddWithValue("@itemgroup", record.Itemgroup);
                        cmd.Parameters.AddWithValue("@finitemgroup", record.Finitemgroup);
                        cmd.Parameters.AddWithValue("@ordernum", record.Ordernum);
                        cmd.Parameters.AddWithValue("@orderdate", record.Orderdate);
                        cmd.Parameters.AddWithValue("@ordertime", record.Ordertime);
                        cmd.Parameters.AddWithValue("@paydate", record.Paydate);
                        cmd.Parameters.AddWithValue("@paytime", record.Paytime);
                        cmd.Parameters.AddWithValue("@price", record.Price);
                        cmd.Parameters.AddWithValue("@qty", record.Qty);
                        cmd.Parameters.AddWithValue("@unit", record.Unit);
                        cmd.Parameters.AddWithValue("@orgprice", record.Orgprice);
                        cmd.Parameters.AddWithValue("@disprice", record.Disprice);
                        cmd.Parameters.AddWithValue("@actualmount", record.Actualmount);
                        cmd.Parameters.AddWithValue("@refund", record.Refund);
                        cmd.Parameters.AddWithValue("@discount", record.Discount);
                        cmd.Parameters.AddWithValue("@channel", record.Channel);
                        cmd.Parameters.AddWithValue("@pickcode", record.Pickcode);
                        cmd.Parameters.AddWithValue("@tableref", record.Tableref);
                        cmd.Parameters.AddWithValue("@realinventory", record.Realinventory);
                        cmd.Parameters.AddWithValue("@member", record.Member);
                        cmd.Parameters.AddWithValue("@membername", record.Membername);
                        cmd.Parameters.AddWithValue("@memberlevel", record.Memberlevel);
                        cmd.Parameters.AddWithValue("@membercardno", record.Membercardno);
                        cmd.Parameters.AddWithValue("@orderstaff", record.Orderstaff);
                        cmd.Parameters.AddWithValue("@opentable", record.Opentable);
                        cmd.Parameters.AddWithValue("@itemmemo", record.Itemmemo);
                        cmd.Parameters.AddWithValue("@ordermemo", record.Ordermemo);
                        cmd.ExecuteNonQuery();
                    }
                
                }
            }

            connection.Close();
            LogHelper.Info("csv data uploaded!");
            Console.WriteLine("数据导入完成！");
        }
    }

public static void uploadfile(string argfilepath,string argMsqlconnectstr)
    {
        string connectionString = argMsqlconnectstr;
        string filePath = argfilepath; // 替换为您的文件路径
        string fileName = Path.GetFileName(filePath);

        byte[] fileData;

        try
        {
            // 读取文件内容为字节数组
            fileData = File.ReadAllBytes(filePath);
            LogHelper.Info("读取文件成功");
        }
        catch (Exception ex)
        {
            LogHelper.Error($"读取文件时出错: {ex.Message}");
            return;
        }

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();

                string sql = "INSERT INTO fzgfiles (filename, filedata) VALUES (@filename, @filedata)";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@filename", fileName);
                    cmd.Parameters.AddWithValue("@filedata", fileData);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    LogHelper.Info($"成功上传 {rowsAffected} 个文件。");
                }
            }
            catch (Exception ex)
            {
                 LogHelper.Info($"数据库操作出错: {ex.Message}");
                // Console.WriteLine($"数据库操作出错: {ex.Message}");
            }
        }
    }


}



public static class SqlEncryptor
{
    private static readonly string aesKey = "MySecretAESKey1234"; // 16 字节（AES-128）
    private static readonly string aesIV = "1234567890123456";     // 16 字节（固定 IV）
    private static byte[] GetValidKey(string key, int size)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        Array.Resize(ref keyBytes, size);
        return keyBytes;
    }
    private static byte[] GetSHA256Key(string key)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(key)).Take(32).ToArray(); // 生成 32 字节密钥
        }
    }

    // 🔹 AES 加密 SQL 语句
    public static string EncryptSql(string sql)
    {
        if (string.IsNullOrEmpty(sql)) throw new ArgumentNullException(nameof(sql));

        try
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = GetSHA256Key(aesKey);
                aesAlg.IV = Encoding.UTF8.GetBytes(aesIV);
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                using (MemoryStream msEncrypt = new MemoryStream())
                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor())
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(sql);
                        swEncrypt.Flush();  // ✅ 确保数据被写入
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray()); // ✅ `msEncrypt` 仍然可用
                }

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ encry failed: {ex.Message}");
            return null;
        }
    }

    public static string DecryptSql(string encryptedSql)
    {
        if (string.IsNullOrEmpty(encryptedSql)) throw new ArgumentNullException(nameof(encryptedSql));

        try
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = GetSHA256Key(aesKey);
                aesAlg.IV = Encoding.UTF8.GetBytes(aesIV);
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedSql)))
                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor())
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ failure decry: {ex.Message}");
            return null;
        }
    }

}



public class FuzhangguiRecord
{
    [Name("门店名称")]
    public string Storename { get; set; } // 已经是引用类型，可以持有null
    [Name("门店ID")]
    public long? Storeid { get; set; } // 添加了?以允许null
    [Name("商品名称")] public string Itemname { get; set; } // 已经是引用类型，可以持有null
    [Name("支付类型")] public string Paymethod { get; set; } // 已经是引用类型，可以持有null
    [Name("子规格商品名")] public string Subitemname { get; set; } // 已经是引用类型，可以持有null
    [Name("属性")] public string Property { get; set; } // 已经是引用类型，可以持有null
    [Name("商品条形码")] public string Barcode { get; set; } // 已经是引用类型，可以持有null

    [Name("商品一级分组")] public string Itemgroup { get; set; } // 已经是引用类型，可以持有null
    [Name("财务分组")] public string Finitemgroup { get; set; } // 已经是引用类型，可以持有null
    [Name("订单号")] public long? Ordernum { get; set; } // 添加了?以允许null
    [Name("下单日期")] public DateTime? Orderdate { get; set; } // 添加了?以允许null
    [Name("下单时间")] public TimeSpan? Ordertime { get; set; } // 添加了?以允许null
    [Name("结账日期")] public DateTime? Paydate { get; set; } // 添加了?以允许null
    [Name("结账时间")] public TimeSpan? Paytime { get; set; } // 添加了?以允许null
    [Name("单价")] public decimal? Price { get; set; } // 添加了?以允许null
    [Name("数量")] public decimal? Qty { get; set; } // 添加了?以允许null
    [Name("单位")] public string Unit { get; set; } // 已经是引用类型，可以持有null
    [Name("原单价格")] public decimal? Orgprice { get; set; } // 添加了?以允许null
    [Name("优惠金额")] public decimal? Disprice { get; set; } // 添加了?以允许null
    [Name("实收金额（含抹零金额）")] public decimal? Actualmount { get; set; } // 添加了?以允许null
    [Name("退款金额")] public decimal? Refund { get; set; } // 添加了?以允许null
    [Name("折扣率")] public decimal? Discount { get; set; } // 添加了?以允许null
    [Name("销售渠道")] public string Channel { get; set; } // 已经是引用类型，可以持有null
    [Name("取餐码")] public string Pickcode { get; set; } // 已经是引用类型，可以持有null
    [Name("桌台")] public string Tableref { get; set; } // 已经是引用类型，可以持有null
    [Name("实时库存")] public decimal? Realinventory { get; set; } // 添加了?以允许null
    [Name("是否会员消费")] public string Member { get; set; } // 添加了?以允许null
    [Name("会员名称")] public string Membername { get; set; } // 已经是引用类型，可以持有null
    [Name("会员等级")] public string Memberlevel { get; set; } // 已经是引用类型，可以持有null
    [Name("会员实体卡号")] public string Membercardno { get; set; } // 已经是引用类型，可以持有null
    [Name("下单人")] public string Orderstaff { get; set; } // 已经是引用类型，可以持有null
    [Name("开台人")] public string Opentable { get; set; } // 已经是引用类型，可以持有null

    [Name("商品备注")] public string Itemmemo { get; set; } // 已经是引用类型，可以持有null
    [Name("订单备注")] public string Ordermemo { get; set; } // 已经是引用类型，可以持有null
}