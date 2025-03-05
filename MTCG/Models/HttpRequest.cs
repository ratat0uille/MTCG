using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public class HttpRequest
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string Version { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();
        public string Body { get; set; }
        public Dictionary<string, string> QueryParams { get; set; } = new();
    }
}
