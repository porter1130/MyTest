using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Workflow;
using Microsoft.SharePoint.WorkflowActions;

namespace SampleWorkflow
{
    public sealed partial class Workflow1 : SequentialWorkflowActivity
    {
        public string logHistoryDescriptions;
        public string logHistoryOutcome;

        public Workflow1()
        {
            InitializeComponent();
        }

        public Guid workflowId = default(System.Guid);
        public SPWorkflowActivationProperties workflowProperties = new SPWorkflowActivationProperties();

        private void onWorkflowActivated1_Invoked(object sender, ExternalDataEventArgs e)
        {
            this.workflowId = workflowProperties.WorkflowId;
        }

        private void codeActivity_ExecuteCode(object sender, EventArgs e)
        {
            workflowProperties.Item.CopyTo(string.Format(@"{0}/{1}/{2}",
                                            workflowProperties.Web.Url,
                                            "DemoLibrary",
                                            workflowProperties.Item.File.Name));

            this.logHistoryDescriptions = string.Format("Attempted file copy of {0} From {1} To Demo Library",
                                                       workflowProperties.Item.File.Name,
                                                       workflowProperties.List.Title);
            this.logHistoryOutcome = "Successful";

        }
    }
}
