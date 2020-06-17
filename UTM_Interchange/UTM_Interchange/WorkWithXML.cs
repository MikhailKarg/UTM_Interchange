using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace UTM_Interchange
{
    public class WorkWithXML
    {
        public void ParseAndSafeResponseFromUTM(string responseFromUTM, Guid rowId, WorkWithDB workWithDB) // after sended XMLContent to UTM
        {
            if (responseFromUTM != null)
            {
                string ReplyId;

                try
                {
                    //parse response from UTM
                    XDocument xdoc = XDocument.Parse(responseFromUTM);
                    ReplyId = xdoc.Root.Element("url")?.Value;

                    //update buffer table
                    if (ReplyId != null | ReplyId != "")
                        workWithDB.UpdateBuffer(rowId, ReplyId);                     
                }
                catch (Exception ex)
                {
                    Log log = new Log(ex);
                }
            }
        }
        public List<UTM_Data> ParseResponseFromUTM(string responseFromUTM) // list replyId and URL for load Ticket from UTM
        {
            List<UTM_Data> listFromUtm = new List<UTM_Data>();

            if (responseFromUTM != null)
            {
                try
                {
                    XDocument xdoc = XDocument.Parse(responseFromUTM);

                    foreach(var i in xdoc.Element("A").Elements("url"))
                    {
                        XAttribute AttrReplyId = i.Attribute("replyId");
                        string replyId = null;

                        if (AttrReplyId != null)
                        {
                            replyId = AttrReplyId.Value;
                        }

                        string url = i.Value;

                        listFromUtm.Add(new UTM_Data(url, replyId));
                    }
                }
                catch (Exception ex)
                {
                    Log log = new Log(ex);
                }
            }

            return listFromUtm;
        }
        public string GetTicketDocType(string XMLContent)
        {
            string docType = null;

            if (XMLContent != null & XMLContent != "")
            {
                try
                {
                    XDocument xdoc = XDocument.Parse(XMLContent);
                    List<XNamespace> namespaces = new List<XNamespace>() { "http://fsrar.ru/WEGAIS/WB_DOC_SINGLE_01", "http://fsrar.ru/WEGAIS/Ticket" };

                    docType = xdoc.Root.Element(namespaces[0] + "Document").Element(namespaces[0] + "Ticket").Element(namespaces[1] + "DocType").Value.ToString();
                }
                catch (Exception ex)
                {
                    Log log = new Log(ex);
                }
            }

            return docType;
        }
        public static string GetDateFromXml(string XMLContent)
        {
            string TicketDate = null;

            if (XMLContent != null & XMLContent != "")
            {
                try
                {
                    XmlDocument xdocument = new XmlDocument();
                    xdocument.LoadXml(XMLContent);
                    XmlNodeList ndList = xdocument.GetElementsByTagName("ns:Ticket");

                    foreach (XmlNode node in ndList)
                    {
                        TicketDate = node["tc:TicketDate"].InnerText;
                    }
                }
                catch (Exception ex)
                {
                    Log log = new Log(ex);
                }
            } 
           
            return TicketDate;
        }
    }
}
