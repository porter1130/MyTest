using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.Office.Server;
using Microsoft.Office.Server.UserProfiles;

namespace ConsoleApp.Services
{
    class UserProfileService
    {
        internal static string GetEmployeeId(string userAccount, SPWeb web)
        {
            string employeeId = string.Empty;

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite site = new SPSite(web.Site.ID))
                {
                    ServerContext context = ServerContext.GetContext(site);
                    UserProfileManager profileManager = new UserProfileManager(context);

                    if (profileManager.UserExists(userAccount))
                    {
                        UserProfile userProfile = profileManager.GetUserProfile(userAccount);
                        if (userProfile["EmployeeID"].Value != null)
                        {
                            employeeId = userProfile["EmployeeID"].Value.ToString();
                        }
                    }
                    else
                    {
                        ConsoleApp.Module.SystemEntry.LogInfoToLogFile(string.Format("There isn't user info ({0}) in ssp, please check it.", userAccount),
                                                                       string.Format("profileManager.UserExists(userAccount) return {0}", bool.FalseString),
                                                                       "GetEmployeeId");
                    }
                }
            });

            return employeeId;
        }
    }
}
