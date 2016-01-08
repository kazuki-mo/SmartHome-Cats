using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models {

  public class DataAdd {

    public string dt { get; set; }          // アップロードデータの更新時刻
    public string pw { get; set; }          // パスワード
    public List<string> dat { get; set; }   // アップロードデータ群

  }
}