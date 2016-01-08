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
using System.Configuration;
using System.Diagnostics;

using System.IO;
using System.Web.Script.Serialization;

namespace MvcApplication1.Controllers {
  public class dataController : ApiController {

    private RDataBase RDB = new RDataBase();

    private CloudTable table; // Azure Table

    private Common common = new Common();

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

    // Azure Tableのデータを取得
    // POST api/data/{UserID名}/{Module名}
    public alldata Post(string fir, string sec, [FromBody]DataQuery dataquery) {

      alldata AllData = new alldata();  // 取得したデータをここに格納

      // RDBの中から、ターゲットモジュールなどを検索
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
      if (common.PasswordError(dataquery.pw, module.rPassWord)) {
        AllData.Type = new List<string>();
        AllData.Type.Add("Password Error");
        return AllData;
      }

      table = common.AzureAccess(); // Azure Tableへアクセス

      TableQuery<DataEntity> query = new TableQuery<DataEntity>();

      if ((dataquery.since == null) && (dataquery.until == null)) { //範囲指定なし
        query = new TableQuery<DataEntity>().Where(
        TableQuery.CombineFilters(
        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Value," + module.id),
        TableOperators.And,
        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, common.GetTimeIndex(module.Latest))
        ));
      } else if (dataquery.since == null) { //～untilのデータ取得
        query = new TableQuery<DataEntity>().Where(
        TableQuery.CombineFilters(
        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Value," + module.id),
        TableOperators.And,
        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, common.GetTimeIndex(dataquery.until))
      ));
      } else if (dataquery.until == null) { //since～のデータ取得
        query = new TableQuery<DataEntity>().Where(
        TableQuery.CombineFilters(
        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Value," + module.id),
        TableOperators.And,
        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, common.GetTimeIndex(dataquery.since) + 1)
      ));
      } else { //since～untilのデータ取得
        query = new TableQuery<DataEntity>().Where(
        TableQuery.CombineFilters(
        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Value," + module.id),
        TableOperators.And,
        TableQuery.CombineFilters(
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, common.GetTimeIndex(dataquery.since) + 1),
          TableOperators.And,
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, common.GetTimeIndex(dataquery.until))
          )
      ));
      }

      List<DataEntity> BeforeDataList = new List<DataEntity>();
      List<DataEntity> AfterDataList = new List<DataEntity>();

      BeforeDataList = table.ExecuteQuery(query).ToList<DataEntity>();
      AfterDataList = table.ExecuteQuery(query).ToList<DataEntity>();

      //"asc"なら逆順にする
      if (dataquery.orderby.Equals("asc")) {
        int count = BeforeDataList.Count();
        int i = 1;
        foreach (var BeforeData in BeforeDataList) {
          AfterDataList[count - i] = BeforeData;
          i++;
        }
      }

      string[] Type;
      int[] TypeId;

      int num = 0;

      //unitlistの中には、ユーザが取得したいデータの列インデックスが格納されている
      if (dataquery.unitlist == null) {
        Type = new string[units.Count];
        TypeId = new int[units.Count];
        foreach (var unit in units) {
          Type[num] = unit.Unit1;
          TypeId[num] = unit.id;
          num++;
        }
      } else {
        Type = new string[dataquery.unitlist.Count()];
        TypeId = new int[dataquery.unitlist.Count()];
        foreach (int index in dataquery.unitlist) {
          Type[num] = units.ElementAt(index - 1).Unit1;
          TypeId[num] = units.ElementAt(index - 1).id;
          num++;
        }
      }

      num = 2;

      AllData.Type = new List<string>();
      AllData.data = new List<List<string>>();

      AllData.Type = Type.ToList();

      List<string> datatmp1 = new List<string>();
      string[] datatmp2;
      if (dataquery.noDate) { //noDateがtrueなら、更新時刻分の列がいらない
        datatmp2 = new string[Type.LongLength];
        for (int i = 0; i < Type.LongLength; i++) {
          datatmp2[i] = string.Empty;
        }
      } else {
        datatmp2 = new string[Type.LongLength + 1];
        for (int i = 0; i < Type.LongLength + 1; i++) {
          datatmp2[i] = string.Empty;
        }
      }

      string time = string.Empty;

      bool first = false;

      foreach (DataEntity entity in AfterDataList) {


        if (time.Equals(entity.RowKey.Split(',')[0])) {
        } else {
          if (first) {
            if (!(dataquery.num == 0)) {
              if (num > dataquery.num) {
                break; // 要求されているデータ数を超えた時点でbreak
              }
            }
            if (!dataquery.noDate) { //noDateがfalseなら、更新時刻を1列目に追加
              datatmp2[0] = common.GetDateIndex(time);
            }
            datatmp1 = datatmp2.ToList();
            AllData.data.Add(datatmp1);
            datatmp1 = new List<string>();
            if (dataquery.noDate) {
              datatmp2 = new string[Type.LongLength];
            } else {
              datatmp2 = new string[Type.LongLength + 1];
            }
            num++;
          }
          first = true;
        }

        // RDBのデータ種類IDと一致する場合のみdatatmpに格納
        for (int i = 0; i < Type.LongLength; i++) {
          if (units.Where(p => p.id == int.Parse(entity.RowKey.Split(',')[1])).Single().id.Equals(TypeId[i])) {
            if (dataquery.noDate) {
              datatmp2[i] = entity.DataVal;
            } else {
              datatmp2[i + 1] = entity.DataVal;
            }
          }
        }

        time = entity.RowKey.Split(',')[0];



      }
      if (!dataquery.noDate) {
        datatmp2[0] = common.GetDateIndex(time);
      }
      datatmp1 = datatmp2.ToList();
      AllData.data.Add(datatmp1);

      return AllData;
    }


    #region テスト用
    //(テスト用) GET api/data/{UserID名}/{Module名}/{UnitsIndex1,UnitsIndex2,…} or "all"
    public alldata Get(string fir, string sec, string thi) {

      alldata AllData = new alldata();

      table = common.AzureAccess();

      var user = RDB.db.Users.Where(p => p.idName.Equals(fir)).Single();
      var module = user.Modules.Where(p => p.Name.Equals(sec)).Single();
      var units = module.Units;

      int id = module.id;

      try {

        TableQuery<DataEntity> query = new TableQuery<DataEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Value," + id));

        AllData = GetData(query, units,thi);

        return AllData;
      } catch {
        return AllData;
      }
    }

    //(テスト用) GET api/data/{ユーザID名}/{モジュール名}/{item}/{最初の件数}/{最後の件数}/{UnitsIndex1,UnitsIndex2,…} or "all"
    //(テスト用) GET api/data/{ユーザID名}/{モジュール名}/{date}/{日時(最古)}/{日時(最新)}/{UnitsIndex1,UnitsIndex2,…} or "all"
    public alldata Get(string fir, string sec, string thi, string fou, string fif, string six) {

      alldata AllData = new alldata();

      table = common.AzureAccess();

      var user = RDB.db.Users.Where(p => p.idName.Equals(fir)).Single();
      var module = user.Modules.Where(p => p.Name.Equals(sec)).Single();
      var units = module.Units;

      int id = module.id;

      //int id = db.Users.Where(p => p.idName.Equals(fir)).Single().Modules.Where(p => p.Name.Equals(sec)).Single().id;

      TableQuery<DataEntity> query3 = new TableQuery<DataEntity>();

      if (thi.Equals("item")) {

        TableQuery<DataEntity> query1 = new TableQuery<DataEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Take," + id)).Take(int.Parse(fou));

        TableQuery<DataEntity> query2 = new TableQuery<DataEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Take," + id)).Take(int.Parse(fif));

        query3 = new TableQuery<DataEntity>().Where(
          TableQuery.CombineFilters(
          TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Value," + id),
          TableOperators.And,
          TableQuery.CombineFilters(
            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, table.ExecuteQuery(query1).LastOrDefault().RowKey),
            TableOperators.And,
            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, (long.Parse(table.ExecuteQuery(query2).LastOrDefault().RowKey) + 1).ToString())
            )
        ));
      }
      if (thi.Equals("date")){
        query3 = new TableQuery<DataEntity>().Where(
        TableQuery.CombineFilters(
        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Value,"+id),
        TableOperators.And,
        TableQuery.CombineFilters(
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, common.GetTimeIndex(fou)+1),
          TableOperators.And,
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, common.GetTimeIndex(fif))
          )
      ));
      }

      AllData = GetData(query3,units,six);

      return AllData;
    }

    //(テスト用) POST api/data
    public string Post([FromBody]string data) {
      return data;
    }



    //(テスト用) POST api/data/{UserID名}/{Module名}/{UnitsIndex1,UnitsIndex2,…} or "all"
    public alldata Post(string fir, string sec,string thi, [FromBody]string data) {

      alldata AllData = new alldata();

      table = common.AzureAccess();

      var user = RDB.db.Users.Where(p => p.idName.Equals(fir)).Single();
      var module = user.Modules.Where(p => p.Name.Equals(sec)).Single();
      var units = module.Units;
      int id = module.id;

      if (common.PasswordError(data, module.rPassWord)) {
        AllData.Type = new List<string>();
        AllData.Type.Add("Password Error");
        return AllData;
      }

      try {

        TableQuery<DataEntity> query = new TableQuery<DataEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Value," + id));

        AllData = GetData(query, units,thi);

        return AllData;
      } catch {
        return AllData;
      }
      
    }

    //(テスト用) POST api/data/{UserID名}/{Module名}/{item}/{最初の件数}/{最後の件数}/{UnitsIndex1,UnitsIndex2,…} or "all"
    //(テスト用) POST api/data/{UserID名}/{Module名}/{date}/{日時(最古)}/{日時(最新)}/{UnitsIndex1,UnitsIndex2,…} or "all"
    public alldata Post(string fir, string sec, string thi, string fou, string fif, string six, [FromBody]string data) {

      alldata AllData = new alldata();

      table = common.AzureAccess();

      var user = RDB.db.Users.Where(p => p.idName.Equals(fir)).Single();
      var module = user.Modules.Where(p => p.Name.Equals(sec)).Single();
      var units = module.Units;
      int id = module.id;

      if(common.PasswordError(data,module.rPassWord)){
        AllData.Type = new List<string>();
        AllData.Type.Add("Password Error");
        return AllData;
      }

      TableQuery<DataEntity> query3 = new TableQuery<DataEntity>();

      if (thi.Equals("item")) {

        TableQuery<DataEntity> query1 = new TableQuery<DataEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Take," + id)).Take(int.Parse(fou));

        TableQuery<DataEntity> query2 = new TableQuery<DataEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Take," + id)).Take(int.Parse(fif));

        query3 = new TableQuery<DataEntity>().Where(
          TableQuery.CombineFilters(
          TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Value," + id),
          TableOperators.And,
          TableQuery.CombineFilters(
            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, table.ExecuteQuery(query1).LastOrDefault().RowKey),
            TableOperators.And,
            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, (long.Parse(table.ExecuteQuery(query2).LastOrDefault().RowKey) + 1).ToString())
            )
        ));
      }
      if (thi.Equals("date")) {
        query3 = new TableQuery<DataEntity>().Where(
        TableQuery.CombineFilters(
        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Value," + id),
        TableOperators.And,
        TableQuery.CombineFilters(
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, common.GetTimeIndex(fou) + 1),
          TableOperators.And,
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, common.GetTimeIndex(fif))
          )
      ));
      }

      AllData = GetData(query3, units, six);

      return AllData;

    }
    #endregion


    //query条件に一致するデータを取得(Azure)
    alldata GetData(TableQuery<DataEntity> query, ICollection<Unit> units, string unitsindex) {

      alldata AllData = new alldata();

      string[] indexes;

      string[] Type;
      int[] TypeId;

      int num = 0;

      if (unitsindex.Equals("all")) {
        Type = new string[units.Count];
        TypeId = new int[units.Count];
        foreach (var unit in units) {
          Type[num] = unit.Unit1;
          TypeId[num] = unit.id;
          num++;
        }
      } else {
        indexes = unitsindex.Split(',');
        Type = new string[indexes.Count()];
        TypeId = new int[indexes.Count()];
        foreach (var index in indexes) {
          Type[num] = units.ElementAt(int.Parse(index)-1).Unit1;
          TypeId[num] = units.ElementAt(int.Parse(index)-1).id;
          num++;
        }
      }
      
      

      AllData.Type = new List<string>();
      AllData.data = new List<List<string>>();

      AllData.Type = Type.ToList();

      List<string> datatmp1 = new List<string>();
      string[] datatmp2 = new string[Type.LongLength + 1];
      for (int i = 0; i < Type.LongLength + 1; i++) {
        datatmp2[i] = string.Empty;
      }

      string time = string.Empty;

      bool first = false;

      foreach (DataEntity entity in table.ExecuteQuery(query)) {

        if (time.Equals(entity.RowKey.Split(',')[0])) {
        } else {
          if (first) {
            datatmp2[0] = common.GetDateIndex(time);
            datatmp1 = datatmp2.ToList();
            AllData.data.Add(datatmp1);
            datatmp1 = new List<string>();
            datatmp2 = new string[Type.LongLength + 1];
          }
          first = true;
        }

        for (int i = 0; i < Type.LongLength; i++) {
          if (units.Where(p => p.id == int.Parse(entity.RowKey.Split(',')[1])).Single().id.Equals(TypeId[i])) {
            datatmp2[i + 1] = entity.DataVal;
          }
        }

        time = entity.RowKey.Split(',')[0];

      }
      datatmp2[0] = common.GetDateIndex(time);
      datatmp1 = datatmp2.ToList();
      AllData.data.Add(datatmp1);

      return AllData;

    }

  }
}
