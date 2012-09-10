using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Workflow;

namespace CA
{
    abstract class BaseWF
    {
        protected SPWeb _web;
        protected MOSSContext _context;

        private int _itemId;

        public int ItemId
        {
            get { return _itemId; }
            set { _itemId = value; }
        }

        public BaseWF(MOSSContext context)
        {
            this._context = context;
            this._web = context.Currweb;
        }

        public abstract void StartWorkflowInstance(int itemId);

        public abstract void RetryingWF(int taskItemId);

        public abstract void ReassignWorkflowTask(int itemId, int taskId, int reassignUserId);

        public virtual SPWorkflow StartWorkflowInstance(int itemId, string eventData) { return null; }

        public virtual bool FindLockedWorkflow() { return false; }

        public virtual void AutoUnLock() { }

        public virtual void SendAlertMail() { }

        public virtual void CancelWorkflowInstance(int itemId) { }

        public virtual void UnLockSpecificWF(int itemId) { }

    }
}
