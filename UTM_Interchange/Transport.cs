using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;

namespace UTM_Interchange
{
   public class Transport
    {
        public static void UploadXMLToUTM()
        {
            List<UTM_Data> utmDataList = WorkWithDB.GetDocument(0);
            WorkWithXML workWithXML = new WorkWithXML();

            int TimeOut;  

            foreach (var utmData in utmDataList)
            {
                try
                {
                    TimeOut = Convert.ToInt32(ConfigurationManager.AppSettings.Get("HTTPTimeout")); //httpTimeout
                    if (TimeOut == 0) TimeOut = 20000;

                    string responseFromServer = null;
                    string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

                    string path = ConfigurationManager.AppSettings.Get("PathToTempFiles");
                    if (path == null) path = Path.GetTempPath();
                    string filePath = path + DateTime.Now.Ticks.ToString("x") + ".xml";

                    using (StreamWriter streamWriter = new StreamWriter(filePath, false, Encoding.UTF8)) // write file to Temp catalog
                    {
                        streamWriter.WriteLine(utmData.XMLContent);
                    }

                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(utmData.URL);
                    HttpWebResponse httpWebResponse = null;
                    httpWebRequest.Timeout = Convert.ToInt32(TimeOut);
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

                    //response from UTM
                    using (httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                    {
                        responseFromServer = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();
                    }

                    httpWebRequest = null;

                    //parse response and update buffer table 
                    workWithXML.ParseAndSafeResponseFromUTM(responseFromServer, utmData.RowId);
                }
                catch(Exception ex)
                {
                    Log log = new Log(ex);
                }
            }
        }
        public static void GetXMLFromUTM()
        {
            try
            {
                int TimeOut = Convert.ToInt32(ConfigurationManager.AppSettings.Get("HTTPTimeout")); //httpTimeout
                if (TimeOut == 0) TimeOut = 20000;

                List<UTM> utm = WorkWithDB.GetUTM();

                foreach (var i in utm)
                {
                    if (i.IsActive)
                    {
                        WorkWithXML workWithXML = new WorkWithXML();
                        List<UTM_Data> listFromUtm = null; //all files on UTM

                        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(i.URL + "/opt/out");
                        httpWebRequest.Timeout = Convert.ToInt32(TimeOut);

                        using (StreamReader streamReader = new StreamReader(httpWebRequest.GetResponse().GetResponseStream(), Encoding.UTF8))
                        {
                            string responseFromUTM = streamReader.ReadToEnd(); // xml with list of all tickets on UTM
                            listFromUtm = workWithXML.ParseResponseFromUTM(responseFromUTM); //get list all files on UTM
                        }

                        httpWebRequest = null;

                        foreach (var y in listFromUtm)
                        {
                            string Ticket = null;

                            HttpWebRequest httpWebRequestGetTicket = (HttpWebRequest)WebRequest.Create(y.URL);
                            httpWebRequestGetTicket.Timeout = Convert.ToInt32(TimeOut);

                            using (StreamReader streamReader = new StreamReader(httpWebRequestGetTicket.GetResponse().GetResponseStream(), Encoding.UTF8))
                            {
                                Ticket = streamReader.ReadToEnd();
                            }

                            y.XMLContent = Ticket;
                            y.UTM_Id = i.Id;
                            y.InsertTicket(); // insert into UTM_Data

                            httpWebRequestGetTicket = null;

                            if (y.Error != 1)
                            {
                                HttpWebRequest httpWebRequestDeleteTicket = (HttpWebRequest)WebRequest.Create(y.URL); //delete ticket from UTM
                                httpWebRequestDeleteTicket.Timeout = Convert.ToInt32(TimeOut);
                                httpWebRequestDeleteTicket.Method = "DELETE";


                                using (StreamReader streamReader = new StreamReader(httpWebRequestDeleteTicket.GetResponse().GetResponseStream(), Encoding.UTF8))
                                {
                                    string responseFromUTM = streamReader.ReadToEnd(); //response after delete ticket from UTM
                                }

                                httpWebRequestDeleteTicket = null;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Log log = new Log(ex);
            }
        }
    }
}
