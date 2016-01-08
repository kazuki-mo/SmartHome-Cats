using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models {
  public class Malldata {

    public int Id;                          // モジュールID
    public List<string> Type;               // データ種類群
    public List<int> TypeDataId;            // データ型ID群
    public List<List<string>> data;         // 表示したいデータ群
    public List<bool> FlagBlob;             // Blobデータかどうか
    public string ModuleName { get; set; }  // モジュール名
    public int NumData;                     // そのモジュールのもつデータ数
    public int TakeNum;                     // 表示したいデータの開始件数
    public string DateStart { get; set; }   // 日付検索（最近）
    public string DateEnd { get; set; }     // 日付検索（最古）
    public bool Deleting;                   // 今、データを削除中かどうか

  }
}