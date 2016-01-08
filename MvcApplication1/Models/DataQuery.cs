using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models {
  public class DataQuery {

    public string pw { get; set; }          // パスワード
    public string since { get; set; }       // 日付検索（最古）
    public string until { get; set; }       // 日付検索（最近）
    public int num { get; set; }            // 取得したいデータの数
    public string orderby { get; set; }     // 取得したいデータは昇順か降順か
    public List<int> unitlist { get; set; } // 取得したいデータの列インデックス
    public bool noDate { get; set; }        // 更新時刻を取得したいかどうか

  }
}