using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Net;
namespace BLL
{
    public static class StaticInfo
    {
        //私钥(解密用)
        public static string privateKey = @"<RSAKeyValue><Modulus>vg8WWJFJrZjMn78FnTqliXxI7Zj43Mf4x8SOT5mC3sfUYnu2uThDk6i36Q+a46BHlVfzb06bsihXBBRwfxnA5Z2WCUasqatKdKqCJY9H7NBnUO3w15GO8+NmFDuCQ6D3poif/fG017wMrcril8aOAP5nBo8SRFWvhVLnldOvCoc=</Modulus><Exponent>AQAB</Exponent><P>3L17FFrnFfOIkiVoZvfMBfCYkS6c2r94e+QJEBpSxJn+UHaY8O1EXStUOwQgqrUcFsqrxPy+rY27TVx78xDjnw==</P><Q>3Gr89WQFV1YOOjbxgYwBRhzx+qFECHZZrpfiMI6rlh/tqGtFudKN8EgtisLR/1OdAdSVt4RBTkTmtwHIXcUwGQ==</Q><DP>h1tdbLbtOwWx+kQcB//tSLsnIuedYXnFrNrBP/GUTWBMlRSUZjBoGmWmaeX3Dhaumb8/ozSEzDG76A1NKFhz6w==</DP><DQ>PcowR4pWhPk229LzOOHKqaELpLr4m3ayBWPGoN4d8+PXd9M6pLEF4Uoamj+rJuyFozG5Fs0YkZx3IO57AO56YQ==</DQ><InverseQ>oCwHBm2ltOKd09sHAxKjyMSC12tT+4/hM+w0g2nalnR6vMHTH8BfGxoiPWIYDfXo1/Jvk2ieh2xz8GUX3R7m7A==</InverseQ><D>RmgkaQ75clvGge8rz0EojbQC+DHRD0jtOmPwLEC8IHd6kDkwSZE0R4EbEDV9tZFss0Bvp+5A81DKW3KO8ibCNCotBL7De3V6sLZY3gabZOmMsFctyvRvhHSSGjhDQMxylagmUEPjBO4hxBLoWNQl86qFxNXKBr4bfAFeEVodseE=</D></RSAKeyValue>";
        public static string key="110120"; 
        


        /// <summary>
        /// 32位MD5加密
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string MD5Encrypt32(string Phone,string password,string Timestamp)
        {
            string cl = Phone + password+Timestamp;
            string pwd = "";
            MD5 md5 = MD5.Create(); //实例化一个md5对像
                                    // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符 
                pwd = pwd + s[i].ToString("X2");
            }
            return pwd;
        }

        public static string MD5Encrypt32(string str)
        {
            string cl = str;
            string pwd = "";
            MD5 md5 = MD5.Create(); //实例化一个md5对像
                                    // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符 
                pwd = pwd + s[i].ToString("X2");
            }

            return pwd;
        }
        public static string ChangeBase64(string str)
        {
            if(str!=""&&str!=null)
            {
                
                byte[] b = Encoding.UTF8.GetBytes(str);
                string returnStr = Convert.ToBase64String(b,Base64FormattingOptions.None);
                return returnStr;
            }
            else
            {
                return "";
            }
        }
        public static string ChangeBase64(MemoryStream ms)
        {
            try
            {
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                String strbaser64 = Convert.ToBase64String(arr);
                return strbaser64;
            }
            catch (Exception ex)
            {
                //LogHelper.Debug(ex);
                return "";
            }
        }
        /// <summary>
        /// Base64解码
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string UnBase64String(string value)
        {
            if (value == null || value == "")
            {
                return "";
            }
            byte[] bytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(bytes);
        }

        #region MD5签名
        /// <summary>
        /// 创建MD5签名 字典按ASCII码升序排序后 拼接成url params格式进行MD5加密
        /// </summary>
        /// <param name="dic">要签名参数字典</param>
        /// <returns>md5加密后的字符串</returns>
        public static string CreateSign(Dictionary<string, string> dic)
        {
            var stringA = "";
            var resultDic = from obj in dic orderby obj.Key  select obj;
            foreach(var keyValue in resultDic)
            {
                if (string.IsNullOrEmpty(keyValue.Value.Trim())) continue;
                stringA += keyValue.Key.Trim() + "=" + keyValue.Value.Replace("\r\n","").Replace(" ","").Replace("\\","").Trim()+"&";
            }
            var stringSignTemp = stringA + "key=" + key;
            var sign = StaticInfo.MD5Encrypt32(stringSignTemp).ToUpper();
            return sign;
        }
        #endregion
     

        // 时间戳转为C#格式时间
        public static DateTime StampToDateTime(string timeStamp)
        {
            DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);

            return dateTimeStart.Add(toNow);
        }

        // DateTime时间格式转换为Unix时间戳格式
        public static int DateTimeToStamp(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }

        /// <summary>  
        /// 将c# DateTime时间格式转换为Unix时间戳格式  
        /// </summary>  
        /// <param name="time">时间</param>  
        /// <returns>long</returns>  
        public static long ConvertDateTimeToInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return t;
        }
    
        #region  Post方式提交请求
		/// <summary>
		/// Post方式提交请求
		/// </summary>
		/// <param name="Url">地址</param>
		/// <param name="postDataStr">form中的参数字符串</param>
		/// <returns></returns>
		public static string CreatePostHttpResponse(string url,string postStr,int tradeChannel)
		{
			try
			{
                StaticInfo.Log("url:"+url.ToString());            
                StaticInfo.Log("postData:"+postStr.ToString());
				// 编辑并Encoding提交的数据  
				byte[] data = new UTF8Encoding().GetBytes(postStr); //Encoding.UTF8.GetBytes(postStr);// new UTF8Encoding().GetBytes(postDataStr);//

				// 发送请求  
				System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
				request.Method = "POST";
				request.Timeout = 5000;
                if(tradeChannel==2){
                    request.ContentType="application/json;charset=utf-8";
                }else {
				request.ContentType = "application/xml;charset=utf-8";
                }
				request.ContentLength = data.Length;

				using (var stream = request.GetRequestStream())
				{ 
					stream.Write(data, 0, data.Length);
				}

				// 获得回复  
				var response = (HttpWebResponse)request.GetResponse();
				var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                StaticInfo.Log("responseData:"+responseString.ToString());
				return responseString;
			}

			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
                StaticInfo.Log("httppost请求发生异常："+ex.ToString());
				return "";
			}


		}
		#endregion



        public static void Log(string str)
        {
            string applacationSrc = System.Environment.CurrentDirectory;
            string logPath="log/";
            try{
                //生成目录
                if (!Directory.Exists(applacationSrc + "/log"))
                {
                    Directory.CreateDirectory(applacationSrc + "/log");
                }
                //读取当前目录下日否有当前日期的txt
                if(!System.IO.File.Exists(logPath+DateTime.Now.ToString("yyyy-MM-dd")+".txt"))
                {
                    FileStream fs=new FileStream(logPath+DateTime.Now.ToString("yyyy-MM-dd")+".txt",FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+":" +str);
                    sw.Close();
                    fs.Close();
                }
                else
                {
                    FileStream fs=new FileStream(logPath+DateTime.Now.ToString("yyyy-MM-dd")+".txt",FileMode.Append, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+":" +str);
                    sw.Close();
                    fs.Close();
                }
            }
            catch(Exception ex)
            {

            }
        }
        #region  将stream转成string
        public static string StreamToString(MemoryStream stream){
            StreamReader reader = new StreamReader(stream);
            string text = reader.ReadToEnd();
            return text;
        }
        #endregion


      
        public static String StringToJson(String s)
       {
           StringBuilder sb = new StringBuilder();
           for (int i = 0; i < s.Length; i++)
           {
               char c = s[i];
               switch (c)
               {
                   case '\"':
                       sb.Append("\\\"");
                       break;
                   case '\\':
                       sb.Append("\\\\");
                       break;
                   case '/':
                       sb.Append("\\/");
                       break;
                   case '\b':
                       sb.Append("\\b");
                       break;
                   case '\f':
                       sb.Append("\\f");
                       break;
                   case '\n':
                       sb.Append("\\n");
                       break;
                   case '\r':
                       sb.Append("\\r");
                       break;
                   case '\t':
                       sb.Append("\\t");
                       break;
                   default:
                       sb.Append(c);
                       break;
               }
           }
           return sb.ToString();
       }

    }
    
    
   
}
