using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace UTM_Interchange
{
    public class WorkWithDB
    {
        public static List<UTM_Data> GetNewData(int utmId)
        {
            List<UTM_Data> utmDataList = new List<UTM_Data>(); //new documents

            string sqlExpression = ConfigurationManager.AppSettings.Get("GetNewData");

            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    command.CommandType = CommandType.StoredProcedure;

                    SqlParameter UTMId = new SqlParameter
                    {
                        ParameterName = "@UTMId",
                        Value = utmId
                    };
                    command.Parameters.Add(UTMId);

                    int sqlCommandTimeout = Convert.ToInt32(ConfigurationManager.AppSettings.Get("SQLCommandTimeout"));
                    command.CommandTimeout = sqlCommandTimeout;
            
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
        public void UpdateBuffer(Guid rowId, string replyId)
        {
            string sqlExpression = ConfigurationManager.AppSettings.Get("UpdateBuffer");

            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    command.CommandType = CommandType.StoredProcedure;

                    int sqlCommandTimeout = Convert.ToInt32(ConfigurationManager.AppSettings.Get("SQLCommandTimeout"));
                    command.CommandTimeout = sqlCommandTimeout;

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
        public static List<UTM> GetActiveUTM()
        {
            List<UTM> utm = new List<UTM>();

            string sqlExpression = ConfigurationManager.AppSettings.Get("GetActiveUTM");

            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    command.CommandType = CommandType.StoredProcedure;

                    int sqlCommandTimeout = Convert.ToInt32(ConfigurationManager.AppSettings.Get("SQLCommandTimeout"));
                    command.CommandTimeout = sqlCommandTimeout;

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string fsrar_id = (string)reader["FSRAR_Id"];
                            string taxCode = (string)reader["TaxCode"];
                            string taxReason = (string)reader["TaxReason"];
                            string url = (string)reader["URL"];
                            int utmId = (int)reader["UTMId"];
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

                            utm.Add(new UTM(fsrar_id, taxCode, taxReason, url, utmId, description, isActive));
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
        public void SetStatus(Guid rowId, string status)
        {
            string sqlExpression = ConfigurationManager.AppSettings.Get("SetStatus");

            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    command.CommandType = CommandType.StoredProcedure;

                    int sqlCommandTimeout = Convert.ToInt32(ConfigurationManager.AppSettings.Get("SQLCommandTimeout"));
                    command.CommandTimeout = sqlCommandTimeout;

                    SqlParameter RowId = new SqlParameter
                    {
                        ParameterName = "@RowId",
                        Value = rowId
                    };
                    command.Parameters.Add(RowId);

                    SqlParameter Status = new SqlParameter
                    {
                        ParameterName = "@Status",
                        Value = status
                    };
                    command.Parameters.Add(Status);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
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

                    int sqlCommandTimeout = Convert.ToInt32(ConfigurationManager.AppSettings.Get("SQLCommandTimeout"));
                    command.CommandTimeout = sqlCommandTimeout;

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string fsrar_id = (string)reader["FSRAR_Id"];
                            string taxCode = (string)reader["TaxCode"];
                            string taxReason = (string)reader["TaxReason"];
                            string url = (string)reader["URL"];
                            int utmId = (int)reader["UTMId"];
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

                            utm.Add(new UTM(fsrar_id, taxCode, taxReason, url, utmId, description, isActive));
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
        public static void UpdateUTMState(int utmId, bool isActive)
        {
            string sqlExpression = ConfigurationManager.AppSettings.Get("UpdateUTMState");

            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    command.CommandType = CommandType.StoredProcedure;

                    int sqlCommandTimeout = Convert.ToInt32(ConfigurationManager.AppSettings.Get("SQLCommandTimeout"));
                    command.CommandTimeout = sqlCommandTimeout;

                    SqlParameter UTMId = new SqlParameter
                    {
                        ParameterName = "@UTMId",
                        Value = utmId
                    };
                    command.Parameters.Add(UTMId);

                    SqlParameter IsActive = new SqlParameter
                    {
                        ParameterName = "@IsActive",
                        Value = isActive
                    };
                    command.Parameters.Add(IsActive);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Log log = new Log(ex);
            }
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

                        int sqlCommandTimeout = Convert.ToInt32(ConfigurationManager.AppSettings.Get("SQLCommandTimeout"));
                        command.CommandTimeout = sqlCommandTimeout;

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
