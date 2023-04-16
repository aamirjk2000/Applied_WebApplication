﻿using System.Globalization;
using System.Security.Claims;

namespace Applied_WebApplication.Data
{
    public interface IAppliedDependency
    {
        string AppPath { get; set; }
        string AppRoot { get; }
        string ReportPath { get; }
        string PrintedReportPath { get; }
        string PrintedReportPathLink { get; }
        string DefaultDB { get; }
        string LocalDB { get; }
        string DefaultPath { get; }
        string UserDBPath { get; }
        string GuestDBPath { get; }
        CultureInfo AppCurture { get; }
        string CultureString { get; }
        string InputDatesFormat { get; }
        string DateFormat { get; set; }
        string CurrencyFormat { get; set; }
        string ReportFooter { get; set; }

    }

    public class AppliedDependency : IAppliedDependency
    {
        public ClaimsPrincipal AppUser { get; set; }
        public string AppPath { get; set; }
        public string AppRoot { get; }
        public string ReportPath { get; }
        public string PrintedReportPath { get; }
        public string PrintedReportPathLink { get; }
        public string UserDBPath { get; }
        public string GuestDBPath { get; }
        public string DefaultDB { get; set; }
       public string LocalDB { get; }
        public string DefaultPath { get; }
        public CultureInfo AppCurture { get; }
        public string CultureString { get; }
        public string InputDatesFormat { get; }
        public string DateFormat { get; set; }
        public string CurrencyFormat { get; set; }
        public string ReportFooter { get; set; }




        public AppliedDependency()
        {

            AppPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            AppRoot = ".\\wwwroot\\";
            ReportPath = string.Concat(AppRoot, "Reports\\");
            PrintedReportPath = string.Concat(AppRoot, "PrintedReports\\");
            PrintedReportPathLink = "~/PrintedReports/";
            DefaultDB = string.Concat(AppRoot, "SQLiteDB\\");
            LocalDB = string.Concat(AppPath, "\\LocalDB\\");
            UserDBPath = string.Concat(DefaultDB, "AppliedUsers.db");
            GuestDBPath = string.Concat(DefaultDB, "Applied.db");
            CultureString = "en-US";
            AppCurture = new CultureInfo(CultureString, false);
            InputDatesFormat = "yyyy-MM-dd";
            DateFormat = "dd-MM-yyyy";
            CurrencyFormat = "#0.00";
            ReportFooter = "Powered by Applied Software House, +92 336 2454 230";


            // If User is existing in class.
            if (AppUser != null)
            {
                string UserName = AppUser.Identity.Name;
                if (AppUser.Identity.Name.Length > 0)
                {
                    PrintedReportPath = string.Concat(PrintedReportPath, UserName, "\\");
                    PrintedReportPathLink = string.Concat(PrintedReportPath, UserName, "/");
                }
            }



        }
    }
}
