// テスト用Controller

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text;

using MvcApplication1.Models;

using System.Web.Script.Serialization;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System.Configuration;

using System.Diagnostics;

using System.Web;
using System.Web.Mvc;

namespace MvcApplication1.Controllers
{
    public class TestModuleController : ApiController{

    //private Sample01REntities db = new Sample01REntities();

      private RDataBase RDB = new RDataBase();
      
      private ChangeUserInfoModel CUmodel = new ChangeUserInfoModel();
    private ChangeModuleInfoModel CMmodel = new ChangeModuleInfoModel();

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

        // GET api/testmodule
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/testmodule/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/testmodule
//        public string Post([FromBody]JsonModelTest value)
        //public string Post(JsonModelTest value)
        public string Post([FromBody]string value)
        {

          string mes = value;

          try {

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            JsonModelTest test2 = serializer.Deserialize<JsonModelTest>(value);
          } catch {
            mes += ":error";
          }


          ////Convert back from the base64string
          //byte[] fromBase64 = Convert.FromBase64String(value.dat[2]);
          //string result = Encoding.UTF8.GetString(fromBase64);

          //String ConnectionString = "UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://127.0.0.1:10002";
          //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);
          //tableClient = storageAccount.CreateCloudTableClient();
          //String TableName = "Sample06";
          //table = tableClient.GetTableReference(TableName);

          //string test = test2.dt + "," + test2.pw;
          //foreach (string x in test2.dat) {

          //  test += "," + x;

          //}
          ////test += result;

          ////DataEntity customer1 = new DataEntity("Take,8", time);
          //DataEntity customer1 = new DataEntity("test1", "test15");
          //customer1.DataVal = test;

          //// Create the TableOperation that inserts the customer entity.
          //TableOperation insertOperation = TableOperation.Insert(customer1);

          //// Execute the insert operation.
          //table.Execute(insertOperation);

          return mes;

          //return value;
        }

        // PUT api/testmodule/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/testmodule/5
        public void Delete(int id)
        {
        }
    }
}
