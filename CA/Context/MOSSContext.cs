using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Web.UI;
using Microsoft.SharePoint.Workflow;

namespace CA
{
    class MOSSContext
    {
        private static MOSSContext _context = null;

        private SPWeb _currweb = null;

        public SPWeb Currweb
        {
            get { return _currweb; }
            set { _currweb = value; }
        }

        private SPList _currList;

        public SPList CurrList
        {
            get { return _currList; }
            set { _currList = value; }
        }

        private SPListItem _currItem;

        public SPListItem CurrItem
        {
            get { return _currItem; }
            set { _currItem = value; }
        }

        private string _wfName;

        public string WfName
        {
            get { return _wfName; }
            set { _wfName = value; }
        }

        private WorkflowVarableValues _workflowVariables;

        private SPUser _applicantSPUser;

        public SPUser ApplicantSPUser
        {
            get { return _applicantSPUser; }
            set { _applicantSPUser = value; }
        }

        public HtmlTextWriter HtmlWriter;

        public string MailContent { get; set; }

        private MOSSContext(SPWeb web)
        {
            _currweb = web;
            this._workflowVariables = new WorkflowVarableValues();
        }

        public static MOSSContext GetInstance(SPWeb web)
        {
            if (_context == null)
            {
                _context = new MOSSContext(web);
            }

            return _context;
        }

        public SPList GetListByName(string listName)
        {
            return _currweb.Lists[listName];
        }

        public void UpdateWorkflowVariable(string key, object value)
        {
            this._workflowVariables[key] = value;
        }

        internal string SerializeWorkflowVariable()
        {
            return SerializeUtil.Serialize(this._workflowVariables);
        }


        internal string GetLastedWorkflowName()
        {
            string workflowName = string.Empty;
            SPList list = this._currList;

            if (list == null)
            {
                throw new Exception("Please set workflow List at first!");
            }
            else if (list.WorkflowAssociations.Count > 0)
            {
                workflowName = list.WorkflowAssociations[list.WorkflowAssociations.Count - 1].Name;
            }

            return workflowName;
        }
    }
}
