using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;

using System.Security.Cryptography;
using System.Text;

namespace MvcApplication1 {
  public class Common {

    //日付→2030/1/1からの距離(100μs単位)
    public string GetTimeIndex(string nowtime) {

      DateTime T1 = DateTime.ParseExact(nowtime, "yyyyMMdd-HHmmss-ffff",
                                        System.Globalization.DateTimeFormatInfo.InvariantInfo,
                                        System.Globalization.DateTimeStyles.None);
      DateTime T2 = new DateTime(2030, 1, 1);
      long difference = long.Parse(String.Format("{0:G}", ((T2 - T1).TotalMilliseconds * 10.0)));
      return String.Format("{0:D13}", difference);

    }

    //2030/1/1からの距離(100μs単位)→日付
    public string GetDateIndex(string Sdifference) {

        System.Diagnostics.Debug.WriteLine(Sdifference);

      long Ldifference = long.Parse(Sdifference) * 1000;
      DateTime T1 = new DateTime(Ldifference);
      DateTime T2 = new DateTime(2030, 1, 1);
      long difference = long.Parse(String.Format("{0:G}", ((T2 - T1).TotalMilliseconds * 10.0))) * 1000;
      DateTime T3 = new DateTime(difference);
      return T3.ToString("yyyyMMdd-HHmmss-ffff");

    }

    //AzureTableにアクセス
    public CloudTable AzureAccess() {

      //研究室サーバにアップロードする場合//
      String ConnectionString = "UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://127.0.0.1:10002";
      
      //WindowsAzureにアップロードする場合//
      
      CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);
      CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
      String TableName = "CatsTable01"; // 研究室サーバ
      //String TableName = "CatsTable01"; // WindowsAzure
      CloudTable table = tableClient.GetTableReference(TableName);
      table.CreateIfNotExists();
      return table;
    }

    //AzureBlobにアクセス
    public CloudBlobContainer BlobAccess() {
      // 現状は、研究室サーバにアップロードする場合でしか動作確認できていない。
      // WindowsAzureにアップロードする場合は、ConnectionStringを変更しなければならないはずであるが、まだ確認できていません。

      String ConnectionString = "UseDevelopmentStorage=true;";
      CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);

      CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
      CloudBlobContainer container = blobClient.GetContainerReference("catscontainer01");

      container.CreateIfNotExists();

      BlobContainerPermissions permission = new BlobContainerPermissions();
      permission.PublicAccess = BlobContainerPublicAccessType.Blob;
      container.SetPermissions(permission);

      return container;
    }

    //パスワードの比較関数
    public bool PasswordError(string Password, string ModulePassword) {

      if (Password == null) {
        if (!(ModulePassword == null)) {
          return true;
        }
      } else if (ModulePassword == null) {
        return false;
      } else if (!Password.Equals(ModulePassword)) {
        return true;
      }

      return false;

    }

    //パスワード(ハッシュmd5)の出力
    public string GetHashPassword(string date, string ModuleName, string ModulePassword) {

      string[] dates = date.Split('-');
      // 「年月日」＋「モジュール名」＋「時分秒」＋「パスワード」＋「100μ秒」の文字列をエンコードする
      // アップロード側も、この規則に従ってハッシュmd5で暗号化をした状態で送らなければならない。
      byte[] byteValue = Encoding.UTF8.GetBytes(dates[0] + ModuleName + dates[1] + ModulePassword + dates[2]);
      var md5 = new MD5CryptoServiceProvider();
      byte[] hashValue = md5.ComputeHash(byteValue);
      StringBuilder hashedText = new StringBuilder();
      for (int i = 0; i < hashValue.Length; i++) {
        hashedText.AppendFormat("{0:X2}", hashValue[i]);
      }
      return hashedText.ToString().ToLower();

    }

  }
}