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
        public List<DataNLPC>? Data { get; set; }
        public string MA_LOP { get; set; }
        public string ID_LOP { get; set; }

        public class DataNLPC
        {
            public string DOT_DANH_GIA { get; set; }
            public string MA_HS_BGD { get; set; }
            public string NL_TU_CHU_TU_HOC { get; set; }
            public string NL_GIAO_TIEP_HOP_TAC { get; set; }
            public string NL_GQUYET_VDE_SANG_TAO { get; set; }
            public string NL_NGON_NGU { get; set; }
            public string NL_TINH_TOAN { get; set; }
            public string NL_KHOA_HOC { get; set; }
            public string NL_CONG_NGHE { get; set; }
            public string NL_TIN_HOC { get; set; }
            public string NL_THAM_MI { get; set; }
            public string NL_THE_CHAT { get; set; }
            public string PC_YEU_NUOC { get; set; }
            public string PC_NHAN_AI { get; set; }
            public string PC_CHAM_CHI { get; set; }
            public string PC_TRUNG_THUC { get; set; }
            public string PC_TRACH_NHIEM { get; set; }
            public string NXNL { get; set; }
            public string NXNL_GK { get; set; }
            public string NXNL_DAC_THU { get; set; }
            public string NXPC { get; set; }
        }


    }
}
