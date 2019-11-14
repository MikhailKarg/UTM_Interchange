using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace UTM_Interchange
{
    public class WorkWithDB
    {
        public static List<UTM_Data> GetDocument(int Status)
        {
            List<UTM_Data> utmDataList = new List<UTM_Data>(); //new documents

            string sqlExpression = ConfigurationManager.AppSettings.Get("GetDocument");

            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    command.CommandType = CommandType.StoredProcedure;

                    SqlParameter DocumentStatus = new SqlParameter
                    {
                        ParameterName = "@Status",
                        Value = Status
                    };
                    command.Parameters.Add(DocumentStatus);
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Guid rowId = (Guid)reader["RowId"]; 
                            string urlAdress = (string)reader["URL"]; 
                            string xmlContent = (string)reader["Content"];

                            utmDataList.Add(new UTM_Data(rowId, urlAdress, xmlContent));
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Log log = new Log(ex);
            }
        
            return utmDataList;
        }
        public static void UpdateBuffer(Guid rowId, string replyId)
        {
            string sqlExpression = ConfigurationManager.AppSettings.Get("UpdateBuffer");

            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    command.CommandType = CommandType.StoredProcedure;

                    SqlParameter ReplyId = new SqlParameter
                    {
                        ParameterName = "@ReplyId",
                        Value = replyId
                    };
                    command.Parameters.Add(ReplyId);

                    SqlParameter RowId = new SqlParameter
                    {
                        ParameterName = "@RowId",
                        Value = rowId
                    };
                    command.Parameters.Add(RowId);
                    command.ExecuteNonQuery();
                }
            }
            catch(Exception ex)
            {
                Log log = new Log(ex);
            }
        }
        public static List<UTM> GetUTM()
        {
            List<UTM> utm = new List<UTM>();

            string sqlExpression = ConfigurationManager.AppSettings.Get("GetUTM");

            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    command.CommandType = CommandType.StoredProcedure;

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string fsrar_id = (string)reader["FSRAR_Id"];
                            string taxCode = (string)reader["TaxCode"];
                            string taxReason = (string)reader["TaxReason"];
                            string url = (string)reader["URL"];
                            int id = (int)reader["Id"];
                            string description = (string)reader["Description"];
                            bool isActive;

                            if (reader["IsActive"] is null)
                            {
                                isActive = false;
                            }
                            else
                            {
                                isActive = (bool)reader["IsActive"];
                            }

                            utm.Add(new UTM(fsrar_id, taxCode, taxReason, url, id, description, isActive));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log log = new Log(ex);
            }

            return utm;
        }
        public static void InsertTicketIntoBuffer(List<UTM_Data> listFromUtm)  //old
        {
            string sqlExpression = ConfigurationManager.AppSettings.Get("InsertTicketIntoBuffer");

            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    connection.Open();

                    foreach (var i in listFromUtm)
                    {
                        SqlCommand command = new SqlCommand(sqlExpression, connection);
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter Content = new SqlParameter
                        {
                            ParameterName = "@Content",
                            Value = i.XMLContent
                        };
                        command.Parameters.Add(Content);

                        SqlParameter ReplyId = new SqlParameter
                        {
                            ParameterName = "@ReplyId",
                            Value = i.ReplyId
                        };
                        command.Parameters.Add(ReplyId);

                        SqlParameter URL = new SqlParameter
                        {
                            ParameterName = "@URL",
                            Value = i.URL
                        };
                        command.Parameters.Add(URL);

                        SqlParameter ExchangeTypeCode = new SqlParameter
                        {
                            ParameterName = "@ExchangeTypeCode",
                            Value = i.ExchangeTypeCode
                        };
                        command.Parameters.Add(ExchangeTypeCode);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Log log = new Log(ex);
            }
        }
    }
}
