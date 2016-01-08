using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data;
using System.Data.Entity;
using MvcApplication1.Models;

namespace MvcApplication1.Controllers {
    public class ModuleDeleteController : ApiController {

        private RDataBase RDB = new RDataBase();

        // モジュールの削除
        // POST api/moduledelete
        public string Post([FromBody]string ModuleName) {

            var user = RDB.db.Users.Where(p => p.idName.Equals(User.Identity.Name)).Single();
            var module = user.Modules.Where(p => p.Name.Equals(ModuleName)).Single();
            var units = module.Units;

            while (!(units.FirstOrDefault() == null)) {
                units.Remove(units.FirstOrDefault());
                //RDB.db.DeleteObject(units.FirstOrDefault());
                RDB.db.SaveChanges();
            }
            RDB.db.Modules.Remove(user.Modules.Where(p => p.Name.Equals(ModuleName)).Single());
            //RDB.db.DeleteObject(user.Modules.Where(p => p.Name.Equals(ModuleName)).Single());

            RDB.db.SaveChanges();

            return ModuleName;
        }

    }
}
