using System;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Models
{
    public class Redirection : ODataObject
    {
        public string Method { get; set; }

        public Zone Zone { get; set; }

        public Uri Uri { get; set; }

        public string Body { get; set; }
    }
}
