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
    public class moduleController : ApiController
    {

      private RDataBase RDB = new RDataBase();

      // モジュール情報の取得
      // GET api/module/{ユーザID}/{モジュール名}
      public moduleinfo Get(string fir, string sec) {

        var user = RDB.db.Users.Where(p => p.idName.Equals(fir)).Single();
        var module = user.Modules.Where(p => p.Name.Equals(sec)).Single();
        var units = module.Units;

        moduleinfo ModuleInfo = new moduleinfo();
        ModuleInfo.Location = module.Location;
        ModuleInfo.Detail = module.Detail;
        ModuleInfo.NumData = module.NumData;
        ModuleInfo.Latest = module.Latest;

        string[] Type = new string[units.Count];
        int num = 0;
        foreach (var unit in units) {
          Type[num] = unit.Unit1;
          num++;
        }
        ModuleInfo.Type = new List<string>();
        ModuleInfo.Type = Type.ToList();

        return ModuleInfo;
      }

    }
}
