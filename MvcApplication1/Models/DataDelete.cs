using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models {
  public class DataDelete {

    public string modulename { get; set; }  // モジュール名
    public string datestart { get; set; }   // 削除範囲（最近）
    public string dateend { get; set; }     // 削除範囲（最古）
    
  }
}