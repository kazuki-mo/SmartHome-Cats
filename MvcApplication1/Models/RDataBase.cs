using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models {
  public class RDataBase {
    
    //研究室サーバにアップロードする場合//
      public CatsDBEntities db = new CatsDBEntities();

    //WindowsAzureにアップロードする場合//
    //public cats_dbEntities db = new cats_dbEntities();

  }
}