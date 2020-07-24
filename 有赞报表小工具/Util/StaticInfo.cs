using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Newtonsoft.Json.Linq;
using System.IO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Web;
using System.Net;
using QRCoder;
using System.Drawing;
using System.Collections;
using System.Reflection;
using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Bson;
using MySql.Data.MySqlClient;
namespace BLL
{
    public static class StaticInfo
    {
        //私钥(解密用)
        public static string privateKey = @"<RSAKeyValue><Modulus>vg8WWJFJrZjMn78FnTqliXxI7Zj43Mf4x8SOT5mC3sfUYnu2uThDk6i36Q+a46BHlVfzb06bsihXBBRwfxnA5Z2WCUasqatKdKqCJY9H7NBnUO3w15GO8+NmFDuCQ6D3poif/fG017wMrcril8aOAP5nBo8SRFWvhVLnldOvCoc=</Modulus><Exponent>AQAB</Exponent><P>3L17FFrnFfOIkiVoZvfMBfCYkS6c2r94e+QJEBpSxJn+UHaY8O1EXStUOwQgqrUcFsqrxPy+rY27TVx78xDjnw==</P><Q>3Gr89WQFV1YOOjbxgYwBRhzx+qFECHZZrpfiMI6rlh/tqGtFudKN8EgtisLR/1OdAdSVt4RBTkTmtwHIXcUwGQ==</Q><DP>h1tdbLbtOwWx+kQcB//tSLsnIuedYXnFrNrBP/GUTWBMlRSUZjBoGmWmaeX3Dhaumb8/ozSEzDG76A1NKFhz6w==</DP><DQ>PcowR4pWhPk229LzOOHKqaELpLr4m3ayBWPGoN4d8+PXd9M6pLEF4Uoamj+rJuyFozG5Fs0YkZx3IO57AO56YQ==</DQ><InverseQ>oCwHBm2ltOKd09sHAxKjyMSC12tT+4/hM+w0g2nalnR6vMHTH8BfGxoiPWIYDfXo1/Jvk2ieh2xz8GUX3R7m7A==</InverseQ><D>RmgkaQ75clvGge8rz0EojbQC+DHRD0jtOmPwLEC8IHd6kDkwSZE0R4EbEDV9tZFss0Bvp+5A81DKW3KO8ibCNCotBL7De3V6sLZY3gabZOmMsFctyvRvhHSSGjhDQMxylagmUEPjBO4hxBLoWNQl86qFxNXKBr4bfAFeEVodseE=</D></RSAKeyValue>";
        public static string key="110120"; 
        /// <summary>
        /// 创建一个Token
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public static ReturnClass CreatToken(string jsonStr)
        {
            
            ReturnClass rc = new ReturnClass();
            try
            {
                JObject jObj = new JObject();
                jObj = JObject.Parse(jsonStr);
                if (string.IsNullOrEmpty(jObj["UserName"].ToString()) || string.IsNullOrEmpty(jObj["Password"].ToString()))
                {
                    rc.Msg = "参数为空";
                    rc.Code = Code.ERR_Sign;
                    rc.Data = "";
                    return rc;
                }
                string userName = jObj["UserName"].ToString();//用户名
                string password =StaticInfo.MD5Encrypt32( jObj["Password"].ToString());//密码
               
                //去数据库查询是否有合法
                MySqlParameter[] sp=new MySqlParameter[2];
                sp[0]=new MySqlParameter("@account",userName);
                sp[1]=new MySqlParameter("@password",password);
                var sqlselect1=string.Format("select * from ws_system_admin where account=@account and pwd=@password");
                //DataRow result=MySqlHelper.GetDataSet(MySqlHelper.Conn,CommandType.Text,sqlselect1,sp).Tables[0].Rows[0];
                //DataTable dtneed=MySqlHelper.GetDataSet(MySqlHelper.Conn,CommandType.Text,sqlselect1,sp).Tables[0];
                DataTable dataTable=MySqlHelper.GetDataSet(MySqlHelper.Conn,CommandType.Text,sqlselect1,sp).Tables[0];
                if(dataTable.Rows.Count<1){
                    rc.Msg = "用户名或密码错误";
                    rc.Code = Code.ERR_Sign;
                    rc.Data = "";
                    return rc;
                }
                DataRow result = dataTable.Rows[0];//MySqlHelper.GetDataSet(MySqlHelper.Conn,CommandType.Text,sqlselect1,sp).Tables[0].Rows[0];  //SqlHelper.ExecuteDataRow( System.Data.CommandType.Text, "select * from [Base_Users] where UserName='" + userName + "' and Password='" + password + "'");

                if (result == null)//说明不存在
                {
                    rc.Msg = "用户不存在";
                    rc.Code = Code.ERR_Sign;
                    rc.Data = "";
                    return rc;
                }

                // //只要是一登陆先清除token
                // MemoryCachingHelper._cache.Remove(result["UserID"].ToString());
                //  //先判断下缓存中是否存在  这个地方必须拿token去获取

                // if(MemoryCachingHelper.Exists(result["UserID"].ToString()))
                // {
                //     rc.Msg = "成功!";
                //     rc.Code = Code.SUCCED;
                //     rc.Data = (Token)MemoryCachingHelper.Get(result["UserID"].ToString());
                // }
                // else//不存在才会去生成Token
                
                //登陆时先删除
                var redisTokenFlag=result["id"].ToString()+result["account"].ToString();
                if(RedisStaticHelper.Exists(redisTokenFlag))
                {
                    //先删除
                    var jwtTokenStr= RedisStaticHelper.Get(redisTokenFlag);
                    RedisStaticHelper.Del(jwtTokenStr);
                    RedisStaticHelper.Del(redisTokenFlag);
                }
                {
                    //生成JWT
                   
                    //生成token
                    Token tk = new Token();
                    tk.userName = result["account"].ToString();
                    tk.userID = result["id"].ToString();
                    tk.sub="Client";
                    //距离上次登录的毫秒数
                    tk.Timestamp = Convert.ToString(DateTimeToStamp(DateTime.Now)); //DateTime.Now.ToString("yyyyMMddHHmmss");
                    //token生成规则 用户名 密码 时间戳 MD5加密
                    //tk.AccessToken = MD5Encrypt32(EmpID, password, tk.Timestamp);
                    //存一下token

                      DateTime UTC = DateTime.UtcNow;
                        Claim[] claims = new Claim[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub,tk.sub),//Subject,
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),//JWT ID,JWT的唯一标识
                            new Claim(JwtRegisteredClaimNames.Iat, UTC.ToString(), ClaimValueTypes.Integer64),//Issued At，JWT颁发的时间，采用标准unix时间，用于验证过期
                        };

                        JwtSecurityToken jwt = new JwtSecurityToken(
            issuer: "TianTao",//jwt签发者,非必须
            audience: tk.userName,//jwt的接收该方，非必须
            claims: claims,//声明集合
            expires: UTC.AddHours(12),//指定token的生命周期，unix时间戳格式,非必须
            signingCredentials: new Microsoft.IdentityModel.Tokens
                .SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes("RayPI's Secret Key")), SecurityAlgorithms.HmacSha256));//使用私钥进行签名加密

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);//生成最后的JWT字符串
            tk.AccessToken=encodedJwt;
                    // int count = SqlHelper.ExecuteNonQuery(System.Data.CommandType.Text, "update [Emp] set Token='" + tk.AccessToken + "' where EmpID='" + EmpID + "' and Pwd='" + password + "'");
                    // if (count < 1)
                    // {
                    //     rc.Msg = "失败，重试";
                    //     rc.Code = Code.SystemError;
                    //     rc.Data = "";
                    //     return rc;
                    // }
                    rc.Msg = "成功!";
                    rc.Code = Code.SUCCED;
                    rc.Data = tk;
                    //将token 存入缓存                    
                    //MemoryCachingHelper.addMemoryCache(tk.AccessToken,tk,new TimeSpan(0,10,0),new TimeSpan(0,10,0));
                    RedisStaticHelper.Set(tk.AccessToken,tk.ToJson());
                    RedisStaticHelper.Set(tk.userID+tk.userName,tk.AccessToken);
                }
                return rc;
            }
            catch(Exception ex)
            {
                StaticInfo.Log(ex.ToString());
                rc.Msg = "违反了中央八项纪律";
                rc.Code = Code.SystemError;
                rc.Data = "";
                return rc;
            }
        }


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
        public static bool CheckToken(string token)
        {

            //DataRow dr = DB.SqlHelper.ExecuteDataRow(CommandType.Text, "select * from Admin where Token='" + token + "'");
            //if (dr != null)
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
            return true;
        }

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

        public static readonly string keys = "lalana";

        //验证签名算法  参数字符串+secret+时间戳   MD5加密
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="sign"></param>
        /// <param name="timeStamp"></param>
        /// <param name="pInput"></param>
        /// <returns></returns>
        // public static bool CheckSign(string key,string pInput)
        // {
           
        //     bool result = false;
        //     try
        //     {
        //         //在请求正文添加timespan（时间戳），nonce（随机数），sign（签名参数）
        //         //"{'appId':'1','phone':'13969800321','pwd':'123456','timespan':'201802932828','nonce':'288','sign':'noce288phone13969800321pwd123456timespan201802932828'}" sign用MD5加密

        //         //入参形式应该为
        //         //{'Phone':'13969800321','Pwd':'123456',TimeStamp':'20180419029388','DesCode':'abc','Sign':'Phone13969800321Pwd123456TimeStamp20180419029388'}
        //         //sign MD5加密  DesCode为RSA公钥加密
        //         //入参先用DES解密
        //         RSACryption rsaInput = new RSACryption();
        //         DESEncrypt.Key = rsaInput.RSADecrypt(privateKey, key);//运用私钥解密传来的公钥加密过的DES秘钥
        //         string inputStr = DESEncrypt.DesDecrypt(pInput);
        //         //将入参转为JSON对象
        //         JObject jobj = JObject.Parse(inputStr);
        //         //然后遍历Json对象所有Key Value 
        //         string strParam = "";
        //         List<string> listParam = new List<string>();
        //         string sign = "";
        //         foreach (var j in jobj)
        //         {
        //             if (j.Key == "sign")
        //             {
        //                 sign = j.Value.ToString();
        //                 continue;
        //             }
        //             string strKeyValue = j.Key + j.Value;
        //             listParam.Add(strKeyValue);
        //         }
        //         listParam.OrderBy(item => item);
        //         foreach(string str in listParam)
        //         {
        //             strParam += str;
        //         }
        //         //完成排序组合，然后MD5加密                
        //         //然后MD5加密
        //         string md5ParamStr = MD5Encrypt32(strParam);
        //         if(sign.Trim()==md5ParamStr.Trim())
        //         {
        //             result = true;
        //         }
        //         else
        //         {
        //             result = false;
        //         }
        //     }
        //     catch(Exception ex)
        //     {
        //         result = false;
        //     }
        //     return result;
        // }


        // //PEM格式密钥转XML
        // /// <summary>      
        // /// RSA私钥格式转换，java->.net      
        // /// </summary>      
        // /// <param name="privateKey">java生成的RSA私钥</param>      
        // /// <returns></returns>     
        // public static string RSAPrivateKeyJava2DotNet(this string privateKey)
        // {
        //     RsaPrivateCrtKeyParameters privateKeyParam = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));
        //     return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
        //     Convert.ToBase64String(privateKeyParam.Modulus.ToByteArrayUnsigned()),
        //     Convert.ToBase64String(privateKeyParam.PublicExponent.ToByteArrayUnsigned()),
        //     Convert.ToBase64String(privateKeyParam.P.ToByteArrayUnsigned()),
        //     Convert.ToBase64String(privateKeyParam.Q.ToByteArrayUnsigned()),
        //     Convert.ToBase64String(privateKeyParam.DP.ToByteArrayUnsigned()),
        //     Convert.ToBase64String(privateKeyParam.DQ.ToByteArrayUnsigned()),
        //     Convert.ToBase64String(privateKeyParam.QInv.ToByteArrayUnsigned()),
        //     Convert.ToBase64String(privateKeyParam.Exponent.ToByteArrayUnsigned()));
        // }
        // public static string RSAPublicKeyJava2DotNet(this string publicKey)
        // {
        //     RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
        //     return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
        //         Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
        //         Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned()));
        // }
        // /// <summary>
        // /// 
        // /// </summary>
        // /// <param name="parames"></param>
        // /// <returns></returns>
        // public static Tuple<string,string> GetQueryString(Dictionary<string,string> parames)
        // {
        //     StringBuilder query = new StringBuilder("");//签名字符串
        //     StringBuilder queryStr = new StringBuilder("");//url参数
        //     try
        //     {
        //         //将字典按Key首字母排序
        //         IDictionary<string, string> sortedParams = new SortedDictionary<string, string>(parames);
        //         IEnumerator<KeyValuePair<string, string>> dem = sortedParams.GetEnumerator();
        //         //将所有签名字符串按照KeyValue形式串起来

        //         if (parames == null || parames.Count < 1)
        //         {
        //             return new Tuple<string, string>("","");
        //         }
        //         while (dem.MoveNext())
        //         {
        //             string key = dem.Current.Key;
        //             string value = dem.Current.Value;
        //             if (string.IsNullOrEmpty(key)&&key!="sign")
        //             {
        //                 query.Append(key).Append(value);
        //                 queryStr.Append("&").Append(key).Append("=").Append(value);
        //             }
        //         }
        //     }
        //     catch (Exception)
        //     {
        //         return new Tuple<string, string>("", "");
        //     }
        //     return new Tuple<string, string>(query.ToString(), queryStr.ToString().Substring(1, queryStr.Length - 1));
        // }
    
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

        #region 生成bace64二维码
        public static string CreateQRCode(string url,int pixel)
        {
            var imgType = Base64QRCode.ImageType.Jpeg;
            
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            Base64QRCode qrCode = new Base64QRCode(qrCodeData);
            string qrCodeImageAsBase64 = qrCode.GetGraphic(pixel, Color.Black, Color.White, true, imgType);
 
            return qrCodeImageAsBase64;

            
        }
        #endregion

        public static void Log(string str)
        {
            string logPath="c:/netCoreLog/";
            try{
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

        public static void MangoDb_handle_Log(string source,string interfaceRouter,string inputData,string outputData,int time)
        {
            try{
                MongoDbHelper md =new MongoDbHelper(MongoDbHelper.getConStr(),"local");//"mongodb://localhost:27017"
                BsonDocument BDoc=new BsonDocument(){
                    {"source",source},
                    {"interfaceRouter",interfaceRouter},
                    {"inputData",inputData},
                    {"outputData",outputData},
                    {"addTime",DateTime.Now.AddHours(8)},
                    {"time",time}
                };
                md.Insert("webapi_handle_log",BDoc);
            }
            catch(Exception ex)
            {

            }
        }
        /// <summary>
        /// 北京时间转换为mongodb时间
        /// </summary>
        /// <param name="dt">时间</param>
        /// <returns>mongodb时间</returns>
        public static DateTime GetMongoDate(DateTime dt)
        {
            return dt.AddHours(8);
        }

        /// <summary>
        /// 取得sql where 后字符串
        /// </summary>
        /// <param name="dicParams"></param>
        /// <returns></returns>
        public static string GetQueryWhere(Dictionary<string,string> dicParams)
        {
            string sqlWhere = " where 1=1 ";
            if(dicParams==null) return sqlWhere;
            foreach(KeyValuePair<string,string> kp in dicParams)
            {
                if(!string.IsNullOrEmpty(kp.Value))
                {
                    sqlWhere += " and "+ kp.Key + "='" + kp.Value+"' ";
                }
            }
            return sqlWhere;
        } 

        /// <summary>
        /// 取得sql where 后字符串
        /// </summary>
        /// <param name="dicParams"></param>
        /// <returns></returns>
        public static string GetQueryWhere(Dictionary<KeyValuePair<string,string>,string> dicParams)
        {
            string sqlWhere = " where 1=1 ";
            if(dicParams==null) return sqlWhere;
            foreach(KeyValuePair<KeyValuePair<string,string>,string> kps in dicParams)
            {
     
                if(!string.IsNullOrEmpty(kps.Key.Key)){
                    sqlWhere += " and "+ kps.Key.Key +  kps.Value +"'" + kps.Key.Value+"' ";
                }
            }
            return sqlWhere;
        } 
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
       /// <summary>  
        /// 生成简易验证码(low)
        /// </summary>  
        /// <param name="code">4位字符串</param>  
        /// <param name="width">图片宽度</param>  
        /// <param name="height">图片高度</param>  
        /// <param name="type">类型</param>  
        /// <returns>返回一个随机数字符串</returns>  
       public static string CreateVerificationCode(string code,int width=100,int height=40,int type=0){
           var resultStr="";
           try
           {
                Bitmap bitmap = new Bitmap(width, height);
                Graphics g=Graphics.FromImage(bitmap);
                g.Clear(Color.Green);
                int w=0;
                Random rdm = new Random(GetRandomSeedbyGuid());  
                foreach(char c in code){
                    Font font=GetRandomFont();
                    Brush brush=GetRandomBrush();
                    SizeF sf=g.MeasureString(c.ToString(),font);
                    System.Drawing.Drawing2D.Matrix mtxSave = g.Transform;
                    System.Drawing.Drawing2D.Matrix mtxRotate = g.Transform;
                    mtxRotate.RotateAt(rdm.Next(-45,45), new Point((int)sf.Width/2+w,(int)sf.Height/2));
                    g.Transform = mtxRotate;
                    g.DrawString(c.ToString(),font,brush, w, 0);
                    g.Transform = mtxSave;
                    w+=(int)sf.Width;
                }                
                //画图片的前景噪音点
                for (int i = 0; i < 100; i++)
                {
                    int x = rdm.Next(width);
                    int y = rdm.Next(height);
                    bitmap.SetPixel(x, y, Color.White);
                }
                //加直线
                for (int i = 0; i < 12; i++)
                {
                    int x1 = rdm.Next(width);
                    int x2 = rdm.Next(width);
                    int y1 = rdm.Next(height);
                    int y2 = rdm.Next(height);

                    g.DrawLine(new Pen(Color.Black), x1, y1, x2, y2);
                }
                MemoryStream ms = new MemoryStream();  
                bitmap.Save(ms,System.Drawing.Imaging.ImageFormat.Png);
                resultStr=ChangeBase64(ms);
                g.Dispose();
                bitmap.Dispose();
           }
           catch(Exception ex){
               resultStr="";
           }
           return resultStr;
       }

    

       #region 随机生成字体和画笔颜色
    //    public static Bitmap GetCharBitmap(char c,int width,int height){
    //        //旋转角度

    //    }
       //public static char[] codes={'A','B','C','D','E','F','G','H','I','G','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z','1','2','3','4','5','6','7','8','9','0'};
       public static Font verificationCodeFont=null;
       public static Font GetRandomFont(){
           Random rdm = new Random(GetRandomSeedbyGuid());
           int i=rdm.Next(0,2);
           switch(i){
               case 0:verificationCodeFont=new Font("宋体",16,FontStyle.Bold);
               break;
               case 1:verificationCodeFont=new Font("黑体",20,FontStyle.Underline);
               break;
               case 2:verificationCodeFont=new Font("微软雅黑",24,FontStyle.Italic);
               break;
           }
           return verificationCodeFont;
       }
       public static  Brush verificationCodeBrush =null;
       public static Brush GetRandomBrush(){
           Random rdm = new Random(GetRandomSeedbyGuid());
           int i=rdm.Next(0,2);
           switch(i){
               case 0:verificationCodeBrush=new SolidBrush(Color.Red);
               break;
               case 1:verificationCodeBrush=new SolidBrush(Color.Blue);
               break;
               case 2:verificationCodeBrush=new SolidBrush(Color.Purple);
               break;
           }
           return verificationCodeBrush;
       }
        /// <summary>  
        /// 生成指定位数的随机数  
        /// </summary>  
        /// <param name="VcodeNum">参数是随机数的位数</param>  
        /// <returns>返回一个随机数字符串</returns>  
        public static string RndNum(int VcodeNum)
        {
            //验证码可以显示的字符集合  
            string Vchar = "0,1,2,3,4,5,6,7,8,9,a,b,c,d,e,f,g,h,i,j,k,l,m,n,p" +
                ",q,r,s,t,u,v,w,x,y,z,A,B,C,D,E,F,G,H,I,J,K,L,M,N,P,P,Q" +
                ",R,S,T,U,V,W,X,Y,Z";
            string[] VcArray = Vchar.Split(new Char[] { ',' });//拆分成数组   
            string code = "";//产生的随机数  
            int temp = -1;//记录上次随机数值，尽量避避免生产几个一样的随机数  

            Random rand = new Random();
            //采用一个简单的算法以保证生成随机数的不同  
            for (int i = 1; i < VcodeNum + 1; i++)
            {
                if (temp != -1)
                {
                    rand = new Random(i * temp * unchecked((int)DateTime.Now.Ticks));//初始化随机类  
                }
                int t = rand.Next(61);//获取随机数  
                if (temp != -1 && temp == t)
                {
                    return RndNum(VcodeNum);//如果获取的随机数重复，则递归调用  
                }
                temp = t;//把本次产生的随机数记录起来  
                code += VcArray[t];//随机数的位数加一  
            }
            return code;
        }
        /// <summary>
        /// 使用Guid生成种子
        /// </summary>
        /// <returns></returns>
        static int GetRandomSeedbyGuid()
        {
            return Guid.NewGuid().GetHashCode();
        }
       #endregion

    }
    
    
   
}
