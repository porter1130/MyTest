using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Text.RegularExpressions;

namespace CA
{
    class BaseTaskStep
    {
        private string _title = "{0}'s {1} needs {2}";
        private string _approveTaskTilte;

        public string ApproveTaskTilte
        {
            get { return this._approveTaskTilte; }
            set { this._approveTaskTilte = value; }
        }
        private string _confirmTaskTilte;

        public string ConfirmTaskTilte
        {
            get { return this._confirmTaskTilte; }
            set { this._confirmTaskTilte = value; }
        }
        private string _checkTaskTilte;

        public string CheckTaskTilte
        {
            get { return this._checkTaskTilte; }
            set { this._checkTaskTilte = value; }
        }
        private string _completeTaskTilte = "Please complete {0}";

        public string CompleteTaskTilte
        {
            get { return this._completeTaskTilte; }
            set { this._completeTaskTilte = value; }
        }

        private string _url = "/_Layouts/CA/WorkFlows/{0}/{1}";
        private string _approveUrl;

        public string ApproveUrl
        {
            get { return this._approveUrl; }
            set { this._approveUrl = value; }
        }
        private string _confirmUrl;

        public string ConfirmUrl
        {
            get { return this._confirmUrl; }
            set { this._confirmUrl = value; }
        }
        private string _checkUrl;

        public string CheckUrl
        {
            get { return this._checkUrl; }
            set { this._checkUrl = value; }
        }
        private string _editUrl;

        public string EditUrl
        {
            get { return this._editUrl; }
            set { this._editUrl = value; }
        }


        public BaseTaskStep(MOSSContext context)
        {
            SPListItem item = context.CurrItem;
            SPUser user = context.ApplicantSPUser;

            this._approveTaskTilte = string.Format(this._title, user.Name, context.WfName, "approval");
            this._confirmTaskTilte = string.Format(this._title, user.Name, context.WfName, "confirm");
            this._checkTaskTilte = string.Format(this._title, user.Name, context.WfName, "approval");
            this._completeTaskTilte = string.Format(this._completeTaskTilte, context.WfName);

            this._approveUrl = string.Format(this._url, context.WfName, "ApproveForm.aspx");
            this._confirmUrl = string.Format(this._url, context.WfName, "ConfirmForm.aspx");
            this._checkUrl = string.Format(this._url, context.WfName, "CheckForm.aspx");
            this._editUrl = string.Format(this._url, context.WfName, "EditForm.aspx");
        }




    }
}
