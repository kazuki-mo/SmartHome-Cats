// テスト用Controller

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using MvcApplication1.Models;

using System.Security.Cryptography;
using System.Text;

namespace MvcApplication1.Controllers
{
    public class TestController : ApiController
    {
        //GET api/test
        public string Get() {
          return "test" ;
        }

        // GET api/test/5
        public string Get(string fir)
        {

          byte[] byteValue = Encoding.UTF8.GetBytes(fir);
          var md5 = new MD5CryptoServiceProvider();
          byte[] hashValue = md5.ComputeHash(byteValue);
          StringBuilder hashedText = new StringBuilder();
          for (int i = 0; i < hashValue.Length; i++) {
            hashedText.AppendFormat("{0:X2}", hashValue[i]);
          }
          string re = hashedText.ToString().ToLower();

            return re;
        }

        // POST api/test
        public string Post([FromBody]string value) {

          byte[] byteValue = Encoding.UTF8.GetBytes("aaa");
          var md5 = new MD5CryptoServiceProvider();
          byte[] hashValue = md5.ComputeHash(byteValue);
          StringBuilder hashedText = new StringBuilder();
          for (int i = 0; i < hashValue.Length; i++) {
            hashedText.AppendFormat("{0:X2}", hashValue[i]);
          }

          string re = null;

          if (value.Equals(hashedText.ToString().ToLower())) {
            re = "一致";
          } else {
            re = "不一致";
          }

          return re;
          //return value;
        }
    }
}
