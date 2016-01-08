using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data;
using System.Data.Entity;
using MvcApplication1.Models;

namespace MvcApplication1.Controllers
{
    public class modulelistController : ApiController
    {

      private RDataBase RDB = new RDataBase();

      // ユーザの持つモジュール一覧の取得
      // GET api/modulelist/{UserID名}
      public List<string> Get(string fir) {

        List<string> modulelist = new List<string>();

        foreach(var x in RDB.db.Users.Where(p => p.idName.Equals(fir)).Single().Modules){
          modulelist.Add(x.Name);
        }
        
        return modulelist;
      }

    }
}
