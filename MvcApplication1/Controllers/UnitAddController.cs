using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.Web.Mvc;

using System.Diagnostics;

using MvcApplication1.Models;
using System.Web.Script.Serialization;

using System.Data;
using System.Data.Entity;

namespace MvcApplication1.Controllers
{
    public class UnitAddController : ApiController
    {

      private RDataBase RDB = new RDataBase();

      private Common common = new Common();

      // データ種類の追加
      // POST api/unitadd/{User名}/{Module名}
      public string Post(string fir, string sec, [FromBody]UnitAdd unitadd) {

          var user = RDB.db.Users.Where(p => p.idName.Equals(fir)).Single();
          var module = user.Modules.Where(p => p.Name.Equals(sec)).Single();
          var units = module.Units;

          if (common.PasswordError(unitadd.pw, module.wPassWord)) {
            return "Password Error";
          }

          foreach (var unitlist in unitadd.ul) {

            Unit unit = new Unit();
            string[] unitype = unitlist.Split('|');
            unit.Unit1 = unitype[0];
            unit.TypeDataId = int.Parse(unitype[1]);
            unit.Modules.Add(module);
            RDB.db.Units.Add(unit);

            RDB.db.SaveChanges();

          }

          return "Success!";

        }

    }
}
