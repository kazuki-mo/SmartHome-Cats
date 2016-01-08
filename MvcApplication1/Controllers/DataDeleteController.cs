using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data;
using System.Data.Entity;
using MvcApplication1.Models;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;

using System.Diagnostics;

using System.Threading;
using System.Threading.Tasks;

namespace MvcApplication1.Controllers
{
    public class DataDeleteController : ApiController{

      private RDataBase RDB = new RDataBase();

      private CloudTable table; // Azure Table

      private Common common = new Common();

      private IEnumerable<DataEntity> Datas;

      // Azure Tableへ挿入するデータの設計図
    #region DataEntity
    public class DataEntity : TableEntity {
      public DataEntity(string partitionkey, string rowkey) {
        this.PartitionKey = partitionkey;
        this.RowKey = rowkey;
      }

      public DataEntity() { }

      public string DataVal { get; set; }
    }
    #endregion

        // Azure Table内のデータを削除
        // POST api/datadelete
        public string Post([FromBody]DataDelete query)
        {
          // このAPIは、管理アプリケーション内の「削除」をクリックした時しか使われない

          table = common.AzureAccess(); // Azure Tableへアクセス

          // RDBの中から、ターゲットモジュールなどを検索
          var loginuser = RDB.db.Users.Where(p => p.idName.Equals(User.Identity.Name)).Single();
          var module = loginuser.Modules.Where(p => p.Name.Equals(query.modulename)).Single();
          int id = module.id;

          CloudBlobContainer container = common.BlobAccess(); // Azure Blobへアクセス

          // 要求された日付範囲内のデータを取得(Take部)
          TableQuery<DataEntity> query1 = new TableQuery<DataEntity>().Where(
          TableQuery.CombineFilters(
          TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Take," + id),
          TableOperators.And,
          TableQuery.CombineFilters(
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, common.GetTimeIndex(query.datestart)),
          TableOperators.And,
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, common.GetTimeIndex(query.dateend))
          )
        ));

          List<TableBatchOperation> deleteOperationList = new List<TableBatchOperation>();
          TableBatchOperation deleteOperation = new TableBatchOperation();

          try {
            module.Type = "1"; //削除中は"1"
            RDB.db.SaveChanges();
          } catch {
          }

          int CountNum = 0;

          // 100件ずつまとめて削除(Take部)（Blobデータなら、1件ずつ削除）
          foreach (var entity in table.ExecuteQuery(query1)) {
            deleteOperation.Delete(entity);
            if (deleteOperation.Count == 100) {
              deleteOperationList.Add(deleteOperation);
              deleteOperation = new TableBatchOperation();
            }
            if (!(entity.DataVal == null)) {
              if (entity.DataVal.Equals("BlobData")) {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(id.ToString() + "," + entity.RowKey);
                blockBlob.Delete();
              }
            }
            CountNum++;
          }
          if (deleteOperation.Count > 0) {
            deleteOperationList.Add(deleteOperation);
            deleteOperation = new TableBatchOperation();
          }
          Parallel.ForEach(deleteOperationList, Operation => {
            table.ExecuteBatch(Operation);
          });

          // 削除後のデータ件数を取得してRDBのNumDataを変更
          TableQuery<DataEntity> Countquery = new TableQuery<DataEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Take," + module.id));
          try {
            module.NumData = table.ExecuteQuery(Countquery).Count();
            RDB.db.SaveChanges();
          } catch {
          }


          // 要求された日付範囲内のデータを取得(Value部)
          TableQuery<DataEntity> query2 = new TableQuery<DataEntity>().Where(
          TableQuery.CombineFilters(
          TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Value," + id),
          TableOperators.And,
          TableQuery.CombineFilters(
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, common.GetTimeIndex(query.datestart)),
          TableOperators.And,
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, common.GetTimeIndex(query.dateend)+1)
          )
        ));

          deleteOperationList.Clear();

          // 100件ずつまとめて削除(Value部)
          foreach (var entity in table.ExecuteQuery(query2)) {
            deleteOperation.Delete(entity);
            if (deleteOperation.Count == 100) {
              deleteOperationList.Add(deleteOperation);
              deleteOperation = new TableBatchOperation();
            }
          }
          if (deleteOperation.Count > 0) {
            deleteOperationList.Add(deleteOperation);
            deleteOperation = new TableBatchOperation();
          }
          Parallel.ForEach(deleteOperationList, Operation => {
            table.ExecuteBatch(Operation);
          });

          try {
            module.Type = "0";
            RDB.db.SaveChanges();
          } catch {
          }

          return "Success!!";
          
        }

    }
}
