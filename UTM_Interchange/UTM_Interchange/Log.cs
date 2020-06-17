using System;
using System.Configuration;
using System.IO;
using System.Text;

namespace UTM_Interchange
{
    public class Log
    {
        public Log(Exception ex)
        {
            LogPath = ConfigurationManager.AppSettings.Get("LogPath");

            if(LogPath == null | LogPath == "")
            {
                LogPath = Path.GetTempPath();
            }

            LogPath += "ExchangeUTMServiceLog_" + DateTime.Today.ToShortDateString() + ".txt";
            LogException(ex);
        }
        public Log(string entry)
        {
            LogPath = ConfigurationManager.AppSettings.Get("LogPath");

            if (LogPath == null | LogPath == "")
            {
                LogPath = Path.GetTempPath();
            }

            LogPath += "ExchangeUTMServiceLog_" + DateTime.Today.ToShortDateString() + ".txt";
            LogEntry(entry);
        }
        string LogPath { get; set; }
        private void LogException(Exception ex)
        {
            using (StreamWriter sw = new StreamWriter(LogPath, true, Encoding.UTF8))
            {
                sw.Write("******************** " + DateTime.Now);
                sw.WriteLine(" ********************");

                if (ex.InnerException != null)
                {
                    sw.Write("Inner Exception Type: ");
                    sw.WriteLine(ex.InnerException.GetType().ToString());
                    sw.Write("Inner Exception: ");
                    sw.WriteLine(ex.InnerException.Message);
                    sw.Write("Inner Source: ");
                    sw.WriteLine(ex.InnerException.Source);
                    if (ex.InnerException.StackTrace != null)
                        sw.WriteLine("Inner Stack Trace: ");
                    sw.WriteLine(ex.InnerException.StackTrace);
                }
                sw.Write("Exception Type: ");
                sw.WriteLine(ex.GetType().ToString());
                sw.WriteLine("Exception: " + ex.Message);
                sw.WriteLine("Source: " + ex.Source);
                sw.WriteLine("Stack Trace: ");
                if (ex.StackTrace != null)
                    sw.WriteLine(ex.StackTrace);
                sw.WriteLine();
            }
        }
        private void LogEntry(string entry)
        {
            using (StreamWriter sw = new StreamWriter(LogPath, true, Encoding.UTF8))
            {
                sw.Write("******************** " + DateTime.Now);
                sw.WriteLine(" ********************");

                sw.Write("Action: ");
                sw.WriteLine(entry);

                sw.WriteLine();
            }
        }
    }
}

