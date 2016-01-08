using System.Data.Entity;

namespace MvcApplication1.Models {
  public class MvcApplication1Context : DbContext {
        // このファイルにカスタム コードを追加できます。変更は上書きされません。
        // 
        // モデル スキーマを変更するときに、自動的に Entity Framework によって
        // データベースを破棄して生成し直す場合は、次のコードを Global.asax の
        // Application_Start メソッドに追加してください。
        // 注意: モデルが変更されるたびに、データベースが破棄され、再作成されます。
        // 
        // System.Data.Entity.Database.SetInitializer(new System.Data.Entity.DropCreateDatabaseIfModelChanges<MvcApplication1.Models.MvcApplication1Context>());

        public MvcApplication1Context() : base("name=MvcApplication1Context")
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
  }
}
