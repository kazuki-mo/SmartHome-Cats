using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

namespace MvcApplication1.Controllers {
  public class HomeController : Controller {

    private CloudTable table; // Azure Table

    private Common common = new Common();

    private RDataBase RDB = new RDataBase();

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

    // ホーム画面
    public ActionResult Index() {

      return View(RDB);
    }

    // 何もなし
    public ActionResult About() {
      return View(RDB);
    }

    // ユーザ情報の表示
    public ActionResult Contact() {
      return View(RDB);
    }

    // ユーザ情報の変更
    public ActionResult ChangeUserInfo() {
      ViewBag.Message = "ユーザ情報の変更";

      ChangeUserInfoModel CUmodel = new ChangeUserInfoModel();

      // RDBの中から、ログインしてるユーザを検索
      var loginuser = RDB.db.Users.Where(p => p.idName.Equals(User.Identity.Name)).Single();

      CUmodel.UserName = loginuser.idName;
      CUmodel.NickName = loginuser.NickName;
      CUmodel.Affiliation = loginuser.Affiliation;
      CUmodel.Detail = loginuser.Detail;
      CUmodel.MailAddress = loginuser.MailAddress;
      CUmodel.CellPhoneNum = loginuser.CellPhoneNum;
      CUmodel.PhoneNum = loginuser.PhoneNum;

      return View(CUmodel);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult ChangeUserInfo(ChangeUserInfoModel model) {

      // モデルが利用可能なら
      if (ModelState.IsValid) {

        // RDBの中から、ログインしてるユーザの検索
        User user = RDB.db.Users.Where(p => p.idName.Equals(User.Identity.Name)).Single();

        if (model.NickName == null) {
          user.NickName = model.UserName;
        } else {
          user.NickName = model.NickName;
        }
        user.Affiliation = model.Affiliation;
        user.Detail = model.Detail;
        user.MailAddress = model.MailAddress;
        user.CellPhoneNum = model.CellPhoneNum;
        user.PhoneNum = model.PhoneNum;

        RDB.db.SaveChanges();

        return RedirectToAction("Contact", "Home");
      }

      // ここで問題が発生した場合はフォームを再表示します
      return View(model);
    }

    // モジュール情報の表示
    [Authorize]
    public ActionResult Detail(String id) {

      var loginuser = RDB.db.Users.Where(p => p.idName.Equals(User.Identity.Name)).Single();
      Module targetmodule = loginuser.Modules.Where(p => p.Name.Equals(id)).Single();

      return View(targetmodule);
    }

    // モジュール情報の変更
    public ActionResult ChangeModuleInfo(Module model) {

      ChangeModuleInfoModel CMmodel = new ChangeModuleInfoModel();

      CMmodel.BeforeName = model.Name;
      CMmodel.ModuleName = model.Name;
      CMmodel.rPassword = model.rPassWord;
      CMmodel.rPWCheck = true;
      CMmodel.wPassword = model.wPassWord;
      CMmodel.wPWCheck = true;
      CMmodel.Location = model.Location;
      CMmodel.Detail = model.Detail;

      return View(CMmodel);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult ChangeModuleInfo(ChangeModuleInfoModel model) {
      
      
      if (ModelState.IsValid) {

        var module = RDB.db.Users.Where(p => p.idName.Equals(User.Identity.Name)).Single().Modules.Where(p => p.Name.Equals(model.BeforeName)).Single();

        // 入力されたモジュール名が既に使用されているかどうかを調べる
        bool Name = false;
        foreach (var x in RDB.db.Users.Where(p => p.idName.Equals(User.Identity.Name)).Single().Modules) {
          if (x.Name.Equals(model.ModuleName)) {
            Name = true;
            if (x.Name.Equals(model.BeforeName)) {
              Name = false;
            }
          }
        }
        if (Name) {
          ModelState.AddModelError("", "そのモジュール名は既に使用しています。");
          return View(model);
        } else {
          module.Name = model.ModuleName;
        }

        if (!model.rPWCheck) {
          module.rPassWord = model.rPassword;
        }
        if (!model.wPWCheck) {
          module.wPassWord = model.wPassword;
        }
        module.Location = model.Location;
        module.Detail = model.Detail;

        RDB.db.SaveChanges();

        return RedirectToAction("Detail", "Home", new { id = module.Name });
      }

      // ここで問題が発生した場合はフォームを再表示します
      return View(model);
    }

    // モジュールの新規作成
    [AllowAnonymous]
    public ActionResult ModuleCreate() {
      return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult ModuleCreate(ChangeModuleInfoModel model) {
      
      if (ModelState.IsValid) {

        User user = RDB.db.Users.Where(p => p.idName.Equals(User.Identity.Name)).Single();
        Module module = new Module();

        // 入力されたモジュール名が既に使用されているかどうかを調べる
        bool Name = false;
        foreach (var x in user.Modules) {
          if (x.Name.Equals(model.ModuleName)) {
            Name = true;
          }
        }
        if (Name) {
          ModelState.AddModelError("", "そのモジュール名は既に使用しています。");
          return View(model);
        } else {
          module.Name = model.ModuleName;
        }

        if (model.rPWCheck) {
          model.rPassword = null;
        } else {
          module.rPassWord = model.rPassword;
        }
        if (model.wPWCheck) {
          model.wPassword = null;
        } else {
          module.wPassWord = model.wPassword;
        }

        module.Location = model.Location;
        module.Detail = model.Detail;
        module.NumData = 0;
        module.Type = "0";

        module.Users.Add(user);
        RDB.db.Modules.Add(module);
        RDB.db.SaveChanges();

        return RedirectToAction("Index", "Home");
      }

      // ここで問題が発生した場合はフォームを再表示します
      return View(model);
    }

    // データの種類の変更
    public ActionResult ChangeTypeData(Module model) {

      ChangeTypeData CTDmodel = new ChangeTypeData();

      CTDmodel.ModuleName = model.Name;
      CTDmodel.UnitList = new List<string>();
      CTDmodel.TypeDataList = new List<string>();
      CTDmodel.TypeDataAllList = new List<string>();

      //今設定されているデータ種類(名前とデータ型)の取得
      foreach (var unit in RDB.db.Modules.Where(p => p.id == model.id).Single().Units) {
        CTDmodel.UnitList.Add(unit.Unit1);
        CTDmodel.TypeDataList.Add(RDB.db.TypeDatas.Where(p => p.id == unit.TypeDataId).Single().DataType);
      }

      //設定可能なデータ型を全て取得
      foreach (var typedata in RDB.db.TypeDatas) {
        CTDmodel.TypeDataAllList.Add(typedata.DataType);
      }


      return View(CTDmodel);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult ChangeTypeData(ChangeTypeData model) {

      var loginuser = RDB.db.Users.Where(p => p.idName.Equals(User.Identity.Name)).Single();
      var module = loginuser.Modules.Where(p => p.Name.Equals(model.ModuleName)).Single();

      //入力されたデータ種類とデータ型に変更（データ型は「変更しない」が選択されていると""（空文字）になる）
      int num = 0;
      foreach (var unit in module.Units) {
        unit.Unit1 = model.UnitList[num];
        if (!model.TypeDataList[num].Equals("")) {
          string typedata = model.TypeDataList[num];
          unit.TypeDataId = RDB.db.TypeDatas.Where(p => p.DataType.Equals(typedata)).Single().id;
        }
        RDB.db.SaveChanges();
        num++;
      }

      return RedirectToAction("Detail", "Home", new { id = module.Name });
    }

    // データの表示
    [AllowAnonymous]
    public ActionResult Data(string ModuleName, int takenum, string DateStart, string DateEnd) {

      table = common.AzureAccess();
      Malldata AllData = new Malldata();

      var loginuser = RDB.db.Users.Where(p => p.idName.Equals(User.Identity.Name)).Single();
      var module = loginuser.Modules.Where(p => p.Name.Equals(ModuleName)).Single();
      int id = module.id;
      var units = module.Units;

      // 削除中ならmoduleテーブルのTypeプロパティは"1"である（DataDeleteController参照）
      if (module.Type.Equals("1")) {
        AllData.ModuleName = ModuleName;
        AllData.Deleting = true;
        return View(AllData);
      } else {
        AllData.Deleting = false;
      }

      // データ数が0なら
      if (module.NumData == 0) {
        AllData.ModuleName = ModuleName;
        AllData.NumData = 0;
        return View(AllData);
      }

      // AllDataの中には、takenum件から20件のValueデータが入る
      TableQuery<DataEntity> query = GetQueryTakeNum(id, takenum, DateStart, DateEnd);
      AllData = GetData(query, units);

      //takenum件から20件のTakeデータを取得して、DataValプロパティが"BlobData"ならば、そのデータはBlobデータである。
      TableQuery<DataEntity> query1 = new TableQuery<DataEntity>().Where(
          TableQuery.CombineFilters(
          TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Take," + id),
          TableOperators.And,
          TableQuery.CombineFilters(
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, common.GetTimeIndex(DateStart)),
          TableOperators.And,
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, common.GetTimeIndex(DateEnd))
          )
        )).Take(takenum);
      TableQuery<DataEntity> query2 = new TableQuery<DataEntity>().Where(
        TableQuery.CombineFilters(
        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Take," + id),
        TableOperators.And,
        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, table.ExecuteQuery(query1).LastOrDefault().RowKey)
      )).Take(20);
      AllData.FlagBlob = new List<bool>();
      foreach (var entity in table.ExecuteQuery(query2)) {
        if (entity.DataVal == null) {
          AllData.FlagBlob.Add(false);
        } else {
          if (entity.DataVal.Equals("BlobData")) {
            AllData.FlagBlob.Add(true);
          } else {
            AllData.FlagBlob.Add(false);
          }
        }
      }

      AllData.ModuleName = ModuleName;
      AllData.NumData = GetDataCount(id, DateStart, DateEnd);
      AllData.TakeNum = takenum;
      AllData.Id = id;
      AllData.DateStart = DateStart;
      AllData.DateEnd = DateEnd;



      return View(AllData);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult Data(Malldata Alldata) {
      //ここには、日付検索時にやってくる。基本は上のと同じ

      table = common.AzureAccess();

      var loginuser = RDB.db.Users.Where(p => p.idName.Equals(User.Identity.Name)).Single();
      int id = loginuser.Modules.Where(p => p.Name.Equals(Alldata.ModuleName)).Single().id;
      var units = RDB.db.Modules.Where(x => x.id.Equals(id)).Single().Units;

      TableQuery<DataEntity> query = GetQueryTakeNum(id, 1, Alldata.DateStart, Alldata.DateEnd);

      Malldata Alldata_tmp = GetData(query, units);

      TableQuery<DataEntity> query1 = new TableQuery<DataEntity>().Where(
          TableQuery.CombineFilters(
          TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Take," + id),
          TableOperators.And,
          TableQuery.CombineFilters(
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, common.GetTimeIndex(Alldata.DateStart)),
          TableOperators.And,
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, common.GetTimeIndex(Alldata.DateEnd))
          )
        )).Take(20);

      Alldata.FlagBlob = new List<bool>();
      foreach (var entity in table.ExecuteQuery(query1)) {
        if (entity.DataVal == null) {
          Alldata.FlagBlob.Add(false);
        } else {
          if (entity.DataVal.Equals("BlobData")) {
            Alldata.FlagBlob.Add(true);
          } else {
            Alldata.FlagBlob.Add(false);
          }
        }
      }

      Alldata.Type = Alldata_tmp.Type;
      Alldata.TypeDataId = Alldata_tmp.TypeDataId;
      Alldata.data = Alldata_tmp.data;
      Alldata.Id = id;
      Alldata.NumData = GetDataCount(id, Alldata.DateStart, Alldata.DateEnd);
      Alldata.TakeNum = 1;

      return View(Alldata);
    }

    // Blobデータの表示
    [AllowAnonymous]
    public ActionResult BlobData(string ModuleName, string date, int takenum, string DateStart, string DateEnd) {

      Balldata BlobAllData = new Balldata();

      var loginuser = RDB.db.Users.Where(p => p.idName.Equals(User.Identity.Name)).Single();
      var module = loginuser.Modules.Where(p => p.Name.Equals(ModuleName)).Single();
      int id = module.id;

      BlobAdd blobdata = GetBlobAllData(id, date);  // Blob内のデータを全取得

      BlobAllData.ModuleName = ModuleName;
      BlobAllData.FileName = blobdata.filename;
      BlobAllData.date = date;
      BlobAllData.Type = blobdata.type;

      blobdata.dataaddlist = GetBlobData_Date(blobdata.dataaddlist, DateStart, DateEnd);  // 日付範囲内のデータを取得
      BlobAllData.data = GetBlobData_Take(blobdata.dataaddlist, takenum); // 日付範囲内のデータの内、次の20件を取得

      BlobAllData.NumData = blobdata.dataaddlist.Count;
      BlobAllData.TakeNum = takenum;
      BlobAllData.Id = id;
      BlobAllData.DateStart = DateStart;
      BlobAllData.DateEnd = DateEnd;

      return View(BlobAllData);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult BlobData(Balldata BlobAllData) {
      // 日付検索時にここに来る。

      var loginuser = RDB.db.Users.Where(p => p.idName.Equals(User.Identity.Name)).Single();
      var module = loginuser.Modules.Where(p => p.Name.Equals(BlobAllData.ModuleName)).Single();
      int id = module.id;

      BlobAdd blobdata = GetBlobAllData(id, BlobAllData.date);

      BlobAllData.Type = blobdata.type;

      // 日付範囲内のデータの最新20件を取得
      blobdata.dataaddlist = GetBlobData_Date(blobdata.dataaddlist, BlobAllData.DateStart, BlobAllData.DateEnd);
      BlobAllData.data = GetBlobData_Take(blobdata.dataaddlist, 1);

      BlobAllData.NumData = blobdata.dataaddlist.Count;
      BlobAllData.TakeNum = 1;
      BlobAllData.Id = id;

      return View(BlobAllData);
    }

    // まりもコードの編集
    [AllowAnonymous]
    public ActionResult ChangeCode(string ModuleName) {

      ChangeCode changecode = new ChangeCode();
      changecode.ModuleName = ModuleName;
      changecode.MarimoCode = RDB.db.Modules.Where(p => p.Name.Equals(ModuleName)).Single().Code;

      return View(changecode);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult ChangeCode(ChangeCode changecode) {

      RDB.db.Modules.Where(p => p.Name.Equals(changecode.ModuleName)).Single().Code = changecode.MarimoCode;
      RDB.db.SaveChanges();

      return RedirectToAction("Detail", "Home", new { id = changecode.ModuleName });
    }


    //AzureDBから、日付範囲内にあるデータをtakenum件から20件取得するクエリを取得
    TableQuery<DataEntity> GetQueryTakeNum(int id, int takenum, string DateStart, string DateEnd) {

      TableQuery<DataEntity> query1 = new TableQuery<DataEntity>().Where(
          TableQuery.CombineFilters(
          TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Take," + id),
          TableOperators.And,
          TableQuery.CombineFilters(
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, common.GetTimeIndex(DateStart)),
          TableOperators.And,
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, common.GetTimeIndex(DateEnd))
          )
        )).Take(takenum);

      TableQuery<DataEntity> query2 = new TableQuery<DataEntity>().Where(
        TableQuery.CombineFilters(
        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Take," + id),
        TableOperators.And,
        TableQuery.CombineFilters(
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, common.GetTimeIndex(DateStart)),
          TableOperators.And,
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, common.GetTimeIndex(DateEnd))
          )
      )).Take(takenum + 19);

      TableQuery<DataEntity> query3 = new TableQuery<DataEntity>().Where(
        TableQuery.CombineFilters(
        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Value," + id),
        TableOperators.And,
        TableQuery.CombineFilters(
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, table.ExecuteQuery(query1).LastOrDefault().RowKey),
          TableOperators.And,
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, (long.Parse(table.ExecuteQuery(query2).LastOrDefault().RowKey) + 1).ToString())
          )
      ));

      return query3;
    }

    //AzureDBから、日付範囲内にある全データの個数を取得
    int GetDataCount(int id, string DateStart, string DateEnd) {

      TableQuery<DataEntity> CountQuery = new TableQuery<DataEntity>().Where(
          TableQuery.CombineFilters(
          TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Take," + id),
          TableOperators.And,
          TableQuery.CombineFilters(
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, common.GetTimeIndex(DateStart)),
          TableOperators.And,
          TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, common.GetTimeIndex(DateEnd))
          )
        ));

      return table.ExecuteQuery(CountQuery).Count();

    }

    //query条件に一致するデータを取得(Azure)TypeとDataだけ
    Malldata GetData(TableQuery<DataEntity> query, ICollection<Unit> units) {

      Malldata AllData = new Malldata();

      string[] Type = new string[units.Count];
      int[] TypeDataId = new int[units.Count];
      int[] TypeId = new int[units.Count];
      int num = 0;

      foreach (var unit in units) {
        Type[num] = unit.Unit1;
        TypeDataId[num] = unit.TypeDataId;
        TypeId[num] = unit.id;
        num++;
      }

      AllData.Type = new List<string>();
      AllData.TypeDataId = new List<int>();
      AllData.data = new List<List<string>>();
      AllData.Type = Type.ToList();
      AllData.TypeDataId = TypeDataId.ToList();

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
            datatmp2[0] = common.GetDateIndex(time); //データの0列目は更新時刻
            datatmp1 = datatmp2.ToList();
            AllData.data.Add(datatmp1);
            datatmp1 = new List<string>();
            datatmp2 = new string[Type.LongLength + 1];
          }
          first = true;
        }

        // RDBのunitsテーブルのidとKVSのRowKeyのRowKeyの,の後の数字が一致している時にdatatmp2に格納
        // RDBにはあるがKVSには無い場合、そのデータは穴抜けのデータとなる。
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

    //IDとDATEから、Blobデータを全て取得
    BlobAdd GetBlobAllData(int ModuleId, string date) {

      string time = common.GetTimeIndex(date);
      CloudBlobContainer container = common.BlobAccess();
      CloudBlockBlob blockBlob = container.GetBlockBlobReference(ModuleId.ToString() + "," + time);

      string jsontext;
      using (var memoryStream = new MemoryStream()) {
        blockBlob.DownloadToStream(memoryStream);
        jsontext = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
      }

      JavaScriptSerializer serializer = new JavaScriptSerializer();
      BlobAdd blobdata = serializer.Deserialize<BlobAdd>(jsontext);

      return blobdata;
    }

    //Blobデータから、日付範囲内にあるBlobデータを全て取得
    List<DataAdd> GetBlobData_Date(List<DataAdd> datalist, string DateStart, string DateEnd) {

      List<DataAdd> ReDatalist = new List<DataAdd>();

      int end = 0;
      if (long.Parse(common.GetTimeIndex(datalist.FirstOrDefault().dt)) > long.Parse(common.GetTimeIndex(DateEnd))) {
        if (long.Parse(common.GetTimeIndex(datalist.LastOrDefault().dt)) > long.Parse(common.GetTimeIndex(DateEnd))) {
          return ReDatalist; // 日付検索の範囲内よりも過去の時刻のデータしか存在しないので、空のまま返す
        } else {
          foreach (var datas in datalist) {
            if (long.Parse(common.GetTimeIndex(datas.dt)) <= long.Parse(common.GetTimeIndex(DateEnd))) {
              break; // break時のendの中には、最初から数えて何番目データからが日付検索の範囲内かが格納される
            } else {
              end++;
            }
          }
        }
      }

      int start = datalist.Count;
      if (long.Parse(common.GetTimeIndex(datalist.LastOrDefault().dt)) < long.Parse(common.GetTimeIndex(DateStart))) {
        if (long.Parse(common.GetTimeIndex(datalist.FirstOrDefault().dt)) < long.Parse(common.GetTimeIndex(DateStart))) {
          return ReDatalist; // 日付検索の範囲内よりも未来の時刻のデータしか存在しないので、空のまま返す
        } else {
          start = 0;
          foreach (var datas in datalist) {
            if (long.Parse(common.GetTimeIndex(datas.dt)) <= long.Parse(common.GetTimeIndex(DateStart))) {
              break; // break時のstartの中には、最初から数えて何番目データまでが日付検索の範囲内に入るかが格納される
            } else {
              start++;
            }
          }
        }
      }

      datalist = datalist.Skip(end).Take(start + 1 - end).ToList();
      DataAdd[] tmpAllData = new DataAdd[datalist.Count];
      for (int i = 0; i < datalist.Count; i++) {
        tmpAllData[i] = datalist[datalist.Count - i - 1];
      }
      datalist = tmpAllData.ToList();

      return datalist;
    }

    //Blobデータから、takenum件から20件のBlobデータを取得
    List<List<string>> GetBlobData_Take(List<DataAdd> datalist, int takenum) {

      List<List<string>> ReDatalist = new List<List<string>>();

      int num = takenum;
      foreach (var datas in datalist.Skip(takenum - 1)) {
        if ((num > takenum + 19) || (num > datalist.Count)) {
          break;
        }
        List<string> tmpdata = new List<string>();
        tmpdata.Add(datas.dt);
        foreach (var data in datas.dat) {
          tmpdata.Add(data);
        }
        ReDatalist.Add(tmpdata);
        num++;
      }

      return ReDatalist;
    }

  }
}
