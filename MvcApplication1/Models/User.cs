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
    
    public partial class User
    {
        public User()
        {
            this.Modules = new HashSet<Module>();
        }
    
        public int id { get; set; }
        public string idName { get; set; }
        public string PassWord { get; set; }
        public string NickName { get; set; }
        public string Affiliation { get; set; }
        public string Detail { get; set; }
        public string MailAddress { get; set; }
        public string CellPhoneNum { get; set; }
        public string PhoneNum { get; set; }
    
        public virtual ICollection<Module> Modules { get; set; }
    }
}
