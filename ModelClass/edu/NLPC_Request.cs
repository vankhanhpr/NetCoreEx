using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.edu
{
    public class NLPC_Request
    {
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }
        public string MaTruong { get; set; }
        public string MaNamHoc { get; set; }
        public string? Type { get; set; }
        public int MaHocKy { get; set; }
        public List<DANH_GIA_NLPC_C1>? Data { get; set; }
        public string MA_LOP { get; set; }
        public string ID_LOP { get; set; }

        public class HOC_SINH
        {
            public int ID_TRUONG { get; set; }
            public int ID_LOP { get; set; }
            public int ID { get; set; }
            public string MA { get; set; }
            public string MA_SO_GD { get; set; }
            public string MA_KHOI { get; set; }
            public string MA_CAP_HOC { get; set; }
        }
    }
}
