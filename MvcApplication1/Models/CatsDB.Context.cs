﻿//------------------------------------------------------------------------------
// <auto-generated>
//    このコードはテンプレートから生成されました。
//
//    このファイルを手動で変更すると、アプリケーションで予期しない動作が発生する可能性があります。
//    このファイルに対する手動の変更は、コードが再生成されると上書きされます。
// </auto-generated>
//------------------------------------------------------------------------------

namespace MvcApplication1.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CatsDBEntities : DbContext
    {
        public CatsDBEntities()
            : base("name=CatsDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Module> Modules { get; set; }
        public DbSet<TypeData> TypeDatas { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
