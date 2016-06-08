using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;

namespace ProjecToxfordApi.Models
{
    public class ProjecToxfordResponseModels
    {
        public ProjecToxfordResponseModels(string msg, HttpStatusCode code)
        {
            Message = msg;
            StatusCode = code;
        }
        public string Message { private set; get; }
        public HttpStatusCode StatusCode { private set; get; }
    }
}