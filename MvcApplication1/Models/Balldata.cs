using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models {
  public class Balldata {

    public int Id;                          // モジュールID
    public List<string> Type;               // データ種類群
    public List<List<string>> data;         // データ群
    public string ModuleName { get; set; }  // モジュール名
    public string FileName { get; set; }    // Blobファイル名
    public string date { get; set; }        // Blobデータの更新時刻
    public int NumData;                     // Blob内データの数
    public int TakeNum;                     // 開始件数（最新から数えて）
    public string DateStart { get; set; }   // 日付検索範囲（最近）
    public string DateEnd { get; set; }     // 日付検索範囲（最古）


  }
}