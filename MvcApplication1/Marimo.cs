using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcApplication1.Models;
using System.Threading;
using System.Diagnostics;

namespace MvcApplication1 {
  public class Marimo {
    // RunMarimo()が実行されると、入力されたまりもコードに従ってプログラムが動く
    // まりもコードのコード規則については、OneDriveにある「まりもコード説明書.docx」を参照すべし

    public DataAdd dataadd;   //アップロードされた行データ
    public string[] codelist; //入力されたまりもコード

    static List<val_ints> val_int;
    static List<val_floats> val_float;

    public void RunMarimo() {

      //string[] codelist = { "while,true", "wait,0.5", "f_temp,get,temp", "send,f_temp", "if,f_temp,<,40", "break", "endi,4", "endw,0", "send,Worning!" };
      //string[] codelist = {"f_distance,0","f_before,get,temp","while,true","wait,1","f_after,get,temp","f_distance,f_after,-,f_before","send,f_distance","f_before,f_after","endw,2"};
      //string[] codelist = { "i_i,0", "i_j,0", "while,count,6", "while,count,4", "send,i_j", "send,i_i", "i_i,i_i,+,1", "endw,3", "i_i,0", "i_j,i_j,+,1", "endw,2" };

      val_int = new List<val_ints>();
      val_float = new List<val_floats>();
      List<bool> WhileCountStart = new List<bool>();
      List<int> WhileCount = new List<int>();

      for (int i = 0; i < codelist.Length; i++) {
        WhileCountStart.Add(false);
        WhileCount.Add(0);
      }

      string[] rowlist;
      float[] calite = new float[2];
      bool Flag_clause = false;

      int row = 0;
      int row_tmp = 0;

      while (true) {

        if (row == codelist.Length) {
          break;
        }

        rowlist = codelist[row].Split(',');

        if (rowlist[0].IndexOf('_') > -1) {

          // 変数代入("i_abc,5"or"i_abc,i_def")
          if (rowlist.Length == 2) {
            if (rowlist[0].Split('_')[0].Equals("i")) {
              if (val_int.Count != getadr_i(rowlist[0])) {
                val_int.RemoveAt(getadr_i(rowlist[0]));
              }
              if (rowlist[1].IndexOf('_') > 0) {
                val_int.Add(new val_ints { Name = rowlist[0].Split('_')[1], Val = val_int[getadr_i(rowlist[1])].Val });
              } else {
                val_int.Add(new val_ints { Name = rowlist[0].Split('_')[1], Val = int.Parse(rowlist[1]) });
              }
            } else if (rowlist[0].Split('_')[0].Equals("f")) {
              if (val_float.Count != getadr_f(rowlist[0])) {
                val_float.RemoveAt(getadr_f(rowlist[0]));
              }
              if (rowlist[1].IndexOf('_') > 0) {
                val_float.Add(new val_floats { Name = rowlist[0].Split('_')[1], Val = val_float[getadr_i(rowlist[1])].Val });
              } else {
                val_float.Add(new val_floats { Name = rowlist[0].Split('_')[1], Val = float.Parse(rowlist[1]) });
              }
            }
          }

          // CPU温度取得("i_temp,get,temp")
          if (rowlist.Length == 3) {
            if (rowlist[2].Equals("temp")) {
              if (rowlist[0].Split('_')[0].Equals("i")) {
                if (val_int.Count != getadr_i(rowlist[0])) {
                  val_int.RemoveAt(getadr_i(rowlist[0]));
                }
                val_int.Add(new val_ints { Name = rowlist[0].Split('_')[1], Val = int.Parse(getTemp()) });
              } else if (rowlist[0].Split('_')[0].Equals("f")) {
                if (val_float.Count != getadr_f(rowlist[0])) {
                  val_float.RemoveAt(getadr_f(rowlist[0]));
                }
                val_float.Add(new val_floats { Name = rowlist[0].Split('_')[1], Val = float.Parse(getTemp()) });
              }
            }
          }

          // GPIO取得orデータの取得
          if (rowlist.Length == 4) {

            // GPIOの値取得("i_gpio,get,gpio,3")
            if (rowlist[2].Equals("gpio")) {
              if (rowlist[0].Split('_')[0].Equals("i")) {
                if (val_int.Count != getadr_i(rowlist[0])) {
                  val_int.RemoveAt(getadr_i(rowlist[0]));
                }
                val_int.Add(new val_ints { Name = rowlist[0].Split('_')[1], Val = int.Parse(getGPIO(int.Parse(rowlist[3]))) });
              } else if (rowlist[0].Split('_')[0].Equals("f")) {
                if (val_float.Count != getadr_f(rowlist[0])) {
                  val_float.RemoveAt(getadr_f(rowlist[0]));
                }
                val_float.Add(new val_floats { Name = rowlist[0].Split('_')[1], Val = float.Parse(getGPIO(int.Parse(rowlist[3]))) });
              }
            }

            // データの取得 ("i_data1,get,nowdata,2")
            else if (rowlist[2].Equals("nowdata")) {
              if (rowlist[0].Split('_')[0].Equals("i")) {
                if (val_int.Count != getadr_i(rowlist[0])) {
                  val_int.RemoveAt(getadr_i(rowlist[0]));
                }
                val_int.Add(new val_ints { Name = rowlist[0].Split('_')[1], Val = int.Parse(dataadd.dat[int.Parse(rowlist[3]) - 1]) });
              }
              if (rowlist[0].Split('_')[0].Equals("f")) {
                if (val_float.Count != getadr_f(rowlist[0])) {
                  val_float.RemoveAt(getadr_f(rowlist[0]));
                }
                val_float.Add(new val_floats { Name = rowlist[0].Split('_')[1], Val = float.Parse(dataadd.dat[int.Parse(rowlist[3]) - 1]) });
              }
            }

          // 2項演算("i_result,i_x1,+,5")
            else {
              if (rowlist[0].Split('_')[0].Equals("i")) {
                calite = calite_i(rowlist);
                if (rowlist[2].Equals("+")) {
                  val_int[getadr_i(rowlist[0])].Val = (int)calite[0] + (int)calite[1];
                }
                if (rowlist[2].Equals("-")) {
                  val_int[getadr_i(rowlist[0])].Val = (int)calite[0] - (int)calite[1];
                }
                if (rowlist[2].Equals("*")) {
                  val_int[getadr_i(rowlist[0])].Val = (int)calite[0] * (int)calite[1];
                }
                if (rowlist[2].Equals("/")) {
                  val_int[getadr_i(rowlist[0])].Val = (int)calite[0] / (int)calite[1];
                }
                if (rowlist[2].Equals("%")) {
                  val_int[getadr_i(rowlist[0])].Val = (int)calite[0] % (int)calite[1];
                }
              }
              if (rowlist[0].Split('_')[0].Equals("f")) {
                calite = calite_f(rowlist);
                if (rowlist[2].Equals("+")) {
                  val_float[getadr_i(rowlist[0])].Val = calite[0] + calite[1];
                }
                if (rowlist[2].Equals("-")) {
                  val_float[getadr_i(rowlist[0])].Val = calite[0] - calite[1];
                }
                if (rowlist[2].Equals("*")) {
                  val_float[getadr_i(rowlist[0])].Val = calite[0] * calite[1];
                }
                if (rowlist[2].Equals("/")) {
                  val_float[getadr_i(rowlist[0])].Val = calite[0] / calite[1];
                }
                if (rowlist[2].Equals("%")) {
                  val_float[getadr_i(rowlist[0])].Val = calite[0] % calite[1];
                }
              }
            }

            row++;
          }

          // if文("if,i_x1,>,i_x2"～"elif,i_x1,>i_x3"～"else,3"～"endi,3")
        } else if (rowlist[0].Equals("if")) {

          Flag_clause = relation(rowlist);

          if (Flag_clause) {
            row_tmp = row;
            while (true) {
              row++;
              if (codelist[row].Split(',')[0].Equals("else") || codelist[row].Split(',')[0].Equals("endi")) {
                if (codelist[row].Split(',')[1].Equals(row_tmp.ToString())) {
                  row++;
                  break;
                }
              }
              if (codelist[row].Split(',')[0].Equals("elif")) {
                if (codelist[row].Split(',')[4].Equals(row_tmp.ToString())) {
                  bool Flag_elif = true;
                  while (Flag_elif) {
                    rowlist = codelist[row].Split(',');
                    Flag_clause = relation(rowlist);
                    if (Flag_clause) {
                      while (true) {
                        row++;
                        if (codelist[row].Split(',')[0].Equals("else") || codelist[row].Split(',')[0].Equals("endi")) {
                          if (codelist[row].Split(',')[1].Equals(row_tmp.ToString())) {
                            row++;
                            Flag_elif = false;
                            break;
                          }
                        }
                        if (codelist[row].Split(',')[0].Equals("elif")) {
                          if (codelist[row].Split(',')[4].Equals(row_tmp.ToString())) {
                            break;
                          }
                        }
                      }
                    } else {
                      row++;
                      break;
                    }
                  }
                  break;
                }
              }
            }
          } else {
            row++;
          }


          // while文
        } else if (rowlist[0].Equals("while")) {

          // 無限ループ("while,true"～"endw,2")
          if (rowlist[1].Equals("true")) {
            row++;
          }

          // 回数ループ("while,count,5"～"endw,2")
          if (rowlist[1].Equals("count")) {
            if (WhileCountStart[row]) {
              if (WhileCount[row] == (int.Parse(rowlist[2]) - 1)) {
                WhileCountStart[row] = false;
                row_tmp = row;
                while (true) {
                  row++;
                  if (codelist[row].Split(',')[0].Equals("endw")) {
                    if (codelist[row].Split(',')[1].Equals(row_tmp.ToString())) {
                      row++;
                      break;
                    }
                  }
                }
              } else {
                WhileCount[row]++;
                row++;
              }
            } else {
              WhileCountStart[row] = true;
              WhileCount[row] = 0;
              row++;
            }
          }

          // 条件ループ("while,i_x1,<,100"～"endw,2")
          if (rowlist.Length == 4) {
            Flag_clause = relation(rowlist);
            if (Flag_clause) {
              row_tmp = row;
              while (true) {
                row++;
                if (codelist[row].Split(',')[0].Equals("endw")) {
                  if (codelist[row].Split(',')[1].Equals(row_tmp.ToString())) {
                    row++;
                    break;
                  }
                }
              }
            } else {
              row++;
            }
          }


          // wait文("wait,1000")
        } else if (rowlist[0].Equals("wait")) {

          Thread.Sleep(int.Parse(rowlist[1]));
          row++;


          // add文(データの追加)
        }else if (rowlist[0].Equals("add")){

          if (dataadd.dat.Count < int.Parse(rowlist[1])) {
            int Count = int.Parse(rowlist[1]) - dataadd.dat.Count;
            for (int i = 0; i < Count; i++) {
              dataadd.dat.Add("");
              Debug.WriteLine("Count: " + dataadd.dat.Count);
            }
          }

            // 追加データが変数 ("add,4,i_data1")
          if (rowlist[2].IndexOf('_') > -1) {

            if (rowlist[2].Split('_')[0].Equals("i")) {
              dataadd.dat[int.Parse(rowlist[1]) - 1] = val_int[getadr_i(rowlist[1])].Val.ToString();
            }
            if (rowlist[2].Split('_')[0].Equals("f")) {
              dataadd.dat[int.Parse(rowlist[1]) - 1] = val_float[getadr_f(rowlist[1])].Val.ToString();
            }

            // 追加データが自由 ("add,5,56.7" "add,6,Worning!!")
          } else {

            dataadd.dat[int.Parse(rowlist[1]) - 1] = rowlist[2];

          }

          row++;

          // send文
        } else if (rowlist[0].Equals("send")) {

          // 変数 ("send,i_x1" or "send,hogehoge")
          if (rowlist.Length == 2) {
            if (rowlist[1].IndexOf('_') > 0) {
              if (rowlist[1].Split('_')[0].Equals("i")) {
                Debug.WriteLine("send: " + val_int[getadr_i(rowlist[1])].Val.ToString());
              }
              if (rowlist[1].Split('_')[0].Equals("f")) {
                Debug.WriteLine("send: " + val_float[getadr_f(rowlist[1])].Val.ToString());
              }
            } else {
              Debug.WriteLine("send: " + rowlist[1]);
            }

            // CPU温度 ("send,get,temp")
          } else if (rowlist.Length == 3) {
            Debug.WriteLine("send: " + getTemp());

            // GPIO値 ("send,get,gpio,1")
          } else if (rowlist.Length == 4) {
            Debug.WriteLine("send: " + getGPIO(int.Parse(rowlist[3])));
          }

          row++;
        
          // mail送信("mail,xxx@yyy.zzz,件名,本文")
        } else if (rowlist[0].Equals("mail")) {

            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage(
               "cats.marimocode.mail@gmail.com", rowlist[1],
               rowlist[2], rowlist[3]);
            System.Net.Mail.SmtpClient sc = new System.Net.Mail.SmtpClient();
            sc.Host = "smtp.gmail.com";
            sc.Port = 587;
            sc.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            sc.Credentials = new System.Net.NetworkCredential("cats.marimocode.mail@gmail.com", "MarimoCats");
            sc.EnableSsl = true;
            sc.Send(msg);
            msg.Dispose();
            sc.Dispose();
            row++;

          // break文("break")
        } else if (rowlist[0].Equals("break")) {

          while (true) {
            row++;
            if (codelist[row].Split(',')[0].Equals("endw")) {
              row++;
              break;
            }
          }


          // continue文("continue")
        } else if (rowlist[0].Equals("continue")) {

          while (true) {
            row++;
            if (codelist[row].Split(',')[0].Equals("endw")) {
              row = int.Parse(codelist[row].Split(',')[1]);
              break;
            }
          }


          // if文に入った場合のelse
        } else if (rowlist[0].Equals("else")) {

          while (true) {
            row++;
            if (codelist[row].Split(',')[0].Equals("endi")) {
              if (codelist[row].Split(',')[1].Equals(rowlist[1])) {
                row++;
                break;
              }
            }
          }


          // if文に入った場合のelif
        } else if (rowlist[0].Equals("elif")) {

          while (true) {
            row++;
            if (codelist[row].Split(',')[0].Equals("endi")) {
              if (codelist[row].Split(',')[1].Equals(rowlist[4])) {
                row++;
                break;
              }
            }
          }


          // while文に入った場合のendw
        } else if (rowlist[0].Equals("endw")) {

          row = int.Parse(rowlist[1]);


          // if文に入った場合のendi
        } else if (rowlist[0].Equals("endi")) {

          row++;


          // 例外はスルー
        } else {

          row++;

        }



      }
    
    }

    // GPIO値取得関数(Raspberry Pi用なので関係なし)
    static string getGPIO(int Pin) {

      //IO_No = Pin
      //GPIO.setmode(GPIO.BCM)
      //GPIO.setup(IO_No,GPIO.IN)
      //return GPIO.input(IO_No)

      return Pin.ToString();

    }

    // CPU温度取得関数(Raspberry Pi用なので関係なし)
    static string getTemp() {

      //temp = subprocess.check_output(['vcgencmd','measure_temp'])
      //temp = getNumDot(temp)
      //return temp

      return "45.8";

    }

    // 文字列の中から数値だけ取得関数
    static string getNumDot(string val) {

      char[] valArray = val.ToCharArray();
      string numdot = ".0123456789";
      string ret = string.Empty;

      foreach (var ch in valArray) {
        if (numdot.IndexOf(ch) > -1) {
          ret += ch;
        }
      }

      return ret;

    }

    // 名前が一致するint型変数を検索して番地を返す
    static int getadr_i(string rowlist) {
      int adr = 0;
      foreach (var val_ints in val_int) {
        if (val_ints.Name.Equals(rowlist.Split('_')[1])) {
          break;
        }
        adr++;
      }
      return adr;
    }

    // 名前が一致するfloat型変数を検索して番地を返す
    static int getadr_f(string rowlist) {
      int adr = 0;
      foreach (var val_floats in val_float) {
        if (val_floats.Name.Equals(rowlist.Split('_')[1])) {
          break;
        }
        adr++;
      }
      return adr;
    }

    // 演算or比較時、一時的にcalite1,calite2に格納する(int型)
    static float[] calite_i(string[] rowlist) {
      float[] calite = new float[2];
      Debug.WriteLine(rowlist[1]);
      Debug.WriteLine(rowlist[1].IndexOf('_'));
      if (rowlist[1].IndexOf('_') > 0) {
        calite[0] = val_int[getadr_i(rowlist[1])].Val;
      } else {
        calite[0] = int.Parse(rowlist[1]);
      }
      if (rowlist[3].IndexOf('_') > 0) {
        calite[1] = val_int[getadr_i(rowlist[3])].Val;
      } else {
        calite[1] = int.Parse(rowlist[3]);
      }

      return calite;
    }

    // 演算or比較時、一時的にcalite1,calite2に格納する(float型)
    static float[] calite_f(string[] rowlist) {
      float[] calite = new float[2];
      if (rowlist[1].IndexOf('_') > 0) {
        calite[0] = val_float[getadr_f(rowlist[1])].Val;
      } else {
        calite[0] = float.Parse(rowlist[1]);
      }
      if (rowlist[3].IndexOf('_') > 0) {
        calite[1] = val_float[getadr_f(rowlist[3])].Val;
      } else {
        calite[1] = float.Parse(rowlist[3]);
      }

      return calite;
    }

    // 比較演算値を返す関数
    static bool relation(string[] rowlist) {
      bool Flag_clause = false;
      float[] calite = new float[2];
      if (rowlist[1].Split('_')[0].Equals("i")) {
        calite = calite_i(rowlist);
      }
      if (rowlist[1].Split('_')[0].Equals("f")) {
        calite = calite_f(rowlist);
      }

      if (rowlist[2].Equals("<")) {
        if (!(calite[0] < calite[1])) {
          Flag_clause = true;
        }
      }
      if (rowlist[2].Equals(">")) {
        if (!(calite[0] > calite[1])) {
          Flag_clause = true;
        }
      }
      if (rowlist[2].Equals("<=")) {
        if (!(calite[0] <= calite[1])) {
          Flag_clause = true;
        }
      }
      if (rowlist[2].Equals(">=")) {
        if (!(calite[0] >= calite[1])) {
          Flag_clause = true;
        }
      }
      if (rowlist[2].Equals("==")) {
        if (!(calite[0] == calite[1])) {
          Flag_clause = true;
        }
      }
      if (rowlist[2].Equals("!=")) {
        if (!(calite[0] != calite[1])) {
          Flag_clause = true;
        }
      }

      return Flag_clause;
    }

  }
}