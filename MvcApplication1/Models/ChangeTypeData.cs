using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models {
  public class ChangeTypeData {

    public string ModuleName { get; set; }            // モジュール名
    public List<string> UnitList { get; set; }        // データ種類群
    public List<string> TypeDataList { get; set; }    // データ型群
    public List<string> TypeDataAllList { get; set; } // TapyeDataテーブルにあるデータ型全て

  }
}