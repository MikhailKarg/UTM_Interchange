using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace UTM_Interchange
{
    public class WorkWithXML
    {
        public string ReplyId { get; set; }
        public void ParseAndSafeResponseFromUTM(string responseFromUTM, Guid rowId) // after sended XMLContent to UTM
        {
            if (responseFromUTM != null)
            {
                try
                {
                    //parse response from UTM
                    XDocument xdoc = XDocument.Parse(responseFromUTM);

                    ReplyId = xdoc.Root.Element("url")?.Value;
                  
                    //update buffer table
                    WorkWithDB.UpdateBuffer(rowId, ReplyId);                     
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
    }
}
