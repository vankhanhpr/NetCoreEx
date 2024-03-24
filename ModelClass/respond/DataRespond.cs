using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.respond
{
    public class DataRespond
    {
        public string message { get; set; }
        public dynamic data { get; set; }
        public dynamic error { get; set; }
        public Boolean success {get; set; }

    }
}
