using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.user
{
    public class User
    {
        [Key]
        public int usid { get; set; }
        public string username { get; set; }
        public string? password { get; set; }
        public int? status { get; set; }

    }
}
