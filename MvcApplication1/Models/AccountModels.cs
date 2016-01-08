using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace MvcApplication1.Models {
  public class UsersContext : DbContext {
    public UsersContext()
      : base("DefaultConnection") {
    }

    public DbSet<UserProfile> UserProfiles { get; set; }
  }

  [Table("UserProfile")]
  public class UserProfile {
    [Key]
    [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }
    public string UserName { get; set; }
  }

  public class RegisterExternalLoginModel {
    [Required]
    [Display(Name = "ユーザー名")]
    public string UserName { get; set; }

    public string ExternalLoginData { get; set; }
  }

  public class LocalPasswordModel {
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "現在のパスワード")]
    public string OldPassword { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "{0} の長さは {2} 文字以上である必要があります。", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "新しいパスワード")]
    public string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "新しいパスワードの確認入力")]
    [Compare("NewPassword", ErrorMessage = "新しいパスワードと確認のパスワードが一致しません。")]
    public string ConfirmPassword { get; set; }
  }

  public class LoginModel {
    [Required]
    [Display(Name = "ユーザー名")]
    public string UserName { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "パスワード")]
    public string Password { get; set; }

    [Display(Name = "このアカウントを記憶する")]
    public bool RememberMe { get; set; }
  }

  public class RegisterModel {
    [Required]
    [Display(Name = "登録コード")]
    public string RegisterCode { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "ユーザー名")]
    public string UserName { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "{0} の長さは {2} 文字以上である必要があります。", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "パスワード")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "パスワードの確認入力")]
    [Compare("Password", ErrorMessage = "パスワードと確認のパスワードが一致しません。")]
    public string ConfirmPassword { get; set; }

    [StringLength(50, ErrorMessage = "[{0}] の長さは {1} 文字以下である必要があります。")]
    [Display(Name = "表示名")]
    public string NickName { get; set; }

    [Display(Name = "所属")]
    public string Affiliation { get; set; }

    [Display(Name = "ユーザの説明")]
    public string Detail { get; set; }

    [StringLength(320, ErrorMessage = "[{0}] の長さは {1} 文字以下である必要があります。")]
    [Display(Name = "メールアドレス")]
    public string MailAddress { get; set; }

    [StringLength(50, ErrorMessage = "[{0}] の長さは {1} 文字以下である必要があります。")]
    [Display(Name = "携帯電話番号")]
    public string CellPhoneNum { get; set; }

    [StringLength(50, ErrorMessage = "[{0}] の長さは {1} 文字以下である必要があります。")]
    [Display(Name = "電話番号")]
    public string PhoneNum { get; set; }

  }

  public class ChangeUserInfoModel {
    
    [StringLength(50)]
    [Display(Name = "ユーザー名")]
    public string UserName { get; set; }

    [StringLength(50, ErrorMessage = "[{0}] の長さは {1} 文字以下である必要があります。")]
    [Display(Name = "ニックネーム")]
    public string NickName { get; set; }

    [Display(Name = "所属")]
    public string Affiliation { get; set; }

    [Display(Name = "ユーザの説明")]
    public string Detail { get; set; }

    [StringLength(320, ErrorMessage = "[{0}] の長さは {1} 文字以下である必要があります。")]
    [Display(Name = "メールアドレス")]
    public string MailAddress { get; set; }

    [StringLength(50, ErrorMessage = "[{0}] の長さは {1} 文字以下である必要があります。")]
    [Display(Name = "携帯電話番号")]
    public string CellPhoneNum { get; set; }

    [StringLength(50, ErrorMessage = "[{0}] の長さは {1} 文字以下である必要があります。")]
    [Display(Name = "電話番号")]
    public string PhoneNum { get; set; }

  }

  public class ChangeModuleInfoModel {

    [Display(Name = "前のモジュール名")]
    public string BeforeName { get; set; }

    [Required]
    [StringLength(20, ErrorMessage = "[{0}] の長さは {1} 文字以下である必要があります。")]
    [Display(Name = "モジュール名")]
    public string ModuleName { get; set; }
    
    [StringLength(20, ErrorMessage = "{0} の長さは {1} 文字以下である必要があります。")]
    [DataType(DataType.Password)]
    [Display(Name = "データ閲覧用パスワード")]
    public string rPassword { get; set; }

    [StringLength(20, ErrorMessage = "{0} の長さは {1} 文字以下である必要があります。")]
    [DataType(DataType.Password)]
    [Display(Name = "データアップロード用パスワード")]
    public string wPassword { get; set; }

    [Display(Name = "パスワードを変更しない")]
    public bool rPWCheck { get; set; }

    [Display(Name = "パスワードを変更しない")]
    public bool wPWCheck { get; set; }

    [StringLength(50, ErrorMessage = "[{0}] の長さは {1} 文字以下である必要があります。")]
    [Display(Name = "設置場所")]
    public string Location { get; set; }

    [Display(Name = "モジュールの説明")]
    public string Detail { get; set; }

  }

  public class ExternalLogin {
    public string Provider { get; set; }
    public string ProviderDisplayName { get; set; }
    public string ProviderUserId { get; set; }
  }
}
