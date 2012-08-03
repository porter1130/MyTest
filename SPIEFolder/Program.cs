using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.IO;
using System.Text.RegularExpressions;

namespace SPIEFolder
{
    class Program
    {
        private const string InfoLogUNCPath = @"c:\temp\Records\";
        private static List<string> ExistedFilesList = new List<string>();
        private static bool isOverwrite = false;
        private static bool isAll = false;
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                string sFormat = "{0,5}{1,-20}{2}\n";
                StringBuilder sb = new StringBuilder();
                sb.Append("\nSYNTAX:SPIEFolder.exe <FolderURL> <Export Path> [-overwrite|-all] [import|export]\r\n");
                sb.Append("\nWhere:\n\n");
                sb.AppendFormat(sFormat, "", "<FolderURL>", "The Web URL where the Doc Library folder exists");
                sb.AppendFormat(sFormat, "", "<Export Path>", "The Path where the docs are exported to");
                sb.AppendFormat(sFormat, "", "-overwrite", "Overwrite the existed local files; Otherwise, continue the latest process.");
                sb.AppendFormat(sFormat, "", "-all", "Download all existed Doc Libraries");
                sb.AppendFormat(sFormat, "", "[import|export]", "Import or export (defaults to export)");
                Console.WriteLine(sb.ToString());
                return;
            }

            bool bImport = false;
            string strDirectory;

            try
            {
                string folderURL = args[0];
                string pattern = @"^http(s?)://.*?(?<folderURL>/.*)$";
                Regex regex = new Regex(pattern);
                Match match = regex.Match(folderURL);

                //foreach (Group group in match.Groups) {
                //    Console.WriteLine(group.Value);
                //}
                string folderUrl = match.Groups["folderURL"].Value;

                //Console.WriteLine("Site URL:{0} Web Title:{1} List Url:{2}", siteUrl, webUrl,listUrl);

                using (SPSite site = new SPSite(args[0]))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        web.Lists.IncludeRootFolder = true;
                        SPList currList = web.GetList(folderUrl);

                        strDirectory = args[1].TrimEnd('\\');

                        if (args.Length >= 3)
                        {
                            if (args.Contains("-overwrite"))
                            {
                                isOverwrite = true;
                            }

                            if (args.Contains("-all"))
                            {
                                isAll = true;
                            }
                        }

                        if (bImport)
                        {
                            ImportFromFileSystemFolder(web.GetFolder(folderUrl), strDirectory);
                        }
                        else
                        {
                            if (!isAll)
                            {
                                ExportToFileSystemFolder(web.GetFolder(folderUrl), strDirectory);
                            }
                            else
                            {
                                foreach (SPList list in web.Lists)
                                {
                                    if (list.BaseType == SPBaseType.DocumentLibrary)
                                    {
                                        SPDocumentLibrary docLibrary = list as SPDocumentLibrary;
                                        if (!docLibrary.Title.StartsWith("_", StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            Console.WriteLine("[Processing the Document library {0}...]", docLibrary.Title);
                                            ExportToFileSystemFolder(docLibrary.RootFolder, strDirectory);
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {

                LogError(Path.GetTempPath(), string.Format("Error Message:{0} Stack Trace:{1}", ex.Message, ex.StackTrace));
            }

        }

        private static void ExportToFileSystemFolder(SPFolder folder, string strDirectory)
        {
            if (!isOverwrite)
            {
                GetExistedFilesList(InfoLogUNCPath);
            }

            ExtractFolder(folder, strDirectory);

        }

        private static long ExtractFolder(SPFolder folder, string strExportPath)
        {
            Console.WriteLine("  Processing Folder {0}...", folder.ServerRelativeUrl);

            string strPath = null;
            long IFolderSize = 0;

            if (!folder.Name.Equals("MarketingCommunication", StringComparison.CurrentCultureIgnoreCase))
            {
                if (strExportPath != null)
                {
                    strPath = CheckPathAndCreate(strExportPath, folder);
                }

                if (strPath != null)
                {
                    foreach (SPFile file in folder.Files)
                    {
                        try
                        {
                            string fileAbsoluteUrl = file.ParentFolder.ParentWeb.Site.MakeFullUrl(file.ServerRelativeUrl);
                            if (ExistedFilesList.Count > 0
                                && ExistedFilesList.Contains(fileAbsoluteUrl))
                            {
                                //Console.WriteLine("Jump the {0}", fileAbsoluteUrl);
                                continue;
                            }

                            string sFileLoc = string.Format("{0}\\{1}", strPath, file.Name);

                            int size = 10 * 1024;
                            using (Stream stream = file.OpenBinaryStream())
                            {
                                using (FileStream fs = new FileStream(sFileLoc, FileMode.OpenOrCreate, FileAccess.Write))
                                {
                                    byte[] buffer = new byte[size];
                                    while (stream.Read(buffer, 0, buffer.Length) > 0)
                                    {
                                        fs.Write(buffer, 0, buffer.Length);
                                    }
                                }
                            }

                            Console.WriteLine("    Exporting file {0} to {1}", file.Name, sFileLoc);
                            LogInfoToRecordFile(InfoLogUNCPath, string.Format("Exporting file [{0}] to {1}",
                                                                            fileAbsoluteUrl,
                                                                            sFileLoc));

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("The Exception occurs on {0}, maybe its file type doesn't support.", file.Name);
                            throw ex;
                        }

                    }

                    foreach (SPFolder subfolder in folder.SubFolders)
                    {
                        if (!subfolder.Name.Equals("Forms"))
                        {
                            ExtractFolder(subfolder, strExportPath);
                        }
                    }

                }
            }
            return IFolderSize;
        }

        private static void GetExistedFilesList(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (directoryInfo.GetFiles().Count() > 0)
            {
                var lastedRecord = (from f in directoryInfo.GetFiles()
                                    orderby f.LastWriteTime descending
                                    select f).First();

                using (StreamReader sr = new StreamReader(lastedRecord.FullName))
                {
                    string content = sr.ReadToEnd();
                    string pattern = @"Exporting file \[(.*)\] to";
                    Regex regex = new Regex(pattern);
                    //Console.WriteLine(regex.Matches(content).Count);

                    foreach (Match match in regex.Matches(content))
                    {
                        //Console.WriteLine(match.Groups[1].ToString());
                        ExistedFilesList.Add(match.Groups[1].ToString());
                    }
                }
            }

        }

        internal static void LogInfoToRecordFile(string path, string ActualMsg)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
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
                        string Info = string.Format("\r\nLog written at : {0}\r\nInfo Details:\r\n {1} ", DateTime.Now.ToString(), ActualMsg);
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

        private static void LogError(string path, string ActualMsg)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
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
                        string Info = string.Format("\r\nLog written at : {0}\r\nInfo Details:\r\n {1} ", DateTime.Now.ToString(), ActualMsg);
                        writer.WriteLine(Info);
                        writer.WriteLine("==========================================");
                        writer.Flush();
                        writer.Close();
                    }
                });

                Console.WriteLine("More Details, please refer to {0}", path);
            }
            catch
            {
                //If there is Info when writing Info log, the system can't record the Info any more. So the exception has not been recorded here.
                //throw;
            }
        }

        private static string CheckPathAndCreate(string strExportPath, SPFolder folder)
        {
            string strPath;
            strPath = string.Format("{0}\\{1}", strExportPath, folder.Url.Replace("/", "\\"));
            //strPath = "d:\\Export\\IT\\IT\\Project\\Phase 2 - Rel 1 Implementation\\90-Team Folder\\Difei\\ARS\\ARS global documents\\FY06 ARS Process Model\\6 - Enterprise Management\\6.01 - Master Data Maintenance - Retail Organization\\6.1.03 CMD Company Code and Purchase Organiz\\test";
            if (!Directory.Exists(strPath))
            {
                try
                {
                    //if (strPath.ToCharArray().Length < 248)
                    //{
                    Directory.CreateDirectory(strPath);
                    //}
                }
                catch (PathTooLongException pathTooLongException) {
                    LogInfoToRecordFile(InfoLogUNCPath, string.Format("The specific folder <{0}> path {1} is too long!",
                                                                        folder.Url,
                                                                        strPath));

                    strPath = null;
                }
            }

            return strPath;
        }

        private static void CreateLongPathDirectory(string strPath)
        {
            string subStrPath = strPath.Substring(0, 247);
            subStrPath = subStrPath.Substring(0, subStrPath.LastIndexOf("\\"));
            Directory.CreateDirectory(subStrPath);

            string leftStrPath = strPath.Substring(subStrPath.Length, strPath.Length - subStrPath.Length);
            char[] splitChars = new char[] { '\\', '\\' };
            string[] directories = leftStrPath.TrimStart(splitChars).Split(splitChars, StringSplitOptions.None);

            string currPath = subStrPath;
            DirectoryInfo directoryInfo = new DirectoryInfo(currPath);
            directoryInfo.CreateSubdirectory(leftStrPath.TrimStart(splitChars));

            //strPath.le

            //if (subStrPath.ToCharArray().Length < 248)
            //{
            //    Directory.CreateDirectory(subStrPath);
            //    CreateSubDirectory(
            //}
            //else {
            //    CreateLongPathDirectory(subStrPath);
            //}
        }

        private static void ImportFromFileSystemFolder(SPFolder folder, string strDirectory)
        {
            throw new NotImplementedException();
        }
    }
}
