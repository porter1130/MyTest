using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Diagnostics;
using System.IO;

namespace ConsoleApp.Module
{
    class SystemEntry
    {
        private static string InfoLogUNCPath = @"C:\TEMP\";

        internal static void LogEventInfo(string info)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                string sourceName = @"C&A";
                string logName = @"EWF";

                if (EventLog.SourceExists(sourceName))
                {

                    string oldLogName = EventLog.LogNameFromSourceName(sourceName, System.Environment.MachineName);
                    if (!oldLogName.Equals(logName))
                    {
                        EventLog.Delete(oldLogName);
                    }
                }
                
                if (!EventLog.Exists(logName))
                {
                    EventLog.CreateEventSource(sourceName, logName);
                }

                EventLog myLog = new EventLog();
                myLog.Source = sourceName;
                myLog.Log = logName;

                myLog.WriteEntry(info, EventLogEntryType.Information);

            });

        }

        internal static void LogInfoToLogFile(string FriendlyMsg, string ActualMsg, string MethodName)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    string path = InfoLogUNCPath;
                    // check if directory exists
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    path = path + DateTime.Today.ToString("dd-MMM-yy") + ".log";
                    // check if file exist
                    if (!File.Exists(path))
                    {
                        File.Create(path).Dispose();
                    }
                    // log the Info now
                    using (StreamWriter writer = File.AppendText(path))
                    {
                        string Info = "\r\nLog written at : " + DateTime.Now.ToString() +
                                       "\r\nContractID/BlockName : " + MethodName +
                                       "\r\nFriendly Message : " + FriendlyMsg +
                                       "\r\n\r\nInfo Details \r\n------------------------------\r\nMessage : " + ActualMsg;

                        writer.WriteLine(Info);
                        writer.WriteLine("==========================================");
                        writer.Flush();
                        writer.Close();
                    }
                });
            }
            catch
            {
                //If there is Info when writing Info log, the system can't record the Info any more. So the exception has not been recorded here.
                //throw;
            }
        }

    }
}
