using ProjecToxfordApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Security;

namespace ProjecToxfordApi.Controllers
{
    public class WeixinController : ApiController
    {
        private const string TOKEN = "weixin";

        private const string APPID = "";
        private const string APPSECRET = "";

        private HttpClient client;

        public WeixinController()
        {
            client = new HttpClient();
        }

        public async Task<HttpResponseMessage> Get(string noncestr, int timestamp, string url)
        {
            //获取access_token（有效期7200秒
            if (HttpRuntime.Cache["access_token"] == null)
            {
                //https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=APPID&secret=APPSECRET

                var queryString = HttpUtility.ParseQueryString(string.Empty);
                queryString["grant_type"] = "client_credential";
                queryString["appid"] = APPID;
                queryString["secret"] = APPSECRET;

                var uri = "https://api.weixin.qq.com/cgi-bin/token?" + queryString;

                HttpResponseMessage response;
                response = await client.GetAsync(uri);
                var msg = await response.Content.ReadAsStringAsync();
                var jsonobj = Newtonsoft.Json.Linq.JObject.Parse(msg);

                HttpRuntime.Cache.Add("access_token",
                    (string)jsonobj["access_token"],
                    null,
                    DateTime.Now.AddMinutes((int)jsonobj["expires_in"]),
                    new TimeSpan(0, 0, 0),
                    System.Web.Caching.CacheItemPriority.AboveNormal,
                    null
                    );

                var access_token = (string)jsonobj["access_token"];
                var expires_in = (int)jsonobj["expires_in"];

            }

            //获得jsapi_ticket（有效期7200秒
            if (HttpRuntime.Cache["jsapi_ticket"] == null)
            {
                //https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token=ACCESS_TOKEN&type=jsapi
                var queryString = HttpUtility.ParseQueryString(string.Empty);
                queryString["access_token"] = (string)HttpRuntime.Cache["access_token"];
                queryString["type"] = "jsapi";

                var uri = "https://api.weixin.qq.com/cgi-bin/ticket/getticket?" + queryString;

                HttpResponseMessage response;
                response = await client.GetAsync(uri);
                var msg = await response.Content.ReadAsStringAsync();
                var jsonobj = Newtonsoft.Json.Linq.JObject.Parse(msg);

                HttpRuntime.Cache.Add("jsapi_ticket",
                                    (string)jsonobj["ticket"],
                                    null,
                                    DateTime.Now.AddMinutes((int)jsonobj["expires_in"]),
                                    new TimeSpan(0, 0, 0),
                                    System.Web.Caching.CacheItemPriority.AboveNormal,
                                    null
                                    );

                var ticket = (string)jsonobj["ticket"];
                var expires_in = (int)jsonobj["expires_in"];
            }


            //签名生成

            var pwd = string.Format("jsapi_ticket={0}&noncestr={1}&timestamp={2}&url={3}",
                (string)HttpRuntime.Cache["jsapi_ticket"],
                noncestr,
                timestamp,
                url
                );

            var tmpStr = FormsAuthentication.HashPasswordForStoringInConfigFile(pwd, "SHA1");

            return Request.CreateResponse(HttpStatusCode.OK, tmpStr);
        }


        /// <summary>
        /// 第一次注册验收使用
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="timestamp"></param>
        /// <param name="nonce"></param>
        /// <param name="echostr"></param>
        public HttpResponseMessage Get(string signature, string timestamp, string nonce, string echostr)
        {
            string[] ArrTmp = { TOKEN, timestamp, nonce };
            Array.Sort(ArrTmp);
            string tmpStr = string.Join("", ArrTmp);
            var result = FormsAuthentication.HashPasswordForStoringInConfigFile(tmpStr, "SHA1").ToLower();

            return new HttpResponseMessage()
            {
                Content = new StringContent(result, Encoding.GetEncoding("UTF-8"), "application/x-www-form-urlencoded")
            };
        }

        private async Task<string> Get()
        {
            if (HttpRuntime.Cache["access_token"] == null)
            {
                //https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=APPID&secret=APPSECRET

                var queryString = HttpUtility.ParseQueryString(string.Empty);
                queryString["grant_type"] = "client_credential";
                queryString["appid"] = APPID;
                queryString["secret"] = APPSECRET;

                var uri = "https://api.weixin.qq.com/cgi-bin/token?" + queryString;

                HttpResponseMessage response;
                response = await client.GetAsync(uri);
                var msg = await response.Content.ReadAsStringAsync();
                var jsonobj = Newtonsoft.Json.Linq.JObject.Parse(msg);

                HttpRuntime.Cache.Add("access_token",
                    (string)jsonobj["access_token"],
                    null,
                    DateTime.Now.AddMinutes((int)jsonobj["expires_in"]), new TimeSpan(0, 0, 0), System.Web.Caching.CacheItemPriority.AboveNormal, null
                    );

                var access_token = (string)jsonobj["access_token"];
                var expires_in = (int)jsonobj["expires_in"];
            }

            return (string)HttpRuntime.Cache["access_token"];
        }


        /// <summary>
        /// 通过ServerID（mediaid）从微信服务器上下载图片，保存到本地，并返回文件名
        /// </summary>
        /// <param name="mediaid"></param>
        /// <returns></returns>
        public async Task<string> Get(string mediaid)
        {
            var mongo = new MongoDBHelper<WeixinImgFileModels>("weixinImgFile");

            //查询mongo中是否存储了mediaid对应的照片文件
            var doc = await mongo.SelectOneAsync(x => x.MediaId == mediaid);
            if (doc != null)
            {
                return doc.FileName;
            }

            //如果文件没有下载过，则下载
            //http://file.api.weixin.qq.com/cgi-bin/media/get?access_token=ACCESS_TOKEN&media_id=MEDIA_ID
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["access_token"] = await Get();
            queryString["media_id"] = mediaid;

            var uri = "http://file.api.weixin.qq.com/cgi-bin/media/get?" + queryString;

            HttpResponseMessage response;
            response = await client.GetAsync(uri);

            var msg = await response.Content.ReadAsStreamAsync();
            var fileName = response.Content.Headers.ContentDisposition.FileName.Replace("\"", "");

            var helper = new ProjecToxfordClientHelper();

            var content = await FileHelper.ReadAsync(msg);

            FileHelper.SaveFile(content, fileName);

            await mongo.InsertAsync(new WeixinImgFileModels()
            {
                FileName = fileName,
                MediaId = mediaid
            });

            return fileName;
        }


    }
}
