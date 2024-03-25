using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using ModelClass.respond;
using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Data.Common;

namespace EduServices.services.test.impl
{
    public class TestImpl : ITest
    {
        private IConfiguration m_configuration;
        public TestImpl(IConfiguration _configuration)
        {
            m_configuration = _configuration;
        }

        public async Task<dynamic> test()
        {
            DataRespond data = new DataRespond();

            try
            {
                using (SqlConnection connection = new SqlConnection(m_configuration.GetSection("ConnectionStrings").GetSection("DefaultConnection").Value))
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        connection.Open();
                        command.CommandText = "select * from users where username = 'admin'";
                        //command.Parameters.AddWithValue("@username", "admin");

                        //command.Parameters.AddWithValue("@admin" , "admin");

                        command.CommandType = CommandType.Text;
                        DataTable table = new DataTable();
                        using (var reader = command.ExecuteReader())
                        {
                            table.Load(reader);
                            //var dictionary = new Dictionary<string, List<object>>();
                            //foreach (DataColumn dataColumn in table.Columns)
                            //{
                            //    var columnValueList = new List<object>();

                            //    foreach (DataRow dataRow in table.Rows)
                            //    {
                            //        columnValueList.Add(dataRow[dataColumn.ColumnName]);
                            //    }

                            //    dictionary.Add(dataColumn.ColumnName, columnValueList);
                            //}
                            data.data = table;
                        }
                        command.Parameters.Clear();
                        connection.Close();
                        return data;
                    }
                }
            }
            catch
            {
                data.success = false;

            }
            return data;

        }
    }
}
