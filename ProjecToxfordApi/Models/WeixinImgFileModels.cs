using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjecToxfordApi.Models
{
    public class WeixinImgFileModels
    {
        public ObjectId _id { set; get; }
        public string MediaId { set; get; }
        public string FileName { set; get; }

    }
}