// テスト用Controller


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
using System.Configuration;


namespace MvcApplication1.Controllers
{
    public class DataAddTestController : ApiController
    {

      private RDataBase RDB = new RDataBase();

      private CloudTable table;
      private CloudTableClient tableClient;

      #region TakeEntity
      public class TakeEntity : TableEntity {
        public TakeEntity(string partition, string row) {
          this.PartitionKey = partition;
          this.RowKey = row;
        }
        public TakeEntity() { }
      }
      #endregion

      #region ValueEntity
      public class ValueEntity : TableEntity {
        public ValueEntity(string partition, string row) {
          this.PartitionKey = partition;
          this.RowKey = row;
        }
        public ValueEntity() { }

        public string Value { get; set; }
      }
      #endregion

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

        // GET api/dataaddtest
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/dataaddtest/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/dataaddtest
        public void Post([FromBody]string value)
        {
        }

        // POST api/dataaddtest/{UserID名}/{Module名}
        public string Post(string fir, string sec, [FromBody]string value) {

          //JavaScriptSerializer serializer = new JavaScriptSerializer();
          //List<DataAdd> dataaddList = serializer.Deserialize<List<DataAdd>>(value);

          //var user = db.Users.Where(p => p.idName.Equals(fir)).Single();
          //var module = user.Modules.Where(p => p.Name.Equals(sec)).Single();
          //var units = module.Units;

          //String ConnectionString = "UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://127.0.0.1:10002";
          //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);
          //tableClient = storageAccount.CreateCloudTableClient();
          //String TableName = "Sample15";
          //table = tableClient.GetTableReference(TableName);

          //string date = String.Empty;
          //int num1 = 0;
          //int addnum = 0;

          //foreach (DataAdd dataadd in dataaddList) {

          //  //Debug.WriteLine("Count: " + dataadd.dat.Count+","+units.Count);

          //  if (dataadd.dat.Count > units.Count) {

          //    int count = dataadd.dat.Count - units.Count;

          //    for (int i = 0; i < count; i++) {

          //      Unit unit = new Unit();
          //      unit.Unit1 = "";
          //      unit.TypeDataId = 10;
          //      unit.Modules.Add(module);

          //      //db.SaveChanges();
          //      addnum++;
          //    }
          //    db.SaveChanges();
          //    units = module.Units;
          //  }

          //  //if (dataadd.dat.Count != units.Count) {
          //  //  return "データの個数が一致しません。";
          //  //}
          //  //Debug.WriteLine("Num: " + addnum);

          //  string time = GetTimeIndex(dataadd.dt);

          //  DataEntity customer1 = new DataEntity("Take," + module.id, time);
          //  TableOperation insertOperation1 = TableOperation.Insert(customer1);
          //  table.Execute(insertOperation1);

          //  int num2 = 0;
          //  foreach (var unit in units) {
          //    if (dataadd.dat.Count == num2) {
          //      break;
          //    }
          //    DataEntity customer2 = new DataEntity("Value," + module.id, time + "," + unit.id);
          //    customer2.DataVal = dataadd.dat[num2];
          //    TableOperation insertOperation2 = TableOperation.Insert(customer2);
          //    table.Execute(insertOperation2);
          //    num2++;
          //  }

          //  num1++;

          //}

          //module.Latest = date;
          //module.NumData += num1;
          //db.SaveChanges();

          return "Success!";

          //return value;
        }

        // PUT api/dataaddtest/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/dataaddtest/5
        public void Delete(int id)
        {
        }

        //日付→2030/1/1からの距離(100μs単位)
        string GetTimeIndex(string nowtime) {

          Debug.WriteLine(nowtime);
          DateTime T1 = DateTime.ParseExact(nowtime, "yyyyMMdd-HHmmss-ffff",
                                            System.Globalization.DateTimeFormatInfo.InvariantInfo,
                                            System.Globalization.DateTimeStyles.None);
          //DateTime T1 = DateTime.ParseExact(nowtime, "yyyy-MM-dd",
          //                                  System.Globalization.DateTimeFormatInfo.InvariantInfo,
          //                                  System.Globalization.DateTimeStyles.None);
          DateTime T2 = new DateTime(2030, 1, 1);
          //return (T2 - T1).TotalMilliseconds.ToString();
          long difference = long.Parse(String.Format("{0:G}", ((T2 - T1).TotalMilliseconds * 10.0)));
          return String.Format("{0:D13}", difference);
          //return T1.ToString();
        }

        //2030/1/1からの距離(100μs単位)→日付
        string GetDateIndex(string Sdifference) {

          long Ldifference = long.Parse(Sdifference) * 1000;
          DateTime T1 = new DateTime(Ldifference);
          DateTime T2 = new DateTime(2030, 1, 1);
          long difference = long.Parse(String.Format("{0:G}", ((T2 - T1).TotalMilliseconds * 10.0))) * 1000;
          DateTime T3 = new DateTime(difference);

          //return T1.ToString("yyyyMMdd-HHmmss-ffff");
          return T3.ToString("yyyyMMdd-HHmmss-ffff");

        }

        //データの追加関数
        void Execute(ITableEntity name) {
          TableOperation insertOperation = TableOperation.Insert(name);
          table.Execute(insertOperation);
        }

    }
}
