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
                String strConnStringUd = sql_function.returnConnString("sqlconnStringCSDLW");
                SqlConnection con = new SqlConnection(strConnStringUd);
                con.Open();
                SqlTransaction trans = con.BeginTransaction();
                //Check lớp học
                var str = "select count(*) as total from LOP where  MA_TRUONG = @MA_TRUONG AND MA_NAM_HOC = @MA_NAM_HOC";
                SqlCommand cmd = new SqlCommand(str);
                cmd.Parameters.Add("@MA_TRUONG", SqlDbType.VarChar).Value = rq.MaTruong;
                cmd.Parameters.Add("@MA_NAM_HOC", SqlDbType.VarChar).Value = rq.MaNamHoc;
                String res = sql_function.GetData_Json(cmd, "sqlconnStringCSDLW");
                List<Lop> dshs = JsonSerializer.Deserialize<List<Lop>>(res);

                if(dshs.Count() == 0)
                {
                    rp = new Response(500, "Lớp học "+rq.ID_LOP+" không tồn tại", null, 0);
                    return rp;
                }
                //Check học sinh
                str = "select ID_TRUONG,ID_LOP,ID,MA,MA_SO_GD,MA_KHOI,MA_CAP_HOC from HOC_SINH where  ID_LOP = @ID_LOP and MA_TRUONG =@MA_TRUONG";
                cmd = new SqlCommand(str);
                cmd.Parameters.Add("@ID_LOP", SqlDbType.VarChar).Value = rq.ID_LOP;
                cmd.Parameters.Add("=@MA_TRUONG ", SqlDbType.VarChar).Value = rq.MaTruong;
                res = sql_function.GetData_Json(cmd, "sqlconnStringCSDLW");
                List<HocSinhChuyenCan>  listHs = JsonSerializer.Deserialize<List<HocSinhChuyenCan>>(res);
                if (listHs.Count() == 0)
                {
                    rp = new Response(500, "Không có học sinh nào trong lớp: " + rq.ID_LOP + " ,trường: "+ rq.MaTruong, null, 0);
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
                int totalNLUpdate =0;//tổng update
                foreach(var nl in listNLPC)
                {
                    foreach(var hs in rq.Data)
                    {
                        if(nl.MA_HS_BGD  == hs.MA_HS_BGD && nl.HOC_KY == rq.MaHocKy && nl.DOT_DANH_GIA == hs.DOT_DANH_GIA)
                        {
                            // Thấy giống là update thôi ngại ngần gì
                            str = 
                                "UPDATE BO_GIAO_DUC_2023.dbo.DANH_GIA_NLPC_C1 SET " +
                                "TT22X_NL_TU_CHU_TU_HOC_GK = NL_TU_CHU_TU_HOC," +
                                "TT22X_NL_GIAO_TIEP_HOP_TAC_GK=NL_GIAO_TIEP_HOP_TAC," +
                                "TT22X_NL_GQUYET_VDE_SANG_TAO_GK=NL_GQUYET_VDE_SANG_TAO," +
                                "TT22X_NL_NGON_NGU_GK=NL_NGON_NGU," +
                                "TT22X_NL_TINH_TOAN_GK=NL_TINH_TOAN," +
                                "TT22X_NL_KHOA_HOC_GK=NL_KHOA_HOC," +
                                "TT22X_NL_THAM_MI_GK=NL_THAM_MI," + 
                                "TT22X_NL_THE_CHAT_GK=NL_THE_CHAT," +
                                "TT22X_PC_YEU_NUOC_GK=PC_YEU_NUOC," +
                                "TT22X_PC_NHAN_AI_GK=PC_NHAN_AI," +
                                "TT22X_PC_CHAM_CHI_GK=PC_CHAM_CHI," +
                                "TT22X_PC_TRUNG_THUC_GK=PC_TRUNG_THUC," +
                                "TT22X_PC_TRACH_NHIEM_GK=PC_TRACH_NHIEM," +
                                "NXNL_GK=NXNL,NXNL_DAC_THU_GK=NXNL_DAC_THU," +
                                "NXPC_GK=NXPC,NGAY_SUA = GETDATE()," +
                                "NGUOI_SUA = @ID_NGUOI_DUNG" +
                                "WHERE ID_HOCSINH = @ID_HOCSINH AND HOC_KY =@HOC_KY AND DOT_DANH_GIA = @DOT_DANH_GIA";
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
