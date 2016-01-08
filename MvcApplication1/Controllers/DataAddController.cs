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

using System.Threading;
using System.Threading.Tasks;

using System.Security.Cryptography;
using System.Text;

namespace MvcApplication1.Controllers {
    public class DataAddController : ApiController {

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

        // （テスト用）POST api/dataadd
        public string Post([FromBody]string value) {

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<DataAdd> dataaddList = serializer.Deserialize<List<DataAdd>>(value);

            String ConnectionString = "UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://127.0.0.1:10002";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            String TableName = "Sample16";
            table = tableClient.GetTableReference(TableName);

            TableBatchOperation batchOperationTake = new TableBatchOperation();
            TableBatchOperation batchOperationValue = new TableBatchOperation();

            string date = String.Empty;

            Marimo marimo = new Marimo();
            //string[] codelist = { "i_i,0", "i_j,0", "while,count,6", "while,count,4", "send,i_j", "send,i_i", "i_i,i_i,+,1", "endw,3", "i_i,0", "i_j,i_j,+,1", "endw,2" };
            //string[] codelist = { "i_data2,get,nowdata,2","if,i_data2,>,100", "send,Worning!!" ,"endi,1" };
            bool Flag_MarimoCode = true;
            try {
                string codelists = RDB.db.Modules.Where(p => p.Name.Equals("HogerX01")).Single().Code.Replace(Environment.NewLine, "|");
                string[] codelist = codelists.Split('|');
                marimo.codelist = codelist;
            } catch {
                Flag_MarimoCode = false;
            }

            foreach (DataAdd dataadd in dataaddList) {

                marimo.dataadd = dataadd;
                if (Flag_MarimoCode) {
                    marimo.RunMarimo();
                }

                foreach (var data in marimo.dataadd.dat) {
                    Debug.WriteLine("Data: " + data);
                }

                date = marimo.dataadd.dt;
                string time = common.GetTimeIndex(date);

                TestEntity customer2 = new TestEntity("Value", time);
                customer2.DataVal = marimo.dataadd.dat;
                batchOperationValue.Insert(customer2);
                if (batchOperationValue.Count == 100) {
                    table.ExecuteBatch(batchOperationValue);
                    batchOperationValue = new TableBatchOperation();
                }

            }

            if (batchOperationTake.Count > 0) {
                table.ExecuteBatch(batchOperationTake);
            }
            if (batchOperationValue.Count > 0) {
                table.ExecuteBatch(batchOperationValue);
            }

            return "Success!";
        }


        // Azure Tableへアップロードされたデータを格納
        // POST api/dataadd/{UserID名}/{Module名}
        public string Post(string fir, string sec, [FromBody]List<DataAdd> dataaddList) {

            //string json = new JavaScriptSerializer().Serialize(dataaddList);
            //return json;
            if (dataaddList == null) {
                return "JSONの書式が間違っています。";
            } else if (dataaddList[0].dat == null) {
                return "データがnullです。";
            } else if (dataaddList.Count > 1) {
                foreach (DataAdd dataadd in dataaddList) {
                    if (dataadd.dt == null) {
                        return "2行以上のデータを送信する場合は、更新時刻データが必須です。";
                    }
                }
            }

            // RDBの中から、ターゲットモジュールなどを検索
            var user = RDB.db.Users.Where(p => p.idName.Equals(fir)).Single();
            var module = user.Modules.Where(p => p.Name.Equals(sec)).Single();
            var units = module.Units;

            // パスワードチェック（ハッシュ関数込み）
            try {
                if (!(module.wPassWord == null)) {
                    // データ追加時は、Hash関数で暗号化されたパスワードを使う
                    string HashPW = common.GetHashPassword(dataaddList[0].dt, sec, module.wPassWord);
                    try {
                        if (!(dataaddList[0].pw.Equals(HashPW))) {
                            return "PassWord error";
                        }
                    } catch {
                        return "You need to password";
                    }
                }
            } catch {
                return "PassWord Check Error";
            }

            try {
                table = common.AzureAccess(); // Azure Tableへアクセス
            } catch {
                return "Azure Access Error";
            }

            // batchを使って、まとめてデータを格納する
            List<TableBatchOperation> batchOperationTakeList = new List<TableBatchOperation>();
            List<TableBatchOperation> batchOperationValueList = new List<TableBatchOperation>();
            TableBatchOperation batchOperationTake = new TableBatchOperation();
            TableBatchOperation batchOperationValue = new TableBatchOperation();

            string date = "";
            int num1 = 0;

            Marimo marimo = new Marimo();

            // 入力されたコードの「改行」はまりもコードコンパイラの中では"|"として扱う
            // 「置換の際にエラーが起きる」＝「まりもコードは入力されていないので実行する必要なし」
            bool Flag_MarimoCode = true;
            try {
                string codelists = module.Code.Replace(Environment.NewLine, "|");
                string[] codelist = codelists.Split('|');
                marimo.codelist = codelist;
            } catch {
                Flag_MarimoCode = false;
            }

            // データ格納中の場合はModuleテーブルのTypeプロパティを"2"にする
            module.Type = "2";
            RDB.db.SaveChanges();

            try {

                foreach (DataAdd dataadd in dataaddList) {

                    // 送られてくる行データ毎にまりもコード実行
                    marimo.dataadd = dataadd;
                    if (Flag_MarimoCode) {
                        marimo.RunMarimo();
                    }

                    // 現在設定されているデータ種類の個数よりもアップロードされたデータの列数が多い場合、RDBにデータ種類を追加する
                    if (marimo.dataadd.dat.Count > units.Count) {
                        int count = marimo.dataadd.dat.Count - units.Count;
                        for (int i = 0; i < count; i++) {
                            Unit unit = new Unit();
                            unit.Unit1 = "";
                            unit.TypeDataId = 10;
                            unit.Modules.Add(module);
                            RDB.db.Units.Add(unit);
                        }
                        RDB.db.SaveChanges();
                        units = module.Units;
                    }

                    if (marimo.dataadd.dt == null) {
                        DateTime now = DateTime.Now;
                        date = now.ToString("yyyyMMdd-HHmmss-ffff");
                    } else {
                        date = marimo.dataadd.dt;
                    }
                    string time = common.GetTimeIndex(date);

                    // 100件ずつまとめてListに追加して、後でListごとまとめて格納する
                    DataEntity customer1 = new DataEntity("Take," + module.id, time);
                    batchOperationTake.Insert(customer1);
                    if (batchOperationTake.Count == 100) {
                        batchOperationTakeList.Add(batchOperationTake);
                        batchOperationTake = new TableBatchOperation();
                    }
                    int num2 = 0;
                    foreach (var unit in units) {
                        if (marimo.dataadd.dat.Count == num2) {
                            break;
                        }
                        DataEntity customer2 = new DataEntity("Value," + module.id, time + "," + unit.id);
                        customer2.DataVal = marimo.dataadd.dat[num2];
                        batchOperationValue.Insert(customer2);
                        if (batchOperationValue.Count == 100) {
                            batchOperationValueList.Add(batchOperationValue);
                            batchOperationValue = new TableBatchOperation();
                        }
                        num2++;
                    }

                    num1++;

                }

                if (batchOperationTake.Count > 0) {
                    batchOperationTakeList.Add(batchOperationTake);
                }
                if (batchOperationValue.Count > 0) {
                    batchOperationValueList.Add(batchOperationValue);
                }

                // 100件ずつまとめられたListを分散処理させながらKVSに格納
                Parallel.ForEach(batchOperationTakeList, Operation => {
                    table.ExecuteBatch(Operation);
                });
                Parallel.ForEach(batchOperationValueList, Operation => {
                    table.ExecuteBatch(Operation);
                });

                if (!date.Equals("")) {
                    module.Latest = date;
                }
                TableQuery<DataEntity> query = new TableQuery<DataEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Take," + module.id));
                module.NumData = table.ExecuteQuery(query).Count();

            } catch {
                module.Type = "0";
                RDB.db.SaveChanges();
                return "Error!";
            }

            // データ格納中以外はModuleテーブルのTypeプロパティは"0"となる
            module.Type = "0";
            RDB.db.SaveChanges();

            return "Success!";
        }

        // Parallel処理するデータ追加関数（現在は利用していない）
        //void insert(int i) {

        //  Debug.WriteLine("insert");

        //  TableBatchOperation batchOperationTake = new TableBatchOperation();
        //  TableBatchOperation batchOperationValue = new TableBatchOperation();

        //  string date = String.Empty;

        //  for (int j = i*100; j < i*100+100 ; j++) {

        //    if (dataaddList.Count == j) {
        //      break;
        //    }

        //    date = dataaddList[j].dt;
        //    string time = common.GetTimeIndex(date);

        //    DataEntity customer1 = new DataEntity("Take," + module.id, time);
        //    batchOperationTake.Insert(customer1);
        //    if (batchOperationTake.Count == 100) {
        //      table.ExecuteBatch(batchOperationTake);
        //      batchOperationTake = new TableBatchOperation();
        //    }

        //    int num2 = 0;
        //    foreach (var unit in units) {
        //      if (dataaddList[j].dat.Count == num2) {
        //        break;
        //      }
        //      DataEntity customer2 = new DataEntity("Value," + module.id, time + "," + unit.id);
        //      customer2.DataVal = dataaddList[j].dat[num2];
        //      batchOperationValue.Insert(customer2);
        //      if (batchOperationValue.Count == 100) {
        //        table.ExecuteBatch(batchOperationValue);
        //        batchOperationValue = new TableBatchOperation();
        //      }
        //      num2++;
        //    }

        //  }

        //  if (batchOperationTake.Count > 0) {
        //    table.ExecuteBatch(batchOperationTake);
        //  }
        //  if (batchOperationValue.Count > 0) {
        //    table.ExecuteBatch(batchOperationValue);
        //  }

        //}

    }
}
