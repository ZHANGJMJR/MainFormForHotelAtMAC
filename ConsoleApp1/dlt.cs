// dlt.cs (修正后的代码)
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using Dapper;
using log4net;
using log4net.Config;
using Oracle.ManagedDataAccess.Client;
using MySql.Data.MySqlClient;
using System.Reflection;
using System.IO;

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
    private static readonly ILog log = LogManager.GetLogger("DltLogger");
    private static readonly string oracleConnStr = "User Id=sys;Password=Orcl$1mph0ny;Data Source=172.16.139.12:1521/mcrspos;DBA Privilege=SYSDBA;";
    protected static readonly string mysqlConnStr = "Server=127.0.0.1;Port=3306;Database=hotel;User=root;Password=root;";
    public string getMysqlConnectStr => mysqlConnStr;
    static Dlt()
    {
        XmlConfigurator.Configure(new FileInfo("log.config"));
    }

    public void DeleteDataByDate(string dateString, string connectionString)
    {
        DateTime date;
        if (!DateTime.TryParse(dateString, out date))
        {
            log.Info("=== Invalid date format clean datum ===");
            throw new ArgumentException("Invalid date format");
        }

        string[] tables = { "guestcheckdetailssumrow", "guestcheckdetails", "guestcheck" };
        log.Info($"=== 开始清理 {date:yyyy-MM-dd} 的数据 ===");

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
                        log.Info($"表 `{table}` 清理完成，影响行数：{affectedRows}");
                    }
                    transaction.Commit();
                    log.Info("=== 清理数据成功 ===");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    log.Error("=== 清理数据失败 ===", ex);
                    throw;
                }
            }
        }
        log.Info("=== 结束清理数据 ===");
    }



    public void SyncData()
    {
        string currentDateStr = "2024-07-01";
        DeleteDataByDate(currentDateStr, getMysqlConnectStr);

        log.Info("=== 开始同步数据 ===");

        using (var oracleConn = new OracleConnection(oracleConnStr))
        using (var mysqlConn = new MySqlConnection(mysqlConnStr))
        {
            try
            {
                oracleConn.Open();
                log.Info("Oracle 数据库连接成功");

                mysqlConn.Open();
                log.Info("MySQL 数据库连接成功");


                string masterQuery = @"SELECT gch.guestCheckID, gch.openBusinessDate as busDate, gch.locationID, gch.revenuecenterid,
                       gch.checknum AS checkNum, gch.opendatetime AS openDateTime, gch.checktotal AS checkTotal,
                       gch.numitems AS numItems, e.firstname AS firstName, e.lastname AS lastName 
                FROM guest_check_hist gch  
                LEFT JOIN employee e ON gch.employeeid = e.employeeid 
                WHERE gch.organizationID = 10260 AND gch.locationID = 2041 
                  AND gch.openbusinessdate >= TO_DATE(:currentDateStr, 'YYYY-MM-DD')
                  AND gch.closebusinessdate <= TO_DATE(:currentDateStr, 'YYYY-MM-DD')";

                var masterRecords = oracleConn.Query<GuestCheckHist>(masterQuery, new { currentDateStr }).ToList();
                if (!masterRecords.Any())
                {
                    log.Warn("主表无数据，不执行从表查询");
                    return;
                }

                log.Info($"从 Oracle 获取 {masterRecords.Count} 条主表记录");
                string masterInsertQuery = @"INSERT INTO guestcheck (guestCheckID, busDate, locationid, revenuecenterid, checkNum, openDateTime, checkTotal, numItems, firstName, lastName) 
                VALUES (@guestCheckID, @busDate, @locationid, @revenuecenterid, @checkNum, @openDateTime, @checkTotal, @numItems, @firstName, @lastName);";
                using (var transaction = new TransactionScope())
                {
                    mysqlConn.Execute(masterInsertQuery, masterRecords);
                    transaction.Complete();
                }

                log.Info("主表数据同步完成");
                string detailQuery = @"SELECT * FROM (SELECT GUEST_CHECK_LINE_ITEM_HIST.transDatetime AS transTime,
GUEST_CHECK_LINE_ITEM_HIST.serviceRoundNum,
(GUEST_CHECK_LINE_ITEM_HIST.lineNum*10) AS lineNum,
GUEST_CHECK_LINE_ITEM_HIST.guestCheckLineItemID,
GUEST_CHECK_LINE_ITEM_HIST.detailType,
MENU_ITEM.menuItemName1Master AS itemName,
STTEXT.stringtext AS itemchname,
rcs.name AS rvcName,
EMPLOYEE.firstName,
EMPLOYEE.lastName,
CASE WHEN GUEST_CHECK_LINE_ITEM_HIST.voidFlag = 1 THEN 'Void'
ELSE 'blank' END AS reasonVoidText,
CASE WHEN GUEST_CHECK_LINE_ITEM_HIST.genFlag1 = 1 THEN 'Return'
ELSE 'blank' END AS returnText, GUEST_CHECK_LINE_ITEM_HIST.recordID,
GUEST_CHECK_LINE_ITEM_HIST.lineTotal AS salesTotal,
GUEST_CHECK_LINE_ITEM_HIST.lineCount AS salesCount,
CASE WHEN GUEST_CHECK_LINE_ITEM_HIST.denominator > 0 
THEN CONCAT('/ ', GUEST_CHECK_LINE_ITEM_HIST.denominator)  ELSE ''
END AS salesCountDivisor,
GUEST_CHECK_LINE_ITEM_HIST.guestCheckID
FROM GUEST_CHECK_LINE_ITEM_HIST left join REVENUE_CENTER
on REVENUE_CENTER.revenueCenterID = GUEST_CHECK_LINE_ITEM_HIST.revenueCenterID
left join Revenue_Center_String RCS on REVENUE_CENTER.Revenuecenterid=RCS.Revenuecenterid  
and rcs.poslanguageid=3 left join EMPLOYEE on  
EMPLOYEE.employeeID = NVL(GUEST_CHECK_LINE_ITEM_HIST.managerEmployeeID,
GUEST_CHECK_LINE_ITEM_HIST.transEmployeeID) left join mENU_ITEM 
on mENU_ITEM.menuItemID = GUEST_CHECK_LINE_ITEM_HIST.recordID 
left join MCRSPOSDB.menu_item_master MIM on MIM.objectnumber = mENU_ITEM.menuitemposref 
left join MCRSPOSDB.string_table STTEXT on MIM.nameid = STTEXT.stringnumberid and
(STTEXT.langid = 2) where (GUEST_CHECK_LINE_ITEM_HIST.locationID =2041) AND 
(GUEST_CHECK_LINE_ITEM_HIST.detailType = 1) AND 
(GUEST_CHECK_LINE_ITEM_HIST.doNotShow IS NULL OR GUEST_CHECK_LINE_ITEM_HIST.doNotShow = 0) AND 
(GUEST_CHECK_LINE_ITEM_HIST.guestCheckID =:guestCheckID) 
and mENU_ITEM.menuitemposref not in (19999997,19999998,19999999)
UNION
SELECT GUEST_CHECK_LINE_ITEM_HIST.transDatetime AS transTime,
       GUEST_CHECK_LINE_ITEM_HIST.serviceRoundNum,
       ((GUEST_CHECK_LINE_ITEM_HIST.lineNum*10)+1) AS lineNum,
       0 AS guestCheckLineItemID,
       GUEST_CHECK_LINE_ITEM_HIST.detailType,
       REASON_CODE.name AS itemName,
       chr('') AS itemchname,
       REVENUE_CENTER.nameMaster AS rvcName,
       EMPLOYEE.firstName,
       EMPLOYEE.lastName,
       'Reason' AS reasonVoidText,
       'blank' AS returnText,
       GUEST_CHECK_LINE_ITEM_HIST.recordID,
       0 AS salesTotal,
       0 AS salesCount,
       '' AS salesCountDivisor,
       GUEST_CHECK_LINE_ITEM_HIST.guestCheckID
FROM     REASON_CODE  RIGHT OUTER JOIN
         EMPLOYEE RIGHT OUTER JOIN
         REVENUE_CENTER  RIGHT OUTER JOIN
         GUEST_CHECK_LINE_ITEM_HIST
         ON REVENUE_CENTER.revenueCenterID = GUEST_CHECK_LINE_ITEM_HIST.revenueCenterID
         ON EMPLOYEE.employeeID = NVL(GUEST_CHECK_LINE_ITEM_HIST.managerEmployeeID,GUEST_CHECK_LINE_ITEM_HIST.transEmployeeID)
         ON REASON_CODE.reasonCodeID = GUEST_CHECK_LINE_ITEM_HIST.reasonCodeID
WHERE (GUEST_CHECK_LINE_ITEM_HIST.organizationID =10260) AND
    (GUEST_CHECK_LINE_ITEM_HIST.locationID =2041) AND
    (GUEST_CHECK_LINE_ITEM_HIST.detailType = 1) AND
    (GUEST_CHECK_LINE_ITEM_HIST.voidFlag = 1) AND
    (GUEST_CHECK_LINE_ITEM_HIST.doNotShow IS NULL OR GUEST_CHECK_LINE_ITEM_HIST.doNotShow = 0) AND
    (GUEST_CHECK_LINE_ITEM_HIST.guestCheckID =:guestCheckID)
UNION
SELECT GUEST_CHECK_LINE_ITEM_HIST.transDatetime AS transTime,
       GUEST_CHECK_LINE_ITEM_HIST.serviceRoundNum,
       ((GUEST_CHECK_LINE_ITEM_HIST.lineNum*10)+1) AS lineNum,
       0 AS guestCheckLineItemID,
       GUEST_CHECK_LINE_ITEM_HIST.detailType,
       REASON_CODE.name AS itemName,
       chr('') AS itemchname,
       REVENUE_CENTER.nameMaster AS rvcName,
       EMPLOYEE.firstName,
       EMPLOYEE.lastName,
       'Reason' AS reasonVoidText,
       'blank' AS returnText,
       GUEST_CHECK_LINE_ITEM_HIST.recordID,
       0 AS salesTotal,
       0 AS salesCount,
       '' AS salesCountDivisor,
       GUEST_CHECK_LINE_ITEM_HIST.guestCheckID
FROM     REASON_CODE  RIGHT OUTER JOIN
         EMPLOYEE RIGHT OUTER JOIN
         REVENUE_CENTER  RIGHT OUTER JOIN
         GUEST_CHECK_LINE_ITEM_HIST
         ON REVENUE_CENTER.revenueCenterID = GUEST_CHECK_LINE_ITEM_HIST.revenueCenterID
         ON EMPLOYEE.employeeID = NVL(GUEST_CHECK_LINE_ITEM_HIST.managerEmployeeID,GUEST_CHECK_LINE_ITEM_HIST.transEmployeeID)
         ON REASON_CODE.reasonCodeID = GUEST_CHECK_LINE_ITEM_HIST.reasonCodeID
WHERE (GUEST_CHECK_LINE_ITEM_HIST.organizationID =10260) AND
    (GUEST_CHECK_LINE_ITEM_HIST.locationID =2041) AND
    (GUEST_CHECK_LINE_ITEM_HIST.detailType = 1) AND
    (GUEST_CHECK_LINE_ITEM_HIST.genFlag1 = 1) AND
    (GUEST_CHECK_LINE_ITEM_HIST.doNotShow IS NULL OR GUEST_CHECK_LINE_ITEM_HIST.doNotShow = 0) AND
    (GUEST_CHECK_LINE_ITEM_HIST.guestCheckID =:guestCheckID)
UNION
SELECT GUEST_CHECK_LINE_ITEM_HIST.transDatetime AS transTime,
       GUEST_CHECK_LINE_ITEM_HIST.serviceRoundNum,
       (GUEST_CHECK_LINE_ITEM_HIST.lineNum*10) AS lineNum,
       GUEST_CHECK_LINE_ITEM_HIST.guestCheckLineItemID,
       GUEST_CHECK_LINE_ITEM_HIST.detailType,
       DISCOUNT.nameMaster AS itemName,
       chr('') AS itemchname,
       REVENUE_CENTER.nameMaster AS rvcName,
       EMPLOYEE.firstName,
       EMPLOYEE.lastName,
       CASE
           WHEN GUEST_CHECK_LINE_ITEM_HIST.voidFlag = 1 THEN 'Void'
           ELSE 'blank' END AS reasonVoidText,
       'blank' AS returnText,
       GUEST_CHECK_LINE_ITEM_HIST.recordID,
       GUEST_CHECK_LINE_ITEM_HIST.lineTotal AS salesTotal,
       0 AS salesCount,
       '' AS salesCountDivisor,
       GUEST_CHECK_LINE_ITEM_HIST.guestCheckID
FROM     DISCOUNT  RIGHT OUTER JOIN
         EMPLOYEE RIGHT OUTER JOIN
         REVENUE_CENTER  RIGHT OUTER JOIN GUEST_CHECK_LINE_ITEM_HIST
ON REVENUE_CENTER.revenueCenterID = GUEST_CHECK_LINE_ITEM_HIST.revenueCenterID
ON EMPLOYEE.employeeID = NVL(GUEST_CHECK_LINE_ITEM_HIST.managerEmployeeID,GUEST_CHECK_LINE_ITEM_HIST.transEmployeeID)
ON DISCOUNT.discountID = GUEST_CHECK_LINE_ITEM_HIST.recordID
WHERE (GUEST_CHECK_LINE_ITEM_HIST.organizationID =10260) AND
    (GUEST_CHECK_LINE_ITEM_HIST.locationID =2041) AND
    (GUEST_CHECK_LINE_ITEM_HIST.detailType = 2) AND
    (GUEST_CHECK_LINE_ITEM_HIST.doNotShow IS NULL OR GUEST_CHECK_LINE_ITEM_HIST.doNotShow = 0) AND
    (GUEST_CHECK_LINE_ITEM_HIST.guestCheckID =:guestCheckID)
UNION
SELECT GUEST_CHECK_HIST.openDateTime  AS transDatetime,
       100 AS serviceRoundNum,
       NULL AS lineNum,
       NULL AS guestCheckLineItemID,
       NULL AS detailType,
       SERVICE_CHARGE.nameMaster AS itemName,
       chr('') AS itemchname,
       REVENUE_CENTER.nameMaster AS rvcName,
       EMPLOYEE.firstName,
       EMPLOYEE.lastName,
       'blank' AS reasonVoidText,
       'blank' AS returnText,
       NULL AS recordID,
       GUEST_CHECK_HIST.autoServiceChargeTotal AS salesTotal,
       0 AS salesCount,
       '' AS salesCountDivisor,
       GUEST_CHECK_HIST.guestCheckID
FROM     EMPLOYEE RIGHT OUTER JOIN
         SERVICE_CHARGE   RIGHT OUTER JOIN
         REVENUE_CENTER  RIGHT OUTER JOIN
         GUEST_CHECK_HIST
         ON GUEST_CHECK_HIST.revenueCenterID = REVENUE_CENTER.revenueCenterID
         ON  SERVICE_CHARGE.serviceChargePosRef = REVENUE_CENTER.autoServiceChargePosref  AND
             SERVICE_CHARGE.locationID = REVENUE_CENTER.locationID
         ON GUEST_CHECK_HIST.employeeID = EMPLOYEE.employeeID
WHERE GUEST_CHECK_HIST.organizationID =10260 AND
    GUEST_CHECK_HIST.locationID =2041 AND
    GUEST_CHECK_HIST.autoServiceChargeTotal > 0 AND
    GUEST_CHECK_HIST.guestCheckID =:guestCheckID
UNION
SELECT GUEST_CHECK_LINE_ITEM_HIST.transDatetime AS transTime,
       GUEST_CHECK_LINE_ITEM_HIST.serviceRoundNum,
       (GUEST_CHECK_LINE_ITEM_HIST.lineNum*10) AS lineNum,
       GUEST_CHECK_LINE_ITEM_HIST.guestCheckLineItemID,
       GUEST_CHECK_LINE_ITEM_HIST.detailType,
       TENDER_MEDIA.nameMaster AS itemName,
       TO_NCHAR(TMS.name) AS itemchname,
       RCS.name  AS rvcName,
       EMPLOYEE.firstName,
       EMPLOYEE.lastName,
       CASE
           WHEN GUEST_CHECK_LINE_ITEM_HIST.voidFlag = 1 THEN 'Void'
           ELSE 'blank' END AS reasonVoidText,
       'blank' AS returnText,
       GUEST_CHECK_LINE_ITEM_HIST.recordID,
       GUEST_CHECK_LINE_ITEM_HIST.lineTotal AS salesTotal,
       0 AS salesCount,
       '' AS salesCountDivisor,
       GUEST_CHECK_LINE_ITEM_HIST.guestCheckID
FROM     TENDER_MEDIA  RIGHT OUTER JOIN
         EMPLOYEE RIGHT OUTER JOIN
         REVENUE_CENTER  RIGHT OUTER JOIN GUEST_CHECK_LINE_ITEM_HIST
ON REVENUE_CENTER.revenueCenterID = GUEST_CHECK_LINE_ITEM_HIST.revenueCenterID
left join Revenue_Center_String RCS on REVENUE_CENTER.Revenuecenterid=RCS.Revenuecenterid  and rcs.poslanguageid=3
ON EMPLOYEE.employeeID = NVL(GUEST_CHECK_LINE_ITEM_HIST.managerEmployeeID,GUEST_CHECK_LINE_ITEM_HIST.transEmployeeID)
ON TENDER_MEDIA.tenderMediaID = GUEST_CHECK_LINE_ITEM_HIST.recordID
RIGHT JOIN  TENDER_MEDIA_string TMS on TMS.TENDERMEDIAID=TENDER_MEDIA.tenderMediaID and TMS.poslanguageid=3
WHERE (GUEST_CHECK_LINE_ITEM_HIST.organizationID =10260) AND
    (GUEST_CHECK_LINE_ITEM_HIST.locationID =2041) AND
    (GUEST_CHECK_LINE_ITEM_HIST.detailType = 4) AND
    (GUEST_CHECK_LINE_ITEM_HIST.doNotShow IS NULL OR GUEST_CHECK_LINE_ITEM_HIST.doNotShow = 0) AND
    (TENDER_MEDIA.typeMaster = 1) AND
    (GUEST_CHECK_LINE_ITEM_HIST.guestCheckID =:guestCheckID )
UNION
SELECT GUEST_CHECK_LINE_ITEM_HIST.transDatetime AS transTime,
       GUEST_CHECK_LINE_ITEM_HIST.serviceRoundNum,
       (GUEST_CHECK_LINE_ITEM_HIST.lineNum*10) AS lineNum,
       GUEST_CHECK_LINE_ITEM_HIST.guestCheckLineItemID,
       GUEST_CHECK_LINE_ITEM_HIST.detailType,
       GUEST_CHECK_LINE_ITEM_HIST.referenceInfo AS itemName,
       TO_NCHAR(GUEST_CHECK_LINE_ITEM_HIST.referenceInfo) AS itemchname,
       RCS.name  AS rvcName,
       EMPLOYEE.firstName,
       EMPLOYEE.lastName,
       CASE
           WHEN GUEST_CHECK_LINE_ITEM_HIST.voidFlag = 1 THEN 'Void'
           ELSE 'blank' END AS reasonVoidText,
       'blank' AS returnText,
       GUEST_CHECK_LINE_ITEM_HIST.recordID,
       0 AS salesTotal,
       0 AS salesCount,
       '' AS salesCountDivisor,
       GUEST_CHECK_LINE_ITEM_HIST.guestCheckID
FROM     EMPLOYEE RIGHT OUTER JOIN
         REVENUE_CENTER RIGHT OUTER JOIN GUEST_CHECK_LINE_ITEM_HIST
ON REVENUE_CENTER.revenueCenterID = GUEST_CHECK_LINE_ITEM_HIST.revenueCenterID
left join Revenue_Center_String RCS on REVENUE_CENTER.Revenuecenterid=RCS.Revenuecenterid  and rcs.poslanguageid=3
ON EMPLOYEE.employeeID = NVL(GUEST_CHECK_LINE_ITEM_HIST.managerEmployeeID,GUEST_CHECK_LINE_ITEM_HIST.transEmployeeID)
WHERE (GUEST_CHECK_LINE_ITEM_HIST.organizationID =10260) AND
    (GUEST_CHECK_LINE_ITEM_HIST.locationID =2041) AND
    (GUEST_CHECK_LINE_ITEM_HIST.detailType = 5) AND
    (GUEST_CHECK_LINE_ITEM_HIST.doNotShow IS NULL OR GUEST_CHECK_LINE_ITEM_HIST.doNotShow = 0) AND
    (GUEST_CHECK_LINE_ITEM_HIST.guestCheckID =:guestCheckID )
UNION
SELECT GUEST_CHECK_LINE_ITEM_HIST.transDatetime AS transTime,
       GUEST_CHECK_LINE_ITEM_HIST.serviceRoundNum,
       (GUEST_CHECK_LINE_ITEM_HIST.lineNum*10) AS lineNum,
       GUEST_CHECK_LINE_ITEM_HIST.guestCheckLineItemID,
       GUEST_CHECK_LINE_ITEM_HIST.detailType,
       CASE
           WHEN TENDER_MEDIA.hideAcctNum = 1
               THEN  CONCAT('xxxx-', SUBSTR(TRIM(GUEST_CHECK_LINE_ITEM_HIST.referenceInfo), -4, 4))
           ELSE referenceInfo
           END itemName,
       CASE
           WHEN TENDER_MEDIA.hideAcctNum = 1
               THEN TO_NCHAR( CONCAT('xxxx-', SUBSTR(TRIM(GUEST_CHECK_LINE_ITEM_HIST.referenceInfo), -4, 4)))
           ELSE TO_NCHAR(referenceInfo)
           END AS itemchname,
       rcs.name AS rvcName,
       EMPLOYEE.firstName,
       EMPLOYEE.lastName,
       CASE
           WHEN GUEST_CHECK_LINE_ITEM_HIST.voidFlag = 1 THEN 'Void'
           ELSE 'blank' END AS reasonVoidText,
       'blank' AS returnText,
       GUEST_CHECK_LINE_ITEM_HIST.recordID,
       0 AS salesTotal,
       0 AS salesCount,
       '' AS salesCountDivisor,
       GUEST_CHECK_LINE_ITEM_HIST.guestCheckID
FROM     TENDER_MEDIA  RIGHT OUTER JOIN
         EMPLOYEE RIGHT OUTER JOIN
         REVENUE_CENTER  RIGHT OUTER JOIN GUEST_CHECK_LINE_ITEM_HIST
ON REVENUE_CENTER.revenueCenterID = GUEST_CHECK_LINE_ITEM_HIST.revenueCenterID
left join Revenue_Center_String RCS on REVENUE_CENTER.Revenuecenterid=RCS.Revenuecenterid  and rcs.poslanguageid=3
ON EMPLOYEE.employeeID = NVL(GUEST_CHECK_LINE_ITEM_HIST.managerEmployeeID,GUEST_CHECK_LINE_ITEM_HIST.transEmployeeID)
ON TENDER_MEDIA.tenderMediaID = GUEST_CHECK_LINE_ITEM_HIST.recordID
WHERE (GUEST_CHECK_LINE_ITEM_HIST.organizationID =10260) AND
    (GUEST_CHECK_LINE_ITEM_HIST.locationID =2041) AND
    (GUEST_CHECK_LINE_ITEM_HIST.detailType = 6) AND
    (GUEST_CHECK_LINE_ITEM_HIST.doNotShow IS NULL OR GUEST_CHECK_LINE_ITEM_HIST.doNotShow = 0) AND
    (GUEST_CHECK_LINE_ITEM_HIST.guestCheckID =:guestCheckID )
union
SELECT GUEST_CHECK_LINE_ITEM_HIST.transDatetime AS transTime,
       GUEST_CHECK_LINE_ITEM_HIST.serviceRoundNum,
       (GUEST_CHECK_LINE_ITEM_HIST.lineNum*10) AS lineNum,
       GUEST_CHECK_LINE_ITEM_HIST.guestCheckLineItemID,
       GUEST_CHECK_LINE_ITEM_HIST.detailType,
       GUEST_CHECK_LINE_ITEM_HIST.referenceInfo AS itemName,
       TO_NCHAR(GUEST_CHECK_LINE_ITEM_HIST.referenceInfo) AS itemchname,
       REVENUE_CENTER.nameMaster AS rvcName,
       EMPLOYEE.firstName,
       EMPLOYEE.lastName,
       CASE
           WHEN GUEST_CHECK_LINE_ITEM_HIST.voidFlag = 1 THEN 'Void'
           ELSE 'blank' END AS reasonVoidText,
       'blank' AS returnText,
       GUEST_CHECK_LINE_ITEM_HIST.recordID AS RECORDID,
       GUEST_CHECK_LINE_ITEM_HIST.lineTotal AS salesTotal,
       0 AS salesCount,
       '' AS salesCountDivisor,
       GUEST_CHECK_LINE_ITEM_HIST.guestCheckID
FROM     EMPLOYEE RIGHT OUTER JOIN
         REVENUE_CENTER  RIGHT OUTER JOIN GUEST_CHECK_LINE_ITEM_HIST
ON REVENUE_CENTER.revenueCenterID = GUEST_CHECK_LINE_ITEM_HIST.revenueCenterID
ON EMPLOYEE.employeeID = NVL(GUEST_CHECK_LINE_ITEM_HIST.managerEmployeeID,GUEST_CHECK_LINE_ITEM_HIST.transEmployeeID)
WHERE (GUEST_CHECK_LINE_ITEM_HIST.organizationID =10260) AND
    (GUEST_CHECK_LINE_ITEM_HIST.locationID =2041) AND
    (GUEST_CHECK_LINE_ITEM_HIST.detailType = 7) AND
    (GUEST_CHECK_LINE_ITEM_HIST.doNotShow IS NULL OR GUEST_CHECK_LINE_ITEM_HIST.doNotShow = 0) AND
    (GUEST_CHECK_LINE_ITEM_HIST.guestCheckID =:guestCheckID ))
ORDER BY guestCheckID ASC,serviceRoundNum ASC, lineNum ASC, guestCheckLineItemID ASC
";
                string detailInsertQuery = @"INSERT INTO guestcheckdetails 
  (transTime, serviceRoundNum, lineNum, guestCheckLineItemID, detailType,itemName,itemName2,
   itemchname, rvcName,firstName,lastName,reasonVoidText,returnText,recordID,salesTotal,
    salesCount,salesCountDivisor,guestCheckID,checkNum,openDatetime) 
VALUES 
  (@transTime, @serviceRoundNum, @lineNum, @guestCheckLineItemID, @detailType,@itemName,@itemName2,
   @itemchname, @rvcName,@firstName,@lastName,@reasonVoidText,@returnText,@recordID,@salesTotal,
    @salesCount,@salesCountDivisor,@guestCheckID,@checkNum,@transTime);";
                string detailQuerySumRow = @" SELECT GUEST_CHECK_HIST.organizationID,
       GUEST_CHECK_HIST.checkNum,
       GUEST_CHECK_HIST.tableRef,
       GUEST_CHECK_HIST.openDatetime,
       round((NVL(GUEST_CHECK_HIST.closeDatetime, GUEST_CHECK_HIST.openDatetime) - GUEST_CHECK_HIST.openDatetime)*1440,2) AS duration,
       GUEST_CHECK_HIST.numGuests,
       GUEST_CHECK_HIST.checkRef,
       LOCATION_HIERARCHY_ITEM.name AS locName,
       RCS.name AS rvcName,
       OTMS.name AS otName,
       EMPLOYEE.firstName,
       EMPLOYEE.lastName
FROM     EMPLOYEE RIGHT OUTER JOIN
         ORDER_TYPE RIGHT OUTER JOIN
         REVENUE_CENTER RIGHT OUTER JOIN
         GUEST_CHECK_HIST LEFT OUTER JOIN LOCATION_HIERARCHY_ITEM
ON GUEST_CHECK_HIST.locationID = LOCATION_HIERARCHY_ITEM.locationID
ON GUEST_CHECK_HIST.revenueCenterID = REVENUE_CENTER.revenueCenterID
ON GUEST_CHECK_HIST.orderTypeID = ORDER_TYPE.orderTypeID
ON GUEST_CHECK_HIST.employeeID = EMPLOYEE.employeeID
LEFT JOIN   Revenue_Center_String RCS on RCS.Revenuecenterid = REVENUE_CENTER.Revenuecenterid 	and rcs.poslanguageid=3 
LEFT JOIN   ORDER_TYPE_STRING OTMS on OTMS.ordertypeid = ORDER_TYPE.ordertypeid 	and OTMS.poslanguageid=3 
WHERE (GUEST_CHECK_HIST.organizationID = 10260) AND
    (GUEST_CHECK_HIST.locationID =2041) AND
    (GUEST_CHECK_HIST.guestCheckID = :guestCheckID )";
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
                        log.Warn($"G_C_ID = {guestCheckID}-{masterRecord.checkNum} 没有对应的从表数据");
                    }
                    else
                    {
                        log.Info($"G_C_ID = {guestCheckID}-{masterRecord.checkNum}从表数据共 {detailRecords.Count} 条");
                        using (var transaction = new TransactionScope())
                        {
                            mysqlConn.Execute(detailInsertQuery, detailRecords);
                            transaction.Complete();
                        }
                    }
                    if (!detailSumRowRecords.Any())
                    {
                        log.Warn($"G_C_ID = {guestCheckID}-{masterRecord.checkNum} 没有对应的从表SUM数据");
                    }
                    else
                    {
                        log.Info($"G_C_ID = {guestCheckID}-{masterRecord.checkNum}从表SUM数据共 {detailSumRowRecords.Count} 条");
                        using (var transaction = new TransactionScope())
                        {
                            mysqlConn.Execute(detailInsertQuerySumRow, detailSumRowRecords);
                            transaction.Complete();
                        }
                    }
                }
                log.Info("从表数据同步完成");
            }
            catch (Exception ex)
            {
                log.Error("数据同步失败: " + ex.Message, ex);
            }
            finally
            {
                if (oracleConn.State == ConnectionState.Open) { oracleConn.Close(); log.Info("Oracle 数据库连接关闭"); }
                if (mysqlConn.State == ConnectionState.Open) { mysqlConn.Close(); log.Info("MySQL 数据库连接关闭"); }
            }
        }
        log.Info("=== 数据同步完成 ===1");
    }
}