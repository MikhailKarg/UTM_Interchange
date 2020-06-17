using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace UTM_Interchange
{
    public class UTM_Scanner
    {
        public static void ScanningUTM()
        {
            List<UTM> UTM_List = WorkWithDB.GetUTM();
            bool isActive = false;

            foreach(var u in UTM_List)
            {
                isActive = ConnectionAttempt(u.URL);

                WorkWithDB.UpdateUTMState(u.UTMId, isActive);
            }
        }
        private static bool ConnectionAttempt(string url)
        {
            bool isActive = false;
            HttpWebRequest httpWebRequest;

            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

                HttpWebResponse httpWebResponse;
                httpWebResponse = null;

                string responseFromServer = null;

                using (httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    responseFromServer = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();
                }

                if (responseFromServer != null & responseFromServer != "")
                {
                    isActive = true;
                }

                httpWebRequest = null;
            }
            catch(Exception ex)
            {
                httpWebRequest = null;
                //Log log = new Log(ex);
            }

            return isActive;
        }
    }
}
