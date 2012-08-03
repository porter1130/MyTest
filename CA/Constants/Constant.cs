using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace CA
{

    static class Constants
    {
        internal static int WF_Lastest_Days = Int32.Parse("-" + ConfigurationManager.AppSettings["WF_Lastest_Days"]);
        internal static string WF_Path_FieldName = ConfigurationManager.AppSettings["WF_Path_FieldName"];
    }

    static class MOSSSiteUrl
    {
        internal static string Production_Environment = ConfigurationManager.AppSettings["Production"];
        internal static string Test_Environment = ConfigurationManager.AppSettings["Test"];
        internal static string Localhost_Environment = ConfigurationManager.AppSettings["Localhost"];
    }

    static class SPFieldName
    {
        #region UnlockWorkflow
        internal const string UnlockHistory = "UnlockHistory";
        internal const string TasksHistory = "TasksHistory";
        internal const string HasException = "HasException";

        #endregion
    }
    static class SPListName
    {
        #region PADChangeRequest
        internal const string PADChangeRequest = "PADChangeRequest";
        #endregion
        #region Non Trade Supplier Setup Maintenance Workflow
        internal const string NonTradeSupplierSetupMaintenanceWorkflow = "Non Trade Supplier Setup Maintenance Workflow";
        #endregion

        #region UnLockWorklfow
        internal const string UnlockWorkflow = "UnlockWorkflow";
        #endregion

        #region Business Card Application
        internal const string BusinessCardApplication = "Business Card Application";
        #endregion

        #region Common List
        internal const string Tasks = "Tasks";
        #endregion

        #region Trave Request Workflow2
        internal const string TravelRequestWorkflow2 = "Travel Request Workflow2";
        internal const string TravelHotelInfo2 = "Travel Hotel Info2";
        internal const string TravelVehicleInfo2 = "Travel Vehicle Info2";
        internal const string TravelDetails2 = "Travel Details2";

        #endregion

        #region Travel Expense Claim Workflow
        internal const string TravelExpenseClaimDetails = "Travel Expense Claim Details";
        internal const string TravelExpenseClaim = "Travel Expense Claim";
        #endregion

        #region Travel Expense Claim For SAP Workflow
        internal const string TravelExpenseClaimDetailsForSAP = "Travel Expense Claim Details For SAP";
        internal const string TravelExpenseClaimForSAP = "Travel Expense Claim For SAP";
        #endregion
        #region Credit Card Claim Workflow
        internal const string CreditCardClaim = "Credit Card Claim Workflow";
        internal const string CreditCardBill = "Credit Card Bill";

        #endregion

        #region Purchase Request Workflow
        internal const string PurchaseRequest = "Purchase Request Workflow";
        internal const string PurchaseRequestDetails = "Purchase Request Details";
        #endregion

        #region Payment Request Workflow
        internal const string PaymentRequest = "PaymentRequestItems";
        #endregion

        #region Purchase Order Workflow
        internal const string PurchaseOrder = "Purchase Order Workflow";
        #endregion

        #region Employee Expense Claim Workflow
        internal const string EmployeeExpenseClaim = "Employee Expense Claim Workflow";
        #endregion

        #region Cash Advance Request Workflow
        internal const string CashAdvanceRequest = "CashAdvanceRequest";
        #endregion

        #region New Employee Equipment Application Workflow
        internal const string NewEmployeeEquipmentApplication = "New Employee Equipment Application";
        #endregion

        #region DemoWorkflow
        internal const string Demo = "DemoWorkflow";
        #endregion

        #region Store Sampling Workflow
        internal const string StoreSampling = "Store Sampling Workflow";
        #endregion
    }

    internal static class WorkflowName
    {
        #region NonTradeSupplierSetupMaintenanceWF2
        internal const string NonTradeSupplierSetupMaintenanceWF2 = "NonTradeSupplierSetupMaintenanceWF2";
        #endregion


        #region Store Sampling Workflow
        internal const string StoreSampling = "Store Sampling Workflow2";
        #endregion

        #region Travel Expense Claim Workflow
        internal const string TravelExpenseClaim = "Travel Expense Claim Workflow";
        #endregion

        #region Travel Request Workflow
        internal const string TravelRequestWorkflow = "Travel Request Workflow";
        internal const string TravelRequestWorkflow2 = "Travel Request Workflow2";
        internal const string TravelRequestWorkflow3 = "Travel Request Workflow3";
        #endregion

        #region New Employee Equipment Application
        internal const string NewEmployeeEquipmentApplication = "New Employee Equipment Application";
        internal const string NewEmployeeEquipmentApplication2 = "New Employee Equipment Application2";
        #endregion

        #region Credit Card Claim Workflow
        internal const string CreditCardClaim = "Credit Card Claim Workflow";
        #endregion

        #region Purchase Request Workflow
        internal const string PurchaseRequest = "Purchase Request Workflow";
        internal const string PurchaseRequest1 = "Purchase Request Workflow1";
        internal const string PurchaseRequest2 = "Purchase Request Workflow2";
        #endregion

        #region Cash Advance Request Workflow
        internal const string CashAdvanceClaim = "CashAdvanceClaimWorkFlow2";
        #endregion
    }
}
