using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using MvcApplication1.Models;

namespace MvcApplication1.Models {
  public class BlobAdd {

    public string filename { get; set; }            // Blobデータのファイル名
    public List<string> type { get; set; }          // データ種類群
    public List<DataAdd> dataaddlist { get; set; }  // アップロードデータ群

  }
}