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

using System.IO;
using System.Web.Script.Serialization;

namespace MvcApplication1.Controllers
{
    public class blobdataController : ApiController
    {

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

        // Blobデータを取得
        // POST api/blobdata/{UserID名}/{Module名}
        public alldata Post(string fir, string sec, [FromBody]BlobQuery blobquery) {

          alldata AllData = new alldata();  // ここに、データの中身が入る

          // RDBの中から、ターゲットモジュールなどの検索
          try {
              var usertest = RDB.db.Users.Where(p => p.idName.Equals(fir)).Single();
          } catch {
              AllData.Type = new List<string>();
              AllData.Type.Add("User does not exist.");
              return AllData;
          }
          var user = RDB.db.Users.Where(p => p.idName.Equals(fir)).Single();
          try {
              var moduletest = user.Modules.Where(p => p.Name.Equals(sec)).Single();
          } catch {
              AllData.Type = new List<string>();
              AllData.Type.Add("Module does not exist.");
              return AllData;
          }
          var module = user.Modules.Where(p => p.Name.Equals(sec)).Single();
          var units = module.Units;

          // パスワードチェック
          if (common.PasswordError(blobquery.pw, module.rPassWord)) {
            AllData.Type = new List<string>();
            AllData.Type.Add("Password Error");
            return AllData;
          }

          string time = common.GetTimeIndex(blobquery.dt);

          // Blob内のデータを取得
          CloudBlobContainer container = common.BlobAccess();
          CloudBlockBlob blockBlob = container.GetBlockBlobReference(module.id.ToString() + "," + time);
          string jsontext;
          using (var memoryStream = new MemoryStream()) {
            blockBlob.DownloadToStream(memoryStream);
            jsontext = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
          }
          JavaScriptSerializer serializer = new JavaScriptSerializer();
          BlobAdd blobdata = serializer.Deserialize<BlobAdd>(jsontext);


          // unitlist配列には、ユーザが取得したい列インデックス番号が入ってある
          if (!(blobquery.unitlist == null)) {
            AllData.Type = new List<string>();
            foreach (var unitindex in blobquery.unitlist) {
              AllData.Type.Add(blobdata.type[unitindex-1]);
            }
          } else {
            AllData.Type = blobdata.type;
          }


          AllData.data = new List<List<string>>();
          List<List<string>> tmpAllData = new List<List<string>>();
          int num = 0;
          foreach (var data in blobdata.dataaddlist) {
            
            List<string> tmpdata = new List<string>();

            // ユーザが取得したいデータ個数やデータ範囲内ではないデータは無視する
            if ((blobquery.num != 0) && (num >= blobquery.num)) {
              break;
            }
            if ((!(blobquery.since == null)) && (long.Parse(common.GetTimeIndex(data.dt)) > long.Parse(common.GetTimeIndex(blobquery.since)))) {
              continue;
            }
            if ((!(blobquery.until == null)) && (long.Parse(common.GetTimeIndex(data.dt)) < long.Parse(common.GetTimeIndex(blobquery.until)))) {
              continue;
            }

            num++;

            // noDateがtrueなら更新時刻を取得しない
            if (!blobquery.noDate) {
              tmpdata.Add(data.dt);
            }

            // unitlistがnullなら、全列のデータを取得
            if (!(blobquery.unitlist == null)) {
              foreach (var unitindex in blobquery.unitlist) {
                  tmpdata.Add(data.dat[unitindex-1]);
              }
            } else {
                foreach(var dat in data.dat){
                    tmpdata.Add(dat);
                }
              //tmpdata = data.dat;
            }

            tmpAllData.Add(tmpdata);
            AllData.data.Add(tmpdata);
          }

          // "asc"なら、逆順にする
          if (blobquery.orderby.Equals("asc")) {
            int count = tmpAllData.Count();
            int i = 1;
            foreach (var tmpdata in tmpAllData) {
              AllData.data[count - i] = tmpdata;
              i++;
            }
          }


          return AllData;
        }


    }
}
