using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;

using System.Data;
using System.Data.Entity;
using MvcApplication1.Models;

using System.Diagnostics;

namespace MvcApplication1.Controllers
{
    public class userController : ApiController
    {

      private RDataBase RDB = new RDataBase();

        // 登録されているユーザ一覧の取得
        // GET api/user
        public List<userlist> Get()
        {

          List<userlist> UserList = new List<userlist>();

          foreach(var x in RDB.db.Users){
            UserList.Add(new userlist { UserId=x.idName, UserName=x.NickName });
          }

            return UserList;
        }


        // ユーザの情報を取得（エラー処理スクリプト込み）
        // GET api/user/{ユーザID}
        public userinfo Get(string fir) {

          userinfo UserInfo = new userinfo();
          User x = new User();

          try {
            x = RDB.db.Users.Where(p => p.idName.Equals(fir)).FirstOrDefault();
            if (x == null) {
              Debug.WriteLine("Check!!");
              UserInfo.ErrorMessage = "ユーザ名：" + fir + " は存在しません。";
              return UserInfo;
            }
          
          } catch {
            UserInfo.ErrorMessage = "SQL Server が停止しています。";
            return UserInfo;
          }
          UserInfo.ErrorMessage = "Success!";
          UserInfo.NickName = x.NickName;
          UserInfo.Affiliation = x.Affiliation;
          UserInfo.Detail = x.Detail;
          UserInfo.MailAddress = x.MailAddress;
          UserInfo.CellPhoneNum = x.CellPhoneNum;
          UserInfo.PhoneNum = x.PhoneNum;
          
          
          return UserInfo;
        }

    }
}
