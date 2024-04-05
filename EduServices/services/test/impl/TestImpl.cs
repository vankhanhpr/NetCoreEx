using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using ModelClass.respond;
using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Text.Json;
using ModelClass.edu;
using Azure.Core;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EduServices.services.test.impl
{
    public class TestImpl : ITest
    {
        private IConfiguration m_configuration;
        public TestImpl(IConfiguration _configuration)
        {
            m_configuration = _configuration;
        }

        public async Task<dynamic> test(NLPC_Request rq)
        {
            Response rp = new Response(200, "Success", null, 0);
            try
            {
                if (rq.MaHocKy != 1 || (rq.MaHocKy != 2))
                {
                    rp = new Response(500, "Học kỳ không tồn tại", null, 0);
                    return rp;
                }

                SQL_function sql_function = new SQL_function();
                string strConnStringUd = sql_function.returnConnString("sqlconnStringCSDLW");
                SqlConnection con = new SqlConnection(strConnStringUd);
                con.Open();
                SqlTransaction trans = con.BeginTransaction();
                //Check lớp học
                var str = "select ID_LOP as total from LOP where  MA_TRUONG = @MA_TRUONG AND MA_NAM_HOC = @MA_NAM_HOC and ID_LOP = @ID_LOP";
                SqlCommand cmd = new SqlCommand(str);
                cmd.Parameters.Add("@MA_TRUONG", SqlDbType.VarChar).Value = rq.MaTruong;
                cmd.Parameters.Add("@MA_NAM_HOC", SqlDbType.VarChar).Value = rq.MaNamHoc;
                cmd.Parameters.Add("@ID_LOP", SqlDbType.VarChar).Value = rq.ID_LOP;
                string res = sql_function.GetData_Json(cmd, "sqlconnStringCSDLW");
                List<Lop> dshs = JsonSerializer.Deserialize<List<Lop>>(res);

                if (dshs.Count() == 0)
                {
                    rp = new Response(500, "Lớp học " + rq.ID_LOP + " không tồn tại", null, 0);
                    return rp;
                }
                //Check học sinh
                str = "select ID_TRUONG,ID_LOP,ID,MA,MA_SO_GD,MA_KHOI,MA_CAP_HOC from HOC_SINH where  ID_LOP = @ID_LOP and MA_TRUONG =@MA_TRUONG";
                cmd = new SqlCommand(str);
                cmd.Parameters.Add("@ID_LOP", SqlDbType.VarChar).Value = rq.ID_LOP;
                cmd.Parameters.Add("=@MA_TRUONG ", SqlDbType.VarChar).Value = rq.MaTruong;
                res = sql_function.GetData_Json(cmd, "sqlconnStringCSDLW");
                List<HocSinhChuyenCan> listHs = JsonSerializer.Deserialize<List<HocSinhChuyenCan>>(res);
                if (listHs.Count() == 0)
                {
                    rp = new Response(500, "Không có học sinh nào trong lớp: " + rq.ID_LOP + " ,trường: " + rq.MaTruong, null, 0);
                    return rp;
                }
                // Lấy danh đánh giá năng lực phẩm chất
                str = "SELECT ID, MA_NAM_HOC, MA_SO_GD, ID_PHONG_GD, MA_PHONG_GD, ID_TRUONG, MA_TRUONG, ID_HOC_SINH, ID_LOP_MON, HOC_KY, MA_MON_HOC,MA_HS_BGD" +
                    "FROM dbo.DANH_GIA_DINH_KY_C1 " +
                    "WHERE MA_NAM_HOC = @MaNamHoc AND MA_TRUONG = @MaTruong AND HOC_KY = @MaHocKy and ID_LOP = @ID_LOP";

                cmd = new SqlCommand(str);
                cmd.Parameters.Add("@MaNamHoc", SqlDbType.VarChar).Value = rq.MaNamHoc;
                cmd.Parameters.Add("=@MaTruong ", SqlDbType.VarChar).Value = rq.MaTruong;
                cmd.Parameters.Add("=@MaHocKy ", SqlDbType.VarChar).Value = rq.MaHocKy;
                cmd.Parameters.Add("=@ID_LOP ", SqlDbType.VarChar).Value = rq.ID_LOP;
                res = sql_function.GetData_Json(cmd, "sqlconnStringCSDLW");
                List<NLPC_Data> listNLPC = JsonSerializer.Deserialize<List<NLPC_Data>>(res);

                //check nếu tồn tại thì update không tồn tại thì check học sinh đó tồn tại hay không
                //Nếu học sinh tồn tại thì cho phép insert, nếu không tồn tại học sinh thì báo lỗi học sinh đó
                int totalNLUpdate = 0;//tổng update
                foreach (var nl in listNLPC)
                {
                    foreach (var hs in rq.Data)
                    {
                        if (nl.MA_HS_BGD == hs.MA_HS_BGD && nl.HOC_KY == rq.MaHocKy && nl.DOT_DANH_GIA == hs.DOT_DANH_GIA)
                        {
                            // Thấy giống là update thôi ngại ngần gì
                            str =
                                "UPDATE BO_GIAO_DUC_2023.dbo.DANH_GIA_NLPC_C1 SET " +
                                    "TT22X_NL_TU_CHU_TU_HOC_GK = @NL_TU_CHU_TU_HOC," +
                                    "TT22X_NL_GIAO_TIEP_HOP_TAC_GK = @NL_GIAO_TIEP_HOP_TAC," +
                                    "TT22X_NL_GQUYET_VDE_SANG_TAO_GK = @NL_GQUYET_VDE_SANG_TAO," +
                                    "TT22X_NL_NGON_NGU_GK = @NL_NGON_NGU," +
                                    "TT22X_NL_TINH_TOAN_GK = @NL_TINH_TOAN," +
                                    "TT22X_NL_KHOA_HOC_GK = @NL_KHOA_HOC," +
                                    "TT22X_NL_THAM_MI_GK = @NL_THAM_MI," +
                                    "TT22X_NL_THE_CHAT_GK = @NL_THE_CHAT," +
                                    "TT22X_PC_YEU_NUOC_GK = @PC_YEU_NUOC," +
                                    "TT22X_PC_NHAN_AI_GK = @PC_NHAN_AI," +
                                    "TT22X_PC_CHAM_CHI_GK = @PC_CHAM_CHI," +
                                    "TT22X_PC_TRUNG_THUC_GK = @PC_TRUNG_THUC," +
                                    "TT22X_PC_TRACH_NHIEM_GK = @PC_TRACH_NHIEM," +
                                    "NXNL_GK = @NXNL," +
                                    "NXNL_DAC_THU_GK = @NXNL_DAC_THU," +
                                    "NXPC_GK = @NXPC," +
                                    "NGAY_SUA = GETDATE()," +
                                    "NGUOI_SUA = @ID_NGUOI_DUNG" +
                                "WHERE ID_HOCSINH = @ID_HOCSINH AND HOC_KY =@HOC_KY AND DOT_DANH_GIA = @DOT_DANH_GIA";
                            cmd = new SqlCommand(str);
                            cmd.Parameters.Add("@NL_TU_CHU_TU_HOC", SqlDbType.Int).Value = hs.NL_TU_CHU_TU_HOC;
                            cmd.Parameters.Add("@NL_GIAO_TIEP_HOP_TAC", SqlDbType.VarChar).Value = hs.NL_GIAO_TIEP_HOP_TAC;
                            cmd.Parameters.Add("@NL_GQUYET_VDE_SANG_TAO", SqlDbType.VarChar).Value = hs.NL_GQUYET_VDE_SANG_TAO;
                            cmd.Parameters.Add("@NL_NGON_NGU", SqlDbType.VarChar).Value = hs.NL_NGON_NGU;
                            cmd.Parameters.Add("@NL_TINH_TOAN", SqlDbType.VarChar).Value = hs.NL_TINH_TOAN;
                            cmd.Parameters.Add("@NL_KHOA_HOC", SqlDbType.VarChar).Value = hs.NL_KHOA_HOC;
                            cmd.Parameters.Add("@NL_THAM_MI", SqlDbType.VarChar).Value = hs.NL_THAM_MI;
                            cmd.Parameters.Add("@NL_THE_CHAT", SqlDbType.VarChar).Value = hs.NL_THE_CHAT;
                            cmd.Parameters.Add("@PC_YEU_NUOC", SqlDbType.VarChar).Value = hs.PC_YEU_NUOC;
                            cmd.Parameters.Add("@PC_NHAN_AI", SqlDbType.VarChar).Value = hs.PC_NHAN_AI;
                            cmd.Parameters.Add("@PC_CHAM_CHI", SqlDbType.VarChar).Value = hs.PC_CHAM_CHI;
                            cmd.Parameters.Add("@PC_TRUNG_THUC", SqlDbType.VarChar).Value = hs.PC_TRUNG_THUC;
                            cmd.Parameters.Add("@PC_TRACH_NHIEM", SqlDbType.VarChar).Value = hs.PC_TRACH_NHIEM;
                            cmd.Parameters.Add("@NXNL_GK", SqlDbType.VarChar).Value = hs.NXNL_GK;
                            cmd.Parameters.Add("@NXNL_DAC_THU", SqlDbType.VarChar).Value = hs.NXNL_DAC_THU;
                            cmd.Parameters.Add("@NXPC", SqlDbType.VarChar).Value = hs.NXPC;
                            cmd.Parameters.Add("@ID_NGUOI_DUNG", SqlDbType.VarChar).Value = hs.ID_NGUOI_DUNG;

                            cmd.Parameters.Add("@ID_HOCSINH", SqlDbType.VarChar).Value = hs.ID_HOCSINH;
                            cmd.Parameters.Add("@HOC_KY", SqlDbType.VarChar).Value = rq.MaHocKy;
                            cmd.Parameters.Add("@DOT_DANH_GIA", SqlDbType.VarChar).Value = hs.DOT_DANH_GIA;


                            cmd.Parameters.Add("=@MA_TRUONG ", SqlDbType.VarChar).Value = rq.MaTruong;
                            res = sql_function.GetData_Json(cmd, "sqlconnStringCSDLW");
                            totalNLUpdate = totalNLUpdate + Int32.Parse(cmd.ExecuteNonQuery().ToString());
                        }
                        else
                        {
                            foreach (var hs_ins in listHs)
                            {
                                if (hs_ins.MA_HS_BGD == hs.MA_HS_BGD)
                                {
                                    str = "INSERT INTO DANH_GIA_NLPC_C1(" +
                                        "MA_NAM_HOC,MA_SO_GD ,ID_PHONG_GD ,MA_PHONG_GD ,ID_TRUONG  ,MA_TRUONG ,ID_HOC_SINH ,HOC_KY,NL_TPVTQ_GK ,NL_HT_GK ,NL_THGQVD_GK  " +
                                        ",PC_CHCL_GK ,PC_TTTN_GK ,PC_TTKL_GK ,PC_DKYT_GK  ,MA_NXNL_GK ,NXNL_GK  ,MA_NXPC_GK ,NXPC_GK  ,NL_TPVTQ_CK ,NL_HT_CK ,NL_THGQVD_CK  " +
                                        ",PC_CHCL_CK ,PC_TTTN_CK  ,PC_TTKL_CK ,PC_DKYT_CK  ,MA_NXNL_CK ,NXNL_CK  ,MA_NXPC_CK ,NXPC_CK  ,NGUOI_TAO ,NGAY_TAO ,NGUOI_SUA " +
                                        ",NGAY_SUA ,NL_TPVTQ_CK_REN_LUYEN_LAI ,NL_HT_CK_REN_LUYEN_LAI  N,L_THGQVD_CK_REN_LUYEN_LAI ,PC_CHCL_CK_REN_LUYEN_LAI " +
                                        ",PC_TTTN_CK_REN_LUYEN_LAI ,PC_TTKL_CK_REN_LUYEN_LAI  ,PC_DKYT_CK_REN_LUYEN_LAI ,TT22X_PC_YEU_NUOC_GK  ,TT22X_PC_NHAN_AI_GK " +
                                        ",TT22X_PC_CHAM_CHI_GK  ,TT22X_PC_TRUNG_THUC_GK ,TT22X_PC_TRACH_NHIEM_GK  ,TT22X_NL_TU_CHU_TU_HOC_GK ,TT22X_NL_GIAO_TIEP_HOP_TAC_GK " +
                                        ",TT22X_NL_GQUYET_VDE_SANG_TAO_GK ,TT22X_NL_NGON_NGU_GK  ,TT22X_NL_TINH_TOAN_GK ,TT22X_NL_KHOA_HOC_GK  ,TT22X_NL_CONG_NGHE_GK" +
                                        ",TT22X_NL_TIN_HOC_GK  ,TT22X_NL_THAM_MI_GK ,TT22X_NL_THE_CHAT_GK  ,TT22X_PC_YEU_NUOC_CK  ,TT22X_PC_NHAN_AI_CK ,TT22X_PC_CHAM_CHI_CK " +
                                        ",TT22X_PC_TRUNG_THUC_CK ,TT22X_PC_TRACH_NHIEM_CK  ,TT22X_NL_TU_CHU_TU_HOC_CK ,TT22X_NL_GIAO_TIEP_HOP_TAC_CK " +
                                        ",TT22X_NL_GQUYET_VDE_SANG_TAO_CK ,TT22X_NL_NGON_NGU_CK  ,TT22X_NL_TINH_TOAN_CK ,TT22X_NL_KHOA_HOC_CK ,TT22X_NL_CONG_NGHE_CK " +
                                        ",TT22X_NL_TIN_HOC_CK  ,TT22X_NL_THAM_MI_CK ,TT22X_NL_THE_CHAT_CK  ,TT22X_PC_YEU_NUOC_REN_LUYEN_LAI  ,TT22X_PC_NHAN_AI_REN_LUYEN_LAI" +
                                        " ,TT22X_PC_CHAM_CHI_REN_LUYEN_LAI  ,TT22X_PC_TRUNG_THUC_REN_LUYEN_LAI ,TT22X_PC_TRACH_NHIEM_REN_LUYEN_LAI  " +
                                        ",TT22X_NL_TU_CHU_TU_HOC_REN_LUYEN_LAI ,TT22X_NL_GIAO_TIEP_HOP_TAC_REN_LUYEN_LAI  " +
                                        ",TT22X_NL_GQUYET_VDE_SANG_TAO_REN_LUYEN_LAI ,TT22X_NL_NGON_NGU_REN_LUYEN_LAI  " +
                                        ",TT22X_NL_TINH_TOAN_REN_LUYEN_LAI ,TT22X_NL_KHOA_HOC_REN_LUYEN_LAI  ,TT22X_NL_CONG_NGHE_REN_LUYEN_LAI " +
                                        ",TT22X_NL_TIN_HOC_REN_LUYEN_LAI  ,TT22X_NL_THAM_MI_REN_LUYEN_LAI ,TT22X_NL_THE_CHAT_REN_LUYEN_LAI " +
                                        ",MA_NXNL_DAC_THU_GK ,NXNL_DAC_THU_GK ,MA_NXNL_DAC_THU_CK  ,NXNL_DAC_THU_CK " +
                                        ",IS_MIEN_NANG_LUC ,IS_MIEN_PHAM_CHAT,ROW_VERSIONID,)" +
                                        "Values(" +
                                        "@MA_NAM_HOC," +
                                        "@MA_SO_GD," +
                                        "@ID_PHONG_GD," +
                                        "@MA_PHONG_GD," +
                                        "@ID_TRUONG," +
                                        "@MA_TRUONG," +
                                        "@ID_HOC_SINH," +
                                        "@HOC_KY," +
                                        "@NL_TPVTQ_GK," +
                                        "@NL_HT_GK," +
                                        "@NL_THGQVD_GK," +
                                        "@PC_CHCL_GK," +
                                        "@PC_TTTN_GK," +
                                        "@PC_TTKL_GK," +
                                        "@PC_DKYT_GK," +
                                        "@MA_NXNL_GK," +
                                        "@NXNL_GK," +
                                        "@MA_NXPC_GK," +
                                        "@NXPC_GK," +
                                        "@NL_TPVTQ_CK," +
                                        "@NL_HT_CK," +
                                        "@NL_THGQVD_CK," +
                                        "@PC_CHCL_CK," +
                                        "@PC_TTTN_CK," +
                                        "@PC_TTKL_CK," +
                                        "@PC_DKYT_CK," +
                                        "@MA_NXNL_CK," +
                                        "@NXNL_CK," +
                                        "@MA_NXPC_CK," +
                                        "@NXPC_CK," +
                                        "@NGUOI_TAO," +
                                        "@NGAY_TAO," +
                                        "@NGUOI_SUA," +
                                        "@NGAY_SUA," +
                                        "@NL_TPVTQ_CK_REN_LUYEN_LAI," +
                                        "@NL_HT_CK_REN_LUYEN_LAI," +
                                        "@NL_THGQVD_CK_REN_LUYEN_LAI," +
                                        "@PC_CHCL_CK_REN_LUYEN_LAI," +
                                        "@PC_TTTN_CK_REN_LUYEN_LAI," +
                                        "@PC_TTKL_CK_REN_LUYEN_LAI," +
                                        "@PC_DKYT_CK_REN_LUYEN_LAI," +
                                        "@TT22X_PC_YEU_NUOC_GK," +
                                        "@TT22X_PC_NHAN_AI_GK," +
                                        "@TT22X_PC_CHAM_CHI_GK," +
                                        "@TT22X_PC_TRUNG_THUC_GK," +
                                        "@TT22X_PC_TRACH_NHIEM_GK," +
                                        "@TT22X_NL_TU_CHU_TU_HOC_GK," +
                                        "@TT22X_NL_GIAO_TIEP_HOP_TAC_GK," +
                                        "@TT22X_NL_GQUYET_VDE_SANG_TAO_GK," +
                                        "@TT22X_NL_NGON_NGU_GK," +
                                        "@TT22X_NL_TINH_TOAN_GK," +
                                        "@TT22X_NL_KHOA_HOC_GK," +
                                        "@TT22X_NL_CONG_NGHE_GK," +
                                        "@TT22X_NL_TIN_HOC_GK," +
                                        "@TT22X_NL_THAM_MI_GK," +
                                        "@TT22X_NL_THE_CHAT_GK," +
                                        "@TT22X_PC_YEU_NUOC_CK," +
                                        "@TT22X_PC_NHAN_AI_CK," +
                                        "@TT22X_PC_CHAM_CHI_CK," +
                                        "@TT22X_PC_TRUNG_THUC_CK," +
                                        "@TT22X_PC_TRACH_NHIEM_CK," +
                                        "@TT22X_NL_TU_CHU_TU_HOC_CK," +
                                        "@TT22X_NL_GIAO_TIEP_HOP_TAC_CK," +
                                        "@TT22X_NL_GQUYET_VDE_SANG_TAO_CK," +
                                        "@TT22X_NL_NGON_NGU_CK," +
                                        "@TT22X_NL_TINH_TOAN_CK," +
                                        "@TT22X_NL_KHOA_HOC_CK," +
                                        "@TT22X_NL_CONG_NGHE_CK," +
                                        "@TT22X_NL_TIN_HOC_CK," +
                                        "@TT22X_NL_THAM_MI_CK," +
                                        "@TT22X_NL_THE_CHAT_CK," +
                                        "@TT22X_PC_YEU_NUOC_REN_LUYEN_LAI," +
                                        "@TT22X_PC_NHAN_AI_REN_LUYEN_LAI," +
                                        "@TT22X_PC_CHAM_CHI_REN_LUYEN_LAI," +
                                        "@TT22X_PC_TRUNG_THUC_REN_LUYEN_LAI," +
                                        "@TT22X_PC_TRACH_NHIEM_REN_LUYEN_LAI," +
                                        "@TT22X_NL_TU_CHU_TU_HOC_REN_LUYEN_LAI," +
                                        "@TT22X_NL_GIAO_TIEP_HOP_TAC_REN_LUYEN_LAI," +
                                        "@TT22X_NL_GQUYET_VDE_SANG_TAO_REN_LUYEN_LAI," +
                                        "@TT22X_NL_NGON_NGU_REN_LUYEN_LAI," +
                                        "@TT22X_NL_TINH_TOAN_REN_LUYEN_LAI," +
                                        "@TT22X_NL_KHOA_HOC_REN_LUYEN_LAI," +
                                        "@TT22X_NL_CONG_NGHE_REN_LUYEN_LAI," +
                                        "@TT22X_NL_TIN_HOC_REN_LUYEN_LAI," +
                                        "@TT22X_NL_THAM_MI_REN_LUYEN_LAI," +
                                        "@TT22X_NL_THE_CHAT_REN_LUYEN_LAI," +
                                        "@MA_NXNL_DAC_THU_GK," +
                                        "@NXNL_DAC_THU_GK," +
                                        "@MA_NXNL_DAC_THU_CK," +
                                        "@NXNL_DAC_THU_CK," +
                                        "@IS_MIEN_NANG_LUC," +
                                        "@IS_MIEN_PHAM_CHAT," +
                                        "@ROW_VERSIONID," +
                                        ")";

                                    cmd.Parameters.Add("@MA_NAM_HOC", SqlDbType.Int).Value = hs.MA_NAM_HOC;
                                    cmd.Parameters.Add("@MA_SO_GD", SqlDbType.NVarChar).Value = hs.MA_SO_GD;
                                    cmd.Parameters.Add("@ID_PHONG_GD", SqlDbType.BigInt).Value = hs.ID_PHONG_GD;
                                    cmd.Parameters.Add("@MA_PHONG_GD", SqlDbType.NVarChar).Value = hs.MA_PHONG_GD;
                                    cmd.Parameters.Add("@ID_TRUONG", SqlDbType.Int).Value = hs.ID_TRUONG;
                                    cmd.Parameters.Add("@MA_TRUONG", SqlDbType.NVarChar).Value = hs.MA_TRUONG;
                                    cmd.Parameters.Add("@ID_HOC_SINH", SqlDbType.Int).Value = hs.ID_HOC_SINH;
                                    cmd.Parameters.Add("@HOC_KY", SqlDbType.Int).Value = hs.HOC_KY;
                                    cmd.Parameters.Add("@NL_TPVTQ_GK", SqlDbType.NVarChar).Value = hs.NL_TPVTQ_GK;
                                    cmd.Parameters.Add("@NL_HT_GK", SqlDbType.NVarChar).Value = hs.NL_HT_GK;
                                    cmd.Parameters.Add("@NL_THGQVD_GK", SqlDbType.NVarChar).Value = hs.NL_THGQVD_GK;
                                    cmd.Parameters.Add("@PC_CHCL_GK", SqlDbType.NVarChar).Value = hs.PC_CHCL_GK;
                                    cmd.Parameters.Add("@PC_TTTN_GK", SqlDbType.NVarChar).Value = hs.PC_TTTN_GK;
                                    cmd.Parameters.Add("@PC_TTKL_GK", SqlDbType.NVarChar).Value = hs.PC_TTKL_GK;
                                    cmd.Parameters.Add("@PC_DKYT_GK", SqlDbType.NVarChar).Value = hs.PC_DKYT_GK;
                                    cmd.Parameters.Add("@MA_NXNL_GK", SqlDbType.NVarChar).Value = hs.MA_NXNL_GK;
                                    cmd.Parameters.Add("@NXNL_GK", SqlDbType.NVarChar).Value = hs.NXNL_GK;
                                    cmd.Parameters.Add("@MA_NXPC_GK", SqlDbType.NVarChar).Value = hs.MA_NXPC_GK;
                                    cmd.Parameters.Add("@NXPC_GK", SqlDbType.NVarChar).Value = hs.NXPC_GK;
                                    cmd.Parameters.Add("@NL_TPVTQ_CK", SqlDbType.NVarChar).Value = hs.NL_TPVTQ_CK;
                                    cmd.Parameters.Add("@NL_HT_CK", SqlDbType.NVarChar).Value = hs.NL_HT_CK;
                                    cmd.Parameters.Add("@NL_THGQVD_CK", SqlDbType.NVarChar).Value = hs.NL_THGQVD_CK;
                                    cmd.Parameters.Add("@PC_CHCL_CK", SqlDbType.NVarChar).Value = hs.PC_CHCL_CK;
                                    cmd.Parameters.Add("@PC_TTTN_CK", SqlDbType.NVarChar).Value = hs.PC_TTTN_CK;
                                    cmd.Parameters.Add("@PC_TTKL_CK", SqlDbType.NVarChar).Value = hs.PC_TTKL_CK;
                                    cmd.Parameters.Add("@PC_DKYT_CK", SqlDbType.NVarChar).Value = hs.PC_DKYT_CK;
                                    cmd.Parameters.Add("@MA_NXNL_CK", SqlDbType.NVarChar).Value = hs.MA_NXNL_CK;
                                    cmd.Parameters.Add("@NXNL_CK", SqlDbType.NVarChar).Value = hs.NXNL_CK;
                                    cmd.Parameters.Add("@MA_NXPC_CK", SqlDbType.NVarChar).Value = hs.MA_NXPC_CK;
                                    cmd.Parameters.Add("@NXPC_CK", SqlDbType.NVarChar).Value = hs.NXPC_CK;
                                    cmd.Parameters.Add("@NGUOI_TAO", SqlDbType.Int).Value = hs.NGUOI_TAO;
                                    cmd.Parameters.Add("@NGAY_TAO", SqlDbType.Datetime).Value = hs.NGAY_TAO;
                                    cmd.Parameters.Add("@NGUOI_SUA", SqlDbType.Int).Value = hs.NGUOI_SUA;
                                    cmd.Parameters.Add("@NGAY_SUA", SqlDbType.Datetime).Value = hs.NGAY_SUA;
                                    cmd.Parameters.Add("@NL_TPVTQ_CK_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.NL_TPVTQ_CK_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@NL_HT_CK_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.NL_HT_CK_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@NL_THGQVD_CK_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.NL_THGQVD_CK_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@PC_CHCL_CK_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.PC_CHCL_CK_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@PC_TTTN_CK_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.PC_TTTN_CK_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@PC_TTKL_CK_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.PC_TTKL_CK_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@PC_DKYT_CK_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.PC_DKYT_CK_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@TT22X_PC_YEU_NUOC_GK", SqlDbType.NVarChar).Value = hs.TT22X_PC_YEU_NUOC_GK;
                                    cmd.Parameters.Add("@TT22X_PC_NHAN_AI_GK", SqlDbType.NVarChar).Value = hs.TT22X_PC_NHAN_AI_GK;
                                    cmd.Parameters.Add("@TT22X_PC_CHAM_CHI_GK", SqlDbType.NVarChar).Value = hs.TT22X_PC_CHAM_CHI_GK;
                                    cmd.Parameters.Add("@TT22X_PC_TRUNG_THUC_GK", SqlDbType.NVarChar).Value = hs.TT22X_PC_TRUNG_THUC_GK;
                                    cmd.Parameters.Add("@TT22X_PC_TRACH_NHIEM_GK", SqlDbType.NVarChar).Value = hs.TT22X_PC_TRACH_NHIEM_GK;
                                    cmd.Parameters.Add("@TT22X_NL_TU_CHU_TU_HOC_GK", SqlDbType.NVarChar).Value = hs.TT22X_NL_TU_CHU_TU_HOC_GK;
                                    cmd.Parameters.Add("@TT22X_NL_GIAO_TIEP_HOP_TAC_GK", SqlDbType.NVarChar).Value = hs.TT22X_NL_GIAO_TIEP_HOP_TAC_GK;
                                    cmd.Parameters.Add("@TT22X_NL_GQUYET_VDE_SANG_TAO_GK", SqlDbType.NVarChar).Value = hs.TT22X_NL_GQUYET_VDE_SANG_TAO_GK;
                                    cmd.Parameters.Add("@TT22X_NL_NGON_NGU_GK", SqlDbType.NVarChar).Value = hs.TT22X_NL_NGON_NGU_GK;
                                    cmd.Parameters.Add("@TT22X_NL_TINH_TOAN_GK", SqlDbType.NVarChar).Value = hs.TT22X_NL_TINH_TOAN_GK;
                                    cmd.Parameters.Add("@TT22X_NL_KHOA_HOC_GK", SqlDbType.NVarChar).Value = hs.TT22X_NL_KHOA_HOC_GK;
                                    cmd.Parameters.Add("@TT22X_NL_CONG_NGHE_GK", SqlDbType.NVarChar).Value = hs.TT22X_NL_CONG_NGHE_GK;
                                    cmd.Parameters.Add("@TT22X_NL_TIN_HOC_GK", SqlDbType.NVarChar).Value = hs.TT22X_NL_TIN_HOC_GK;
                                    cmd.Parameters.Add("@TT22X_NL_THAM_MI_GK", SqlDbType.NVarChar).Value = hs.TT22X_NL_THAM_MI_GK;
                                    cmd.Parameters.Add("@TT22X_NL_THE_CHAT_GK", SqlDbType.NVarChar).Value = hs.TT22X_NL_THE_CHAT_GK;
                                    cmd.Parameters.Add("@TT22X_PC_YEU_NUOC_CK", SqlDbType.NVarChar).Value = hs.TT22X_PC_YEU_NUOC_CK;
                                    cmd.Parameters.Add("@TT22X_PC_NHAN_AI_CK", SqlDbType.NVarChar).Value = hs.TT22X_PC_NHAN_AI_CK;
                                    cmd.Parameters.Add("@TT22X_PC_CHAM_CHI_CK", SqlDbType.NVarChar).Value = hs.TT22X_PC_CHAM_CHI_CK;
                                    cmd.Parameters.Add("@TT22X_PC_TRUNG_THUC_CK", SqlDbType.NVarChar).Value = hs.TT22X_PC_TRUNG_THUC_CK;
                                    cmd.Parameters.Add("@TT22X_PC_TRACH_NHIEM_CK", SqlDbType.NVarChar).Value = hs.TT22X_PC_TRACH_NHIEM_CK;
                                    cmd.Parameters.Add("@TT22X_NL_TU_CHU_TU_HOC_CK", SqlDbType.NVarChar).Value = hs.TT22X_NL_TU_CHU_TU_HOC_CK;
                                    cmd.Parameters.Add("@TT22X_NL_GIAO_TIEP_HOP_TAC_CK", SqlDbType.NVarChar).Value = hs.TT22X_NL_GIAO_TIEP_HOP_TAC_CK;
                                    cmd.Parameters.Add("@TT22X_NL_GQUYET_VDE_SANG_TAO_CK", SqlDbType.NVarChar).Value = hs.TT22X_NL_GQUYET_VDE_SANG_TAO_CK;
                                    cmd.Parameters.Add("@TT22X_NL_NGON_NGU_CK", SqlDbType.NVarChar).Value = hs.TT22X_NL_NGON_NGU_CK;
                                    cmd.Parameters.Add("@TT22X_NL_TINH_TOAN_CK", SqlDbType.NVarChar).Value = hs.TT22X_NL_TINH_TOAN_CK;
                                    cmd.Parameters.Add("@TT22X_NL_KHOA_HOC_CK", SqlDbType.NVarChar).Value = hs.TT22X_NL_KHOA_HOC_CK;
                                    cmd.Parameters.Add("@TT22X_NL_CONG_NGHE_CK", SqlDbType.NVarChar).Value = hs.TT22X_NL_CONG_NGHE_CK;
                                    cmd.Parameters.Add("@TT22X_NL_TIN_HOC_CK", SqlDbType.NVarChar).Value = hs.TT22X_NL_TIN_HOC_CK;
                                    cmd.Parameters.Add("@TT22X_NL_THAM_MI_CK", SqlDbType.NVarChar).Value = hs.TT22X_NL_THAM_MI_CK;
                                    cmd.Parameters.Add("@TT22X_NL_THE_CHAT_CK", SqlDbType.NVarChar).Value = hs.TT22X_NL_THE_CHAT_CK;
                                    cmd.Parameters.Add("@TT22X_PC_YEU_NUOC_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.TT22X_PC_YEU_NUOC_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@TT22X_PC_NHAN_AI_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.TT22X_PC_NHAN_AI_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@TT22X_PC_CHAM_CHI_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.TT22X_PC_CHAM_CHI_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@TT22X_PC_TRUNG_THUC_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.TT22X_PC_TRUNG_THUC_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@TT22X_PC_TRACH_NHIEM_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.TT22X_PC_TRACH_NHIEM_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@TT22X_NL_TU_CHU_TU_HOC_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.TT22X_NL_TU_CHU_TU_HOC_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@TT22X_NL_GIAO_TIEP_HOP_TAC_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.TT22X_NL_GIAO_TIEP_HOP_TAC_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@TT22X_NL_GQUYET_VDE_SANG_TAO_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.TT22X_NL_GQUYET_VDE_SANG_TAO_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@TT22X_NL_NGON_NGU_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.TT22X_NL_NGON_NGU_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@TT22X_NL_TINH_TOAN_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.TT22X_NL_TINH_TOAN_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@TT22X_NL_KHOA_HOC_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.TT22X_NL_KHOA_HOC_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@TT22X_NL_CONG_NGHE_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.TT22X_NL_CONG_NGHE_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@TT22X_NL_TIN_HOC_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.TT22X_NL_TIN_HOC_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@TT22X_NL_THAM_MI_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.TT22X_NL_THAM_MI_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@TT22X_NL_THE_CHAT_REN_LUYEN_LAI", SqlDbType.NVarChar).Value = hs.TT22X_NL_THE_CHAT_REN_LUYEN_LAI;
                                    cmd.Parameters.Add("@MA_NXNL_DAC_THU_GK", SqlDbType.NVarChar).Value = hs.MA_NXNL_DAC_THU_GK;
                                    cmd.Parameters.Add("@NXNL_DAC_THU_GK", SqlDbType.NVarChar).Value = hs.NXNL_DAC_THU_GK;
                                    cmd.Parameters.Add("@MA_NXNL_DAC_THU_CK", SqlDbType.NVarChar).Value = hs.MA_NXNL_DAC_THU_CK;
                                    cmd.Parameters.Add("@NXNL_DAC_THU_CK", SqlDbType.NVarChar).Value = hs.NXNL_DAC_THU_CK;
                                    cmd.Parameters.Add("@IS_MIEN_NANG_LUC", SqlDbType.Int).Value = hs.IS_MIEN_NANG_LUC;
                                    cmd.Parameters.Add("@IS_MIEN_PHAM_CHAT", SqlDbType.Int).Value = hs.IS_MIEN_PHAM_CHAT;
                                    cmd.Parameters.Add("@ROW_VERSIONID", SqlDbType.Int).Value = hs.ROW_VERSIONID;

                                }

                            }
                        }
                    }
                }
                //con.Close();
                //trans.Commit();
                return rp;
            }
            catch
            {
                data.success = false;

            }
            return data;

        }
    }
}
