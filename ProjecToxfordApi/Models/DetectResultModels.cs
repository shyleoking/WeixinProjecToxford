using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjecToxfordApi.Models
{
    /// <summary>
    /// 一个FileName可以和多个FaceID关联
    /// </summary>
    public class DetectResultModels
    {
        public ObjectId _id { set; get; }
        public string faceId { set; get; }
        public string FileName { set; get; }
        public double Age { set; get; }
        public string Gender { set; get; }
        public double Smile { set; get; }

    }

    public class FaceRectangleModels
    {
        public double top { set; get; }
        public double left { set; get; }
        public double width { set; get; }
        public double height { set; get; }
    }

    public class FaceAttribute
    {
        public double Age { set; get; }
        public string Gender { set; get; }
        public double Smile { set; get; }

    }


}