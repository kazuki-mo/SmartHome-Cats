//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class Module
    {
        public Module()
        {
            this.Users = new HashSet<User>();
            this.Units = new HashSet<Unit>();
        }
    
        public int id { get; set; }
        public string Name { get; set; }
        public string rPassWord { get; set; }
        public string Location { get; set; }
        public string Detail { get; set; }
        public int NumData { get; set; }
        public string Latest { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public string wPassWord { get; set; }
    
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Unit> Units { get; set; }
    }
}
