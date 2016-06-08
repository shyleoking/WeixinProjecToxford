using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjecToxfordApi.Models
{
    /// <summary>
    /// 对两个FaceID做出的比较结果
    /// </summary>
    public class VerifyModels
    {
        public ObjectId _id { set; get; }

        public string FaceID1 { set; get; }

        public string FaceID2 { set; get; }

        public double Confidence { set; get; }

        public bool IsIdentical { set; get; }

    }
}