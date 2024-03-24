using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.auth.onebss
{
    public class TokenRequest
    {
        public int us_id { get; set; }
        public string client_id { get; set; }   
        public string client_secret { get; set;}
        public string grant_type { get; set;}
        public string otp { get; set;}
        public string secretCode { get; set;}
    }
}
