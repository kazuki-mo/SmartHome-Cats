using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.Web.Mvc;

using System.Diagnostics;

using MvcApplication1.Models;
using System.Web.Script.Serialization;

using System.Data;
using System.Data.Entity;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;

using System.Threading;
using System.Threading.Tasks;

using System.Security.Cryptography;
using System.Text;

namespace MvcApplication1.Controllers
{
  public class blobaddController : ApiController {

    private RDataBase RDB = new RDataBase();

    private CloudTable table; // Azure Table

    private Common common = new Common();

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

    #region TestEntity
    public class TestEntity : TableEntity {
      public TestEntity(string partitionkey, string rowkey) {
        this.PartitionKey = partitionkey;
        this.RowKey = rowkey;
      }

      public TestEntity() { }

      public List<string> DataVal { get; set; }
    }
    #endregion

    // Azure Blobにデータを追加
    // POST api/blobadd/{UserID名}/{Module名}
    public string Post(string fir, string sec, [FromBody]BlobAdd blobadd) {

      // RDBの中からターゲットモジュールなどの検索
      var user = RDB.db.Users.Where(p => p.idName.Equals(fir)).Single();
      var module = user.Modules.Where(p => p.Name.Equals(sec)).Single();
      var units = module.Units;

      // パスワードチェック(ハッシュ関数込み)
      if (!(module.wPassWord == null)) {
        string HashPW = common.GetHashPassword(blobadd.dataaddlist[0].dt, sec, module.wPassWord);
        try {
          if (!(blobadd.dataaddlist[0].pw.Equals(HashPW))) {
            return "PassWord error";
          }
        } catch {
          return "You need to password";
        }
      }

      try {

        // データ挿入中はTypeプロパティは"2"
        module.Type = "2";
        RDB.db.SaveChanges();

        // もし、データ種類の設定を事前に行っていなければ、初期設定をする
        if (units.Count == 0) { 
          Unit unit = new Unit();
          unit.Unit1 = "File Name";
          unit.TypeDataId = 12;
          unit.Modules.Add(module);
          RDB.db.SaveChanges();
          units = module.Units;
        }

        string time = common.GetTimeIndex(blobadd.dataaddlist[0].dt);

        // Table内には、モジュールIDとBlobデータ格納時刻とファイル名が入る
        #region Insert AzureTable

        table = common.AzureAccess();

        DataEntity customer1 = new DataEntity("Take," + module.id, time);
        customer1.DataVal = "BlobData";
        TableOperation insertOperationTake = TableOperation.Insert(customer1);
        table.Execute(insertOperationTake);

        DataEntity customer2 = new DataEntity("Value," + module.id, time + "," + units.FirstOrDefault().id);
        customer2.DataVal = blobadd.filename;
        TableOperation insertOperationValue = TableOperation.Insert(customer2);
        table.Execute(insertOperationValue);

        #endregion

        // Blob内には、送信されたデータ全てをSONテキストとして1行に格納される
        #region Insert AzureBlob

        JavaScriptSerializer serializer = new JavaScriptSerializer();
        string json = serializer.Serialize(blobadd);

        CloudBlobContainer container = common.BlobAccess();

        CloudBlockBlob blockBlob = container.GetBlockBlobReference(module.id.ToString() + "," + time);

        blockBlob.UploadText(json);
        blockBlob.Properties.ContentType = "application/JSON";
        blockBlob.SetProperties();

        #endregion

        module.Latest = blobadd.dataaddlist[0].dt;
        module.NumData += 1;

      } catch {
        module.Type = "0";
        RDB.db.SaveChanges();
      }
      module.Type = "0";
      RDB.db.SaveChanges();

      return "Success!";
    }

  }
}
