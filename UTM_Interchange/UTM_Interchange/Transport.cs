using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UTM_Interchange
{
   public class Transport
    {
        public static List<Task> SendXMLToUTM()
        {
            List<Task> tasksList = new List<Task>();
            List<UTM> utmList = WorkWithDB.GetActiveUTM();         

            int TimeOut = Convert.ToInt32(ConfigurationManager.AppSettings.Get("HTTPTimeout")); // httpTimeout       
            if (TimeOut == 0) TimeOut = 20000;

            foreach (var u in utmList)
            {
                List<UTM_Data> utmDataList = WorkWithDB.GetNewData(u.UTMId);

                tasksList.Add(AsyncSend(utmDataList, TimeOut));
            }
            return tasksList;
        }
        public static List<Task> GetXMLFromUTM()
        {
            List<Task> tasksList = new List<Task>();
            List<UTM> utm = WorkWithDB.GetActiveUTM();

            int TimeOut = Convert.ToInt32(ConfigurationManager.AppSettings.Get("HTTPTimeout")); //httpTimeout\
            if (TimeOut == 0) TimeOut = 20000;

            foreach (var u in utm)
            {
                tasksList.Add(AsyncGet(u, TimeOut));
            }
            return tasksList;
        }
        public static void Send(List<UTM_Data> utmDataList, int timeOut)
        {
            WorkWithXML workWithXML = new WorkWithXML();
            WorkWithDB workWithDB = new WorkWithDB();

            foreach (var utmData in utmDataList)
            {             
                HttpWebRequest httpWebRequest;

                try
                {
                    string sendingStatus = ConfigurationManager.AppSettings.Get("SendingStatus");
                    if (sendingStatus == null || sendingStatus == "") sendingStatus = "Sending";

                    workWithDB.SetStatus(utmData.RowId, sendingStatus);

                    string responseFromServer = null;
                    string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

                    string fileExtension = ConfigurationManager.AppSettings.Get("FileExtension");
                    if (fileExtension == null || fileExtension == "") fileExtension = ".xml";

                    string path = ConfigurationManager.AppSettings.Get("PathToTempFiles");
                    if (path == null || path == "") path = Path.GetTempPath();

                    string filePath = path + utmData.RowId + fileExtension;

                    using (StreamWriter streamWriter = new StreamWriter(filePath, false, Encoding.UTF8)) // write file to Temp catalog
                    {
                        streamWriter.WriteLine(utmData.XMLContent);
                    }

                    httpWebRequest = (HttpWebRequest)WebRequest.Create(utmData.URL);
                    httpWebRequest.Timeout = Convert.ToInt32(timeOut);
                    httpWebRequest.ContentType = "multipart/form-data; boundary=" + boundary;
                    httpWebRequest.Method = "POST";
                    httpWebRequest.KeepAlive = true;
                    httpWebRequest.Credentials = CredentialCache.DefaultCredentials;

                    //request to UTM
                    using (Stream requestStream = httpWebRequest.GetRequestStream())
                    {
                        byte[] boundaryBytesBegin = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                        requestStream.Write(boundaryBytesBegin, 0, boundaryBytesBegin.Length);

                        byte[] headersBytes = Encoding.UTF8.GetBytes(string.Format($"Content-Disposition: form-data; name=\"xml_file\"; filename=\"{filePath}\"\r\nContent-Type: text/xml\r\n\r\n"));
                        requestStream.Write(headersBytes, 0, headersBytes.Length);

                        byte[] bytesXMLContent = Encoding.UTF8.GetBytes(utmData.XMLContent);
                        requestStream.Write(bytesXMLContent, 0, bytesXMLContent.Length);

                        byte[] boundaryBytesEnd = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                        requestStream.Write(boundaryBytesEnd, 0, boundaryBytesEnd.Length);
                    }

                    HttpWebResponse httpWebResponse;

                    //response from UTM
                    using (httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                    {
                        responseFromServer = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();
                    }
                    httpWebRequest = null;

                    //parse response and update buffer table 
                    workWithXML.ParseAndSafeResponseFromUTM(responseFromServer, utmData.RowId, workWithDB);

                    //delete temp xml file
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch (Exception ex)
                {
                    Log log = new Log(ex);

                    string errorStatus = ConfigurationManager.AppSettings.Get("ErrorStatus");
                    if (errorStatus == null || errorStatus == "") errorStatus = "Error";

                    workWithDB.SetStatus(utmData.RowId, errorStatus);

                    httpWebRequest = null;
                }
            }
        }
        private static async Task AsyncSend(List<UTM_Data> utmDataList, int timeOut)
        {
            await Task.Run(() => Send(utmDataList, timeOut));
        }
        public static void Get(UTM utm, int timeOut)
        {
            try
            {
                if (utm.IsActive)
                {
                    WorkWithXML workWithXML = new WorkWithXML();
                    List<UTM_Data> listFromUtm = null; //all files from UTM

                    string pathToOutFilesFromUTM = ConfigurationManager.AppSettings.Get("pathToOutFilesFromUTM");
                    if (pathToOutFilesFromUTM == null || pathToOutFilesFromUTM == "") pathToOutFilesFromUTM = "/opt/out";

                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(utm.URL + pathToOutFilesFromUTM);
                    httpWebRequest.Method = "GET";
                    httpWebRequest.Timeout = Convert.ToInt32(timeOut);

                    using (StreamReader streamReader = new StreamReader(httpWebRequest.GetResponse().GetResponseStream(), Encoding.UTF8))
                    {
                        string responseFromUTM = streamReader.ReadToEnd(); // xml with list of all tickets on UTM
                        listFromUtm = workWithXML.ParseResponseFromUTM(responseFromUTM); //get list all files on UTM
                        listFromUtm.Reverse();
                    }

                    httpWebRequest = null;

                    foreach (var y in listFromUtm)
                    {
                        string xmlContent = null;

                        HttpWebRequest httpWebRequestGetTicket = (HttpWebRequest)WebRequest.Create(y.URL);
                        httpWebRequestGetTicket.Method = "GET";
                        httpWebRequestGetTicket.Timeout = Convert.ToInt32(timeOut);

                        using (StreamReader streamReader = new StreamReader(httpWebRequestGetTicket.GetResponse().GetResponseStream(), Encoding.UTF8))
                        {
                            xmlContent = streamReader.ReadToEnd();
                        }

                        httpWebRequestGetTicket = null;

                        y.XMLContent = xmlContent;
                        y.UTMId = utm.UTMId;
        
                        string DocType = null;

                        if (y.ExchangeTypeCode.ToUpper() == "TICKET")
                            DocType = workWithXML.GetTicketDocType(y.XMLContent);
                        else
                            DocType = y.ExchangeTypeCode;


                        if (y.Error != 1 & DocTypeDeletePossibility(DocType))
                        {
                            y.InsertTicket(); // insert into UTM_Data

                            HttpWebRequest httpWebRequestDeleteTicket = (HttpWebRequest)WebRequest.Create(y.URL); //delete ticket from UTM
                            HttpWebResponse httpWebResponseDeleteTicket = null;

                            httpWebRequestDeleteTicket.Timeout = Convert.ToInt32(timeOut);
                            httpWebRequestDeleteTicket.Method = "DELETE";
                            httpWebResponseDeleteTicket = (HttpWebResponse)httpWebRequestDeleteTicket.GetResponse();

                            httpWebRequestDeleteTicket = null;
                            httpWebResponseDeleteTicket = null;
                        }                      
                    }
                }
            }
            catch(Exception ex)
            {
                Log log = new Log(ex);
            }
        }
        private static async Task AsyncGet(UTM utm, int timeOut)
        {
            await Task.Run(() => Get(utm, timeOut));
        }
        public static void DeleteAsiiuTicketFromUTM()
        {
            List<UTM> utmList = WorkWithDB.GetActiveUTM();

            try
            {
                foreach (var utm in utmList)
                {
                    if (utm.IsActive & UTMDeleteTicketPossibility(utm.UTMId))
                    {
                        string pathToOutFilesFromUTM = ConfigurationManager.AppSettings.Get("pathToTicket");

                        int timeOut = Convert.ToInt32(ConfigurationManager.AppSettings.Get("HTTPTimeout")); // httpTimeout
                        if (timeOut == 0) timeOut = 20000;

                        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(utm.URL + pathToOutFilesFromUTM);
                        httpWebRequest.Method = "GET";
                        httpWebRequest.Timeout = Convert.ToInt32(timeOut);

                        WorkWithXML workWithXML = new WorkWithXML();
                        List<UTM_Data> listTicketFromUtm = null; // all files on UTM

                        using (StreamReader streamReader = new StreamReader(httpWebRequest.GetResponse().GetResponseStream(), Encoding.UTF8))
                        {
                            string responseFromUTM = streamReader.ReadToEnd(); // xml with list of all tickets on UTM
                            listTicketFromUtm = workWithXML.ParseResponseFromUTM(responseFromUTM); // get list all files on UTM
                        }

                        httpWebRequest = null;

                        foreach (var linkticket in listTicketFromUtm)
                        {
                            string Ticket = null;

                            HttpWebRequest httpWebRequestGetTicket = (HttpWebRequest)WebRequest.Create(linkticket.URL);
                            httpWebRequestGetTicket.Method = "GET";
                            httpWebRequestGetTicket.Timeout = Convert.ToInt32(timeOut);
                       
                            using (StreamReader streamReader = new StreamReader(httpWebRequestGetTicket.GetResponse().GetResponseStream(), Encoding.UTF8))
                            {
                                Ticket = streamReader.ReadToEnd();
                            }

                            httpWebRequestGetTicket = null;

                            linkticket.XMLContent = Ticket;
                            linkticket.UTMId = utm.UTMId;

                            string docType = workWithXML.GetTicketDocType(linkticket.XMLContent);

                            if (!DocTypeDeletePossibility(docType))
                            {
                                DateTime TicketDate = DateTime.Parse(WorkWithXML.GetDateFromXml(linkticket.XMLContent));
                                DateTime NowDateTime = DateTime.Now;
                                TimeSpan TicketOld = NowDateTime.Subtract(TicketDate);

                                string ticketTimeSpan = ConfigurationManager.AppSettings.Get("ticketTimeSpan");
                                
                                if (TicketOld > TimeSpan.Parse(ticketTimeSpan))
                                {
                                    HttpWebRequest httpWebRequestDelete = null;
                                    HttpWebResponse httpWebResponseDelete = null;

                                    try
                                    {
                                        httpWebRequestDelete = (HttpWebRequest)WebRequest.Create(linkticket.URL);
                                        httpWebRequestDelete.Method = "DELETE";
                                        httpWebResponseDelete = (HttpWebResponse)httpWebRequestDelete.GetResponse();
                                    }
                                    finally
                                    {
                                        httpWebRequestDelete = null;
                                        httpWebResponseDelete = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Log log = new Log(ex);
            }
        } // delete Asiiu's tickets from UTM
        private static bool DocTypeDeletePossibility(string DocTypeCode)
        {
            bool isDelete = true;

            try
            {
                string[] DocTypeCodes = ConfigurationManager.AppSettings.Get("listOfDocTypeCodeThatDontDelete").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var i in DocTypeCodes)
                {
                    if (i.ToUpper() == DocTypeCode.ToUpper())
                        isDelete = false;
                }
            }
            catch(Exception ex)
            {
                Log log = new Log(ex);
                isDelete = false;
            }
            
            return isDelete;
        }
        private static bool UTMDeleteTicketPossibility(int UTMId)
        {
            bool isDelete = false;

            try
            {
                string[] UTMIdArray = ConfigurationManager.AppSettings.Get("listUTMIdForDeleteTicket").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var i in UTMIdArray)
                {
                    if (Convert.ToInt32(i) == UTMId)
                        isDelete = true;
                }
            }
            catch(Exception ex)
            {
                Log log = new Log(ex);
            }

            return isDelete;
        }
    }
}
