using MongoDB.Bson;
using Newtonsoft.Json;
using ProjecToxfordApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ProjecToxfordApi.Controllers
{
    public class FaceController : ApiController
    {
        private static string uploadedImageFiled = System.Configuration.ConfigurationManager.AppSettings["UploadedImage"];

        ProjecToxfordClientHelper client;

        public FaceController()
        {
            client = new ProjecToxfordClientHelper();
        }

        /// <summary>
        /// 提供标准Web进行POST图片的方式
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("face/detect")]
        public async Task<HttpResponseMessage> Detect()
        {
            var key = "detect";

            var httpPostedFile = HttpContext.Current.Request.Files[uploadedImageFiled];
            if (httpPostedFile != null)
            {
                var content = await FileHelper.ReadAsync(httpPostedFile.InputStream);

                var result = await client.PostAsync(key,
                    content,
                    new Dictionary<string, string> {
                    {"returnFaceId","true"},
                    {"returnFaceLandmarks","flase"},
                    }
                    );
                FileHelper.SaveFile(content, httpPostedFile.FileName);

                return client.CreateHttpResponseMessage(Request, result);
            }
            throw new HttpResponseException(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// 提供客户端显示要求对两个FaceID进行比较
        /// </summary>
        /// <param name="faceId1"></param>
        /// <param name="faceId2"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("face/verify/{faceId1}/{faceId2}")]
        public async Task<HttpResponseMessage> Verify(string faceId1, string faceId2)
        {
            var key = "verify";

            var mongo = new MongoDBHelper<VerifyModels>("faceverify");

            //先检查数据库中是否有上次比较的结果
            var doc = await mongo.SelectOneAsync(x =>
                (x.FaceID1 == faceId1 && x.FaceID2 == faceId2)
                );
            if (doc != null)
            {
                var mongoResult = new
                {
                    faceID1 = doc.FaceID1,
                    faceID2 = doc.FaceID2,
                    confidence = doc.Confidence,
                    isIdentical = doc.IsIdentical
                }.ToJson();



                //var apiResult = doc.ToJson<VerifyModels>();

                return client.CreateHttpResponseMessage(
                    Request,
                    new Models.ProjecToxfordResponseModels(mongoResult, HttpStatusCode.OK));
            }

            //如果之前的结果没有查询到，则提交牛津查询
            var result = await client.PostAsync(
                   key,
                    new
                    {
                        faceId1 = faceId1,
                        faceId2 = faceId2
                    }
                   );

            if (result.StatusCode == HttpStatusCode.OK)
            {

                var tmp = Newtonsoft.Json.Linq.JObject.Parse(result.Message);
                //如果为了加速查询的话，我们采用两次写入
                await mongo.InsertAsync(new VerifyModels()
                {
                    FaceID1 = faceId1,
                    FaceID2 = faceId2,
                    Confidence = (double)tmp["confidence"],
                    IsIdentical = (bool)tmp["isIdentical"]
                });

                await mongo.InsertAsync(new VerifyModels()
                {
                    FaceID1 = faceId2,
                    FaceID2 = faceId1,
                    Confidence = (double)tmp["confidence"],
                    IsIdentical = (bool)tmp["isIdentical"]
                });

                var resultJson = new
                {
                    faceID1 = faceId1,
                    faceID2 = faceId2,
                    confidence = (double)tmp["confidence"],
                    isIdentical = (bool)tmp["isIdentical"]
                }.ToJson();

                return client.CreateHttpResponseMessage(
                    Request,
                    new Models.ProjecToxfordResponseModels(resultJson, HttpStatusCode.OK));
            }
            return client.CreateHttpResponseMessage(Request, result);



        }





        /// 返回提交的照片上的Face信息
        /// </summary>
        /// <param name="weixnmediaid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("face/detect/{weixnmediaid}")]
        public async Task<HttpResponseMessage> Detect(string weixnmediaid)
        {
            var key = "detect";

            //得到从微信服务器下载的文件名
            var fileName = await new WeixinController().Get(weixnmediaid);

            var mongo = new MongoDBHelper<DetectResultModels>("facedetect");

            //照片之前有没有下载过
            var docArr = await mongo.SelectMoreAsync(x => x.FileName == fileName);
            if (docArr.Count > 0)
            {
                var resultJson = docArr.Select(
                    doc => new
                    {
                        faceId = doc.faceId,
                        filename = doc.FileName,
                        age = doc.Age,
                        gender = doc.Gender,
                        smile = doc.Smile
                    }
                    ).ToJson();

                return client.CreateHttpResponseMessage(
                    Request,
                    new Models.ProjecToxfordResponseModels(resultJson, HttpStatusCode.OK));
            }
            //if (docArr != null)
            //{
            //    var apiResult = docArr.ToJson();
            //    return client.CreateHttpResponseMessage(
            //        Request,
            //        new Models.ProjecToxfordResponseModels(apiResult, HttpStatusCode.OK));
            //}

            //如果Mongo中没有该照片对应的Face信息
            var content = await FileHelper.ReadAsync(fileName);

            if (content != null)
            {
                var result = await client.PostAsync(key,
                    content,
                    new Dictionary<string, string> {
                    {"returnFaceId","true"},
                    {"returnFaceLandmarks","flase"},
                    {"returnFaceAttributes","age,gender,smile"}
                    }
                    );

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var tmpJArr = Newtonsoft.Json.Linq.JArray.Parse(result.Message);
                    //将牛津结果写入数据库
                    foreach (var tmp in tmpJArr)
                    {
                        await mongo.InsertAsync(new DetectResultModels()
                        {
                            FileName = fileName,
                            faceId = tmp["faceId"] != null ? (string)tmp["faceId"] : "",
                            Age = tmp["faceAttributes"]["age"] != null ? (double)tmp["faceAttributes"]["age"] : 0,
                            Gender = tmp["faceAttributes"]["gender"] != null ? (string)tmp["faceAttributes"]["gender"] : "",
                            Smile = tmp["faceAttributes"]["smile"] != null ? (double)tmp["faceAttributes"]["smile"] : 0
                        });
                    }
                    //重新从牛津读取数据

                    var resultJson = tmpJArr.Select(x => new
                      {
                          faceId = (string)x["faceId"],
                          age = x["faceAttributes"]["age"]!=null?(double)x["faceAttributes"]["age"]:0,
                          gender =x["faceAttributes"]["gender"]!=null? (string)x["faceAttributes"]["gender"]:"",
                          smile = x["faceAttributes"]["smile"] != null ? (double)x["faceAttributes"]["smile"] : 0,
                          fileName = fileName
                      }).ToJson();

                    return client.CreateHttpResponseMessage(
                        Request,
                        new Models.ProjecToxfordResponseModels(resultJson, HttpStatusCode.OK));
                }
            }
            throw new HttpResponseException(HttpStatusCode.BadRequest);
        }




    }
}
