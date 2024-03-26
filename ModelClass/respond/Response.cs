using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.respond
{
    public class Response
    {
        public Response(int statusCode, string message, dynamic data, dynamic error) { 
        }
        //200, "Success", stuff, 0
        public int statusCode { get; set; }
        public string message { get; set; }
        public dynamic data { get; set; }
        public dynamic error { get; set; }
    }
}
