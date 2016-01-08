using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models {
  public class moduleinfo {

    public string Location;     // 設置場所
    public string Detail;       // モジュールの詳細情報
    public int NumData;         // モジュールの持つデータの数
    public string Latest;       // データアップロードの最終更新時刻
    public List<string> Type;   // モジュールの持つデータのデータ種類群

  }
}