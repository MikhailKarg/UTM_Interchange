using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace UTM_Interchange
{
    public class UTM_Data
    {
        public UTM_Data(Guid rowId, string url, string xmlcontent)
        {
            RowId = rowId;
            URL = url;
            XMLContent = xmlcontent;
        }
        public UTM_Data(string url, string replyId)
        {
            URL = url;
            ReplyId = replyId;
            ExchangeTypeCode = GetExchangeTypeCode(url);
        }

        public Guid RowId { get; set; }
        public string ReplyId { get; set; }
        public string URL { get; set; }
        public string XMLContent { get; set; }
        public string ExchangeTypeCode { get; set; }
        public int UTMId { get; set; }
        public int Error { get; set; }

        private string GetExchangeTypeCode(string url)
        {
            char s = '/';

            int index = url.LastIndexOf(s);
            string interim = url.Remove(index);
            index = interim.LastIndexOf(s);

            return interim.Substring(++index);
        }
        public void InsertTicket() // new
        {
            string sqlExpression = ConfigurationManager.AppSettings.Get("InsertTicketIntoBuffer");

            try
            {
                using(SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    command.CommandType = CommandType.StoredProcedure;

                    int sqlCommandTimeout = Convert.ToInt32(ConfigurationManager.AppSettings.Get("SQLCommandTimeout"));
                    command.CommandTimeout = sqlCommandTimeout;

                    SqlParameter Content = new SqlParameter
                    {
                        ParameterName = "@Content",
                        Value = this.XMLContent
                    };
                    command.Parameters.Add(Content);

                    SqlParameter ReplyId = new SqlParameter
                    {
                        ParameterName = "@ReplyId",
                        Value = this.ReplyId
                    };
                    command.Parameters.Add(ReplyId);

                    SqlParameter URL = new SqlParameter
                    {
                        ParameterName = "@URL",
                        Value = this.URL
                    };
                    command.Parameters.Add(URL);

                    SqlParameter DocumentType = new SqlParameter
                    {
                        ParameterName = "@ExchangeTypeCode",
                        Value = this.ExchangeTypeCode
                    };
                    command.Parameters.Add(DocumentType);

                    SqlParameter UTM_Id = new SqlParameter
                    {
                        ParameterName = "@UTM_Id",
                        Value = this.UTMId
                    };
                    command.Parameters.Add(UTM_Id);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Log log = new Log(ex);
                Error = 1;
            }
        }
    }
}
