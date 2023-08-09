using Applied_WebApplication.Data;
using AppReportClass;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace Applied_WebApplication.Pages.ReportPrint
{
    [Authorize]
    public class CompanyBalancesModel : PageModel
    {
        [BindProperty]
        public Parameters Variables { get; set; }
        public DataTable MyTable { get; set; }
        public string UserName => User.Identity.Name;



        public void OnGet()
        {
            Variables = new()
            {
                ReportDate = AppRegistry.GetDate(UserName, "DueRptDate"),
                ReportType = AppRegistry.GetText(UserName, "DueRptType")
            };

            var _Filter1 = "";
            if (Variables.ReportType == "All") { _Filter1 = ""; }
            if (Variables.ReportType == "Receivable") { _Filter1 = " AND [DR] <> 0"; }
            if (Variables.ReportType == "Payable") { _Filter1 = " AND [CR] <> 0"; }


            var _ReportDate = Variables.ReportDate.AddDays(1).ToString(AppRegistry.DateYMD);
            var _COA_List = AppRegistry.GetText(UserName, "CompanyGLs");
            var _Filter = $"Date([L].[Vou_Date]) < Date('{_ReportDate}') {_Filter1}";
            MyTable = DataTableClass.GetTable(UserName, SQLQuery.CompanyBalances(_Filter, _COA_List));


        }

        #region Refresh
        public IActionResult OnPostRefresh()
        {
            AppRegistry.SetKey(UserName, "DueRptDate", Variables.ReportDate, KeyType.Date);
            AppRegistry.SetKey(UserName, "DueRptType", Variables.ReportType, KeyType.Text);

            var _ReportDate = Variables.ReportDate.AddDays(1).ToString(AppRegistry.DateYMD);
            var _COA_List = AppRegistry.GetText(UserName, "CompanyGLs");
            var _Filter = $"Date([L].[Vou_Date]) < Date('{_ReportDate}')";
            MyTable = DataTableClass.GetTable(UserName, SQLQuery.CompanyBalances(_Filter, _COA_List));
            return Page();
        }
        #endregion

        #region Receivable
        public IActionResult OnPostReceivable()
        {
            Variables.ReportType = "Receivable";
            AppRegistry.SetKey(UserName, "DueRptDate", Variables.ReportDate, KeyType.Date);
            AppRegistry.SetKey(UserName, "DueRptType", Variables.ReportType, KeyType.Text);
            return RedirectToPage();

        }

        #endregion

        #region Payable
        public IActionResult OnPostPayable()
        {
            Variables.ReportType = "Payable";
            AppRegistry.SetKey(UserName, "DueRptDate", Variables.ReportDate, KeyType.Date);
            AppRegistry.SetKey(UserName, "DueRptType", Variables.ReportType, KeyType.Text);
            return RedirectToPage();

        }
        #endregion

        #region Print
        public IActionResult OnPostPrint(AppReportClass.ReportType _ReportType)
        {
            var _Heading1 = "Receivable / Payable Report";
            var _Heading2 = "";
            var _COA_List = AppRegistry.GetText(UserName, "CompanyGLs");
            var _Filter = $"";
            MyTable = DataTableClass.GetTable(UserName, SQLQuery.CompanyBalances(_Filter, _COA_List));


            AppReportClass.ReportParameters RptParameters = new()
            {
                ReportFile = "CompanyBalances",
                OutputPath = "",
                OutputPathLink = "",
                OutputFileName = "",
                OutputFileExtention = "",
                OutputFileFullName = "",
                DataSetName = "",
                ReportData = MyTable,
                ReportType = _ReportType,
                CompanyName = "",
                Heading1 = _Heading1,
                Heading2 = _Heading2,
                Footer = AppFunctions.AppGlobals.ReportFooter,
            };
            return Page();
        }
        #endregion

        #region Ledger
        public IActionResult OnPostLedger(int ID, DateTime _Date)
        {
            AppRegistry.SetKey(UserName, "GL_Company", ID, KeyType.Number);
            AppRegistry.SetKey(UserName, "GL_Dt_From", AppRegistry.GetFiscalFrom(), KeyType.Date);
            AppRegistry.SetKey(UserName, "GL_Dt_To", Variables.ReportDate, KeyType.Date);
            return RedirectToPage("PrintReport", "GLCompany", new { _ReportType = ReportType.Preview });
        }


        #endregion


        #region Variables
        public class Parameters
        {
            public DateTime ReportDate { get; set; }
            public string ReportType { get; set; } = "All";
        }
        #endregion

    }
}