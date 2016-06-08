using ProjecToxfordApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ProjecToxfordApi.Controllers
{
    public class ProjecToxfordClientHelper
    {
        private const string serviceHost = "https://api.projectoxford.ai/face/v1.0";
        private const string KEY = "6b1043bade2646b6bd2b58c4cbe999bf";
        private HttpClient client;
       

        public ProjecToxfordClientHelper()
        {
            client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            client.DefaultRequestHeaders.Add("ContentType", "application/json");
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", KEY);
        }


        public async Task<ProjecToxfordResponseModels> PostAsync(string querkey, object body, Dictionary<string, string> querystr = null)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            if (querystr != null)
            {
                foreach (var entry in querystr)
                {
                    queryString[entry.Key] = entry.Value;
                }
            }
            var uri = string.Format("{0}/{1}?{2}", serviceHost, querkey, queryString);

            byte[] byteData = null;

            if (body.GetType() == typeof(byte[]))
            {
                byteData = (byte[])body;
            }
            else
            {
                var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(body);
                byteData = Encoding.UTF8.GetBytes(jsonStr);
            }

            HttpResponseMessage response;
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = body.GetType() == typeof(byte[]) ? 
                    new MediaTypeHeaderValue("application/octet-stream") : 
                    new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
                var msg = await response.Content.ReadAsStringAsync();
                return new ProjecToxfordResponseModels(msg, response.StatusCode);
            }
        }


        //public async Task<ProjecToxfordResponseModels> PostAsync(string querkey, object body, Dictionary<string, string> querystr = null)
        //{
        //    var queryString = HttpUtility.ParseQueryString(string.Empty);
        //    if (querystr != null)
        //    {
        //        foreach (var entry in querystr)
        //        {
        //            queryString[entry.Key] = entry.Value;
        //        }
        //    }
        //    var uri = string.Format("{0}/{1}?{2}", serviceHost, querkey, queryString);

        //    var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(body);
        //    byte[] byteData = Encoding.UTF8.GetBytes(jsonStr);

        //    HttpResponseMessage response;
        //    using (var content = new ByteArrayContent(byteData))
        //    {
        //        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        //        response = await client.PostAsync(uri, content);
        //        var msg = await response.Content.ReadAsStringAsync();
        //        return new ProjecToxfordResponseModels(msg, response.StatusCode);
        //    }
        //}

        //public async Task<ProjecToxfordResponseModels> PostAsync(string querkey, byte[] body, Dictionary<string, string> querystr = null)
        //{
        //    var queryString = HttpUtility.ParseQueryString(string.Empty);
        //    if (querystr != null)
        //    {
        //        foreach (var entry in querystr)
        //        {
        //            queryString[entry.Key] = entry.Value;
        //        }
        //    }

        //    var uri = string.Format("{0}/{1}?{2}", serviceHost, querkey, queryString);

        //    //var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(body);

        //    HttpResponseMessage response;
        //    using (var content = new ByteArrayContent(body))
        //    {
        //        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        //        response = await client.PostAsync(uri, content);
        //        var msg = await response.Content.ReadAsStringAsync();
        //        return new ProjecToxfordResponseModels(msg, response.StatusCode);
        //    }
        //}



        public HttpResponseMessage CreateHttpResponseMessage(HttpRequestMessage request, ProjecToxfordResponseModels result)
        {
            if (result.StatusCode == HttpStatusCode.OK)
            {

                return request.CreateResponse(HttpStatusCode.OK, result.Message);
            }
            else
            {
                return request.CreateErrorResponse(result.StatusCode, result.Message);
            }
        }



    }
}