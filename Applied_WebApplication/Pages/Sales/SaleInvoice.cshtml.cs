using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NPOI.HSSF.Record;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Drawing;
using System.Text;

namespace Applied_WebApplication.Pages.Sales
{
    public class SaleInvoiceModel : PageModel
    {
        #region Setup
        [BindProperty]
        public MyParameters Variables { get; set; }
        public List<Message> ErrorMessages { get; set; } = new();
        public string UserName => User.Identity.Name;
        public DataTable Invoice { get; set; }
        public DataRow Row1 { get; set; }
        public DataRow Row2 { get; set; }
        public bool IsSaved { get; set; } = false;
        public bool Refresh { get; set; } = false;
        public string Vou_No { get; set; }
        public int TranID { get; set; }
        public bool IsNew { get; set; }

        public SQLiteCommand MyCommand = new();


        TempDBClass TempInvoice11 { get; set; } = new();
        TempDBClass TempInvoice22 { get; set; } = new();

        public string _Filter1;
        public string _Filter2;
        #endregion

        #region GET

        public void OnGet(string? Vou_No, int? Sr_No, bool? _Refresh)
        {

            #region Nill, Nill & Nill Vales
            if (Vou_No == null && Sr_No == null && _Refresh == null)
            {
                Vou_No ??= "NEW";
                _Refresh ??= true;
                Refresh = (bool)_Refresh;

                if (Refresh)                         // Setup a Voucher in as Temp Data Base; (Reset)
                {
                    _Filter1 = $"Vou_No='{Vou_No}'";
                    TempInvoice11 = new(UserName, Tables.BillReceivable, _Filter1, Refresh);
                    _Filter2 = $"TranID={TempInvoice11.CurrentRow["ID"]}";
                    TempInvoice22 = new(UserName, Tables.BillReceivable2, _Filter2, Refresh);

                    MyCommand = TempDBClass.CommandDeleteAll(TempInvoice11.MyTempConnection, Tables.BillReceivable);
                    MyCommand.ExecuteNonQuery();

                    MyCommand = TempDBClass.CommandDeleteAll(TempInvoice11.MyTempConnection, Tables.BillReceivable2);
                    MyCommand.ExecuteNonQuery();
                }
                Row1 = TempInvoice11.CurrentRow;
                Row2 = TempInvoice22.CurrentRow;

                Row1["ID"] = MaxID(Tables.BillReceivable); 
                Row2["ID"] = 0;
                Row1["Vou_No"] = "NEW";
                Row2["TranID"] = Row1["ID"];
                Row2["Sr_No"] = 1;
                Row1["Status"] = VoucherStatus.Submitted.ToString();

                Invoice = TempInvoice22.TempTable;

                Row2Variables();
                return;
            }
            #endregion

            if (Vou_No.Length == 0) { Vou_No = "NEW"; }

            Variables = new();

            if (Vou_No.ToUpper() == "NEW")
            {
                IsNew = true;

                _Filter1 = $"Vou_No='NEW'";
                TempInvoice11 = new TempDBClass(UserName, Tables.BillReceivable, _Filter1, false);
                if (TempInvoice11.CountTemp > 0)
                {
                    TranID = (int)TempInvoice11.CurrentRow["ID"];
                    Row1 = TempInvoice11.CurrentRow;
                }
                else
                {
                    Row1 = TempInvoice11.NewRecord();
                    Row1["Vou_No"] = "NEW";
                    Row1["Status"] = VoucherStatus.Submitted.ToString(); ;
                    TranID = 1;
                    Sr_No = -1;
                }

                _Filter2 = $"TranID={TranID}";
                TempInvoice22 = new(UserName, Tables.BillReceivable2, _Filter2, false);
                if (Sr_No == -1)
                {
                    Row2 = TempInvoice22.NewRecord();
                    Row2["Sr_No"] = MaxSrNo(TempInvoice22.TempTable);
                    Row2["TranID"] = TranID;
                }
                else
                {
                    if (TempInvoice22.CountTemp > 0)
                    {
                        TempInvoice22.TempView.RowFilter = $"Sr_No={Sr_No}";
                        if (TempInvoice22.TempView.Count > 0)
                        {
                            Row2 = TempInvoice22.TempView[0].Row;
                        }
                        else
                        {
                            Row2 = TempInvoice22.CurrentRow;                 // Error id not found the record, pass empty row
                        }

                    }
                }

                Invoice = TempInvoice22.TempTable;
                Row2Variables();
                return;
            }

            #region Temp

            //if (Vou_No.Length == 11)
            //{

            //    TempInvoice1 = new TempTableClass(UserName, Tables.BillReceivable, Vou_No, true);
            //    if (TempInvoice1.CountTemp > 0)
            //    {
            //        TranID = (int)TempInvoice1.CurrentRow["ID"];
            //        Row1 = TempInvoice1.CurrentRow;
            //    }
            //    else
            //    {
            //        ErrorMessages.Add(MessageClass.SetMessage("Error.... Voucher Not Found"));
            //        return;
            //    }

            //    TempInvoice2 = new TempTableClass(UserName, Tables.BillReceivable2, TranID, IsNew);
            //    if (Sr_No == -1)
            //    {
            //        Row2 = TempInvoice2.NewRecord();
            //        Row2["Sr_No"] = MaxSrNo(TempInvoice2.TempTable);
            //        Row2["TranID"] = Row1["ID"];
            //    }
            //    else
            //    {
            //        if (TempInvoice2.CountTemp > 0)
            //        {
            //            TempInvoice2.TempView.RowFilter = $"Sr_No={Sr_No}";
            //            if (TempInvoice2.TempView.Count > 0)
            //            {
            //                Row2 = TempInvoice2.TempView[0].Row;
            //            }
            //            else
            //            {
            //                Row2 = TempInvoice2.CurrentRow;
            //            }

            //        }
            //        else
            //        {
            //            ErrorMessages.Add(MessageClass.SetMessage("Error.... Voucher Not Found"));
            //            return;
            //        }
            //    }
            //    Invoice = TempInvoice2.TempTable;
            //    Row2Variables();
            //    return;

            //}


            #endregion
        }
        #endregion

        public int MaxSrNo(DataTable _Table)
        {
            var MaxSrNo = _Table.Compute("MAX(Sr_No)", "");
            if (MaxSrNo == DBNull.Value) { return 1; }
            else { return (int)MaxSrNo + 1; }
        }

        public int MaxID(Tables _Tables)
        {
            DataTable _Table= DataTableClass.GetTable(UserName, _Tables);
            var MaxTranID = _Table.Compute("MAX(ID)", "");
            if (MaxTranID == DBNull.Value) { return 1; }
            else { return (int)MaxTranID + 1; }
        }



        #region Variables / Rows
        private void Row2Variables()
        {

            Variables = new()
            {
                ID1 = (int)Row1["ID"],
                ID2 = (int)Row2["ID"],

                Vou_No = Row1["Vou_No"].ToString(),
                Vou_Date = (DateTime)Row1["Vou_Date"],
                Company = (int)Row1["Company"],
                Employee = (int)Row1["Employee"],
                Ref_No = Row1["Ref_No"].ToString(),
                Inv_No = Row1["Inv_No"].ToString(),
                Inv_Date = (DateTime)Row1["Inv_Date"],
                Pay_Date = (DateTime)Row1["Pay_Date"],
                Remarks = Row1["Description"].ToString(),
                Comments = Row1["Comments"].ToString(),
                Status = Row1["Status"].ToString(),

                TranID = (int)Row1["ID"],
                Sr_No = (int)Row2["Sr_No"],
                Inventory = (int)Row2["Inventory"],
                Batch = Row2["Batch"].ToString(),
                Qty = (decimal)Row2["Qty"],
                Rate = (decimal)Row2["Rate"],
                Tax = (int)Row2["Tax"],
                Description = Row2["Description"].ToString(),
                Project = (int)Row2["Project"],
            };

            Variables.Amount = (Variables.Qty * Variables.Rate);
            Variables.TaxRate = (int)AppFunctions.GetTaxRate(UserName, Variables.Tax);
            Variables.TaxAmount = (Variables.Amount * Variables.TaxRate) / 100;
            Variables.NetAmount = Variables.Amount + Variables.TaxAmount;


        }
        private void Variables2Row()
        {
            Row1["ID"] = Variables.ID1;
            Row2["ID"] = Variables.ID2;

            Row1["Vou_No"] = Variables.Vou_No;
            Row1["Vou_Date"] = Variables.Vou_Date;
            Row1["Company"] = Variables.Company;
            Row1["Employee"] = Variables.Employee;
            Row1["Ref_No"] = Variables.Ref_No;
            Row1["Inv_No"] = Variables.Inv_No;
            Row1["Inv_Date"] = Variables.Inv_Date;
            Row1["Pay_Date"] = Variables.Pay_Date;
            Row1["Description"] = Variables.Remarks;
            Row1["Comments"] = Variables.Comments;
            Row1["Status"] = Variables.Status;

            Row2["TranID"] = Variables.TranID;
            Row2["Sr_No"] = Variables.Sr_No;
            Row2["Inventory"] = Variables.Inventory;
            Row2["Batch"] = Variables.Batch;
            Row2["Qty"] = Variables.Qty;
            Row2["Rate"] = Variables.Rate;
            Row2["Tax"] = Variables.Tax;
            Row2["Description"] = Variables.Description;
            Row2["Project"] = Variables.Project;

        }

        #endregion

        #region POST

        public IActionResult OnPostAdd()
        {
            Vou_No = Variables.Vou_No;
            TranID = Variables.TranID;

            _Filter1 = $"Vou_No='{Vou_No}'";
            TempInvoice11 = new(UserName, Tables.BillReceivable, _Filter1, false);
            _Filter2 = $"TranID={TempInvoice11.CurrentRow["ID"]}";
            TempInvoice22 = new(UserName, Tables.BillReceivable2, _Filter2, false);
            TempInvoice22.TempView.RowFilter = $"Sr_No={Variables.Sr_No}";
            if (TempInvoice22.TempView.Count==0) { TempInvoice22.NewRecord(); }

            Row1 = TempInvoice11.CurrentRow;
            Row2 = TempInvoice22.CurrentRow;

            Variables2Row();

            var ValidRow1 = TempInvoice11.TableValidate.Validation(Row1);
            var ValidRow2 = TempInvoice22.TableValidate.Validation(Row2);

            if (ValidRow1 && ValidRow2)
            {
                TempInvoice11.Save(TempInvoice11.MyTempConnection, false);
                TempInvoice22.CurrentRow["TranID"] = TempInvoice11.CurrentRow["ID"];
                TempInvoice22.Save(TempInvoice22.MyTempConnection, false);
            }
            else
            {
                ErrorMessages.AddRange(TempInvoice11.ErrorMessages);
                ErrorMessages.AddRange(TempInvoice22.ErrorMessages);
            }

            // Reset and Update Web Page Data
            if (ErrorMessages.Count == 0)
            {
                _Filter1 = $"Vou_No='{Vou_No}'";
                TempInvoice11 = new(UserName, Tables.BillReceivable, _Filter1, false);
                _Filter2 = $"TranID={TempInvoice11.CurrentRow["ID"]}";
                TempInvoice22 = new(UserName, Tables.BillReceivable2, _Filter2, false);

                Invoice = TempInvoice22.TempTable;
                var message2 = $"Serial No {Variables.Sr_No} of Invoice No {Variables.Vou_No} has been saved successfully.";
                ErrorMessages.Add(MessageClass.SetMessage(message2, Color.Yellow));
            }
            return Page();
        }
        public IActionResult OnPostEdit(int? Sr_No)
        {
            var Vou_No = Variables.Vou_No;
            return RedirectToPage("SaleInvoice", routeValues: new { Vou_No, Sr_No });
        }
        public IActionResult OnPostDelete(int? Sr_No)
        {
            var Vou_No = Variables.Vou_No;
            // Write code here for delete the record.

            return RedirectToPage("SaleInvoice", routeValues: new { Vou_No, Sr_No });
        }

        public IActionResult OnPostNew()
        {
            var Vou_No = Variables.Vou_No;
            var Sr_No = -1;
            return RedirectToPage("SaleInvoice", routeValues: new { Vou_No, Sr_No });
        }
        public IActionResult OnPostSave()
        {
            if (Variables.Vou_No.ToUpper() == "NEW")
            {
                var IsSaved = Save_VoucherNew();
                if (!IsSaved) { return Page(); }
            }
            else
            {
                var IsSaved = Save_Voucher();
                if (!IsSaved) { return Page(); }
            }
            #region Temp
            //// Validate Data
            //bool Valid1 = false;
            //bool Valid2 = true;

            //if (TempTable1.Rows.Count == 1)
            //{
            //    Row1 = TempTable1.Rows[0];
            //    ErrorMessages = TableValidationClass.RowValidation(Row1);
            //    if (ErrorMessages.Count > 0) { return Page(); } else { Valid1 = true; }
            //}
            //else
            //{
            //    ErrorMessages.Add(MessageClass.SetMessage("No record found..1.." + Variables.Vou_No));
            //}

            //if (Valid1)                 // If Table 1 valid is true;
            //{
            //    if (TempTable2.Rows.Count > 0)
            //    {
            //        foreach (DataRow Row in TempTable2.Rows)
            //        {
            //            ErrorMessages = TableValidationClass.RowValidation(Row);
            //            if (ErrorMessages.Count > 0) { return Page(); } else { Valid2 = true; }
            //        }
            //    }
            //    else
            //    {
            //        ErrorMessages.Add(MessageClass.SetMessage("No record found..2.." + Variables.Vou_No));
            //    }

            //    #endregion

            //    if (Valid2)
            //    {
            //        //var TargetRow = Row1;
            //        DataTableClass TargetTable1 = new(UserName, Tables.BillReceivable);
            //        TargetTable1.NewRecord();
            //        TargetTable1.CurrentRow.ItemArray = Row1.ItemArray;

            //        if (Variables.Vou_No.ToUpper() == "NEW")
            //        {
            //            Variables.Vou_No = GetNewVouNo(Variables.Vou_Date, "SL");
            //            TargetTable1.CurrentRow["Vou_No"] = Variables.Vou_No;
            //            TargetTable1.CurrentRow["ID"] = 0;
            //        }

            //        #region Save

            //        TargetTable1.Save();
            //        Variables.TranID = (int)TargetTable1.CurrentRow["ID"];
            //        ErrorMessages.AddRange(TargetTable1.TableValidation.MyMessages);
            //        if (ErrorMessages.Count > 0)
            //        {
            //            return Page();
            //        }
            //        Variables.TranID = (int)TargetTable1.CurrentRow["ID"];


            //        // Table 2


            //        DataTableClass TargetTable2 = new(UserName, Tables.BillReceivable2);
            //        TargetTable2.MyDataView.RowFilter = $"TranID={Variables.TranID}";

            //        DataView TempView2 = TargetTable2.MyDataView;
            //        foreach (DataRow Row in TargetTable2.Rows)
            //        {
            //            TempView2.RowFilter = $"ID={Row["ID"]}";
            //            if (TempView2.Count == 0)
            //            {
            //                TargetTable2.Delete();                      // Delete in Target Table is not exist in Temp Table;
            //            }
            //        }

            //        foreach (DataRow Row in TempTable2.Rows)
            //        {
            //            Row2 = TargetTable2.NewRecord();
            //            Row2.ItemArray = Row.ItemArray;
            //            TargetTable2.CurrentRow = Row2;
            //            TargetTable2.Save();                                    // Save existing all Temp Record in Targe Table.
            //        }
            //        if (TargetTable2.ErrorMessages.Count > 0) { return Page(); }

            //        #endregion


            //        #region Delete
            //        // Delete Voucher from Temp Data
            //        TempTableClass.Delete(UserName, Tables.BillReceivable, (int)Row1["ID"]);

            //        foreach (DataRow Row in TempTable2.Rows)
            //        {
            //            TempTableClass.Delete(UserName, Tables.BillReceivable2, (int)Row1["ID"]);
            //        }

            //        #endregion

            //        var message = $"Sale invoice {Variables.Vou_No} SAVED successfully., ";
            //        ErrorMessages.Add(MessageClass.SetMessage(message, Color.Green));
            //    }
            //}
            //IsSaved = true;


            #endregion

            var Vou_No = Variables.Vou_No;
            var Sr_No = 1;
            return RedirectToPage("SaleInvoice", routeValues: new { Vou_No, Sr_No });

        }

        private bool Save_Voucher()
        {
            var TempTable1 = TempTableClass.GetTable(UserName, Tables.BillReceivable, $"Vou_No = '{Variables.Vou_No}'");
            var TempTable2 = TempTableClass.GetTable(UserName, Tables.BillReceivable2, $"TranID = {TempTable1.Rows[0]["ID"]}");

            #region Validation
            // Validate Data
            bool Valid1 = false;
            bool Valid2 = true;

            if (TempTable1.Rows.Count == 1)
            {
                Row1 = TempTable1.Rows[0];
                ErrorMessages = TableValidationClass.RowValidation(Row1);
                if (ErrorMessages.Count > 0) { return false; } else { Valid1 = true; }
            }
            else
            {
                ErrorMessages.Add(MessageClass.SetMessage("No record found..1.." + Variables.Vou_No));
            }

            if (Valid1)                 // If Table 1 valid is true;
            {
                if (TempTable2.Rows.Count > 0)
                {
                    foreach (DataRow Row in TempTable2.Rows)
                    {
                        ErrorMessages = TableValidationClass.RowValidation(Row);
                        if (ErrorMessages.Count > 0) { return false; } else { Valid2 = true; }
                    }
                }
                else
                {
                    ErrorMessages.Add(MessageClass.SetMessage("No record found..2.." + Variables.Vou_No));
                }

                #endregion

                if (Valid2)
                {
                    DataTableClass TargetTable1 = new(UserName, Tables.BillReceivable);
                    TargetTable1.NewRecord();
                    TargetTable1.CurrentRow.ItemArray = Row1.ItemArray;

                    #region Save

                    TargetTable1.Save();
                    Variables.TranID = (int)TargetTable1.CurrentRow["ID"];
                    ErrorMessages.AddRange(TargetTable1.TableValidation.MyMessages);
                    if (ErrorMessages.Count > 0) { return false; }
                    Variables.TranID = (int)TargetTable1.CurrentRow["ID"];

                    // Table 2
                    DataTableClass TargetTable2 = new(UserName, Tables.BillReceivable2);
                    TargetTable2.MyDataView.RowFilter = $"TranID={Variables.TranID}";

                    DataView TempView2 = TargetTable2.MyDataView;
                    foreach (DataRow Row in TargetTable2.Rows)
                    {
                        TempView2.RowFilter = $"ID={Row["ID"]}";
                        if (TempView2.Count == 0)
                        {
                            TargetTable2.Delete();                      // Delete in Target Table is not exist in Temp Table;
                        }
                    }

                    foreach (DataRow Row in TempTable2.Rows)
                    {
                        Row2 = TargetTable2.NewRecord();
                        Row2.ItemArray = Row.ItemArray;
                        TargetTable2.CurrentRow = Row2;
                        TargetTable2.Save();                                    // Save existing all Temp Record in Targe Table.
                        if (TargetTable2.ErrorMessages.Count > 0) { return false; }
                    }


                    #endregion

                    #region Delete
                    // Delete Voucher from Temp Data
                    TempTableClass.Delete(UserName, Tables.BillReceivable, (int)Row1["ID"]);

                    foreach (DataRow Row in TempTable2.Rows)
                    {
                        TempTableClass.Delete(UserName, Tables.BillReceivable2, (int)Row["ID"]);
                    }

                    #endregion

                }
                //-------------------
            }
            return true;
        }



        private bool Save_VoucherNew()
        {
            var TempTable1 = TempTableClass.GetTable(UserName, Tables.BillReceivable, $"Vou_No = 'NEW'");
            var TempTable2 = TempTableClass.GetTable(UserName, Tables.BillReceivable2, $"TranID = {TempTable1.Rows[0]["ID"]}");

            #region Validation
            // Validate Data
            bool Valid1 = false;
            bool Valid2 = true;

            if (TempTable1.Rows.Count == 1)
            {
                Row1 = TempTable1.Rows[0];
                ErrorMessages = TableValidationClass.RowValidation(Row1);
                if (ErrorMessages.Count > 0) { return false; } else { Valid1 = true; }
            }
            else
            {
                ErrorMessages.Add(MessageClass.SetMessage("No record found..1.." + Variables.Vou_No));
                return false;
            }

            if (Valid1)                 // If Table 1 valid is true;
            {
                if (TempTable2.Rows.Count > 0)
                {
                    foreach (DataRow Row in TempTable2.Rows)
                    {
                        ErrorMessages = TableValidationClass.RowValidation(Row);
                        if (ErrorMessages.Count > 0) { return false; } else { Valid2 = true; }
                    }
                }
                else
                {
                    ErrorMessages.Add(MessageClass.SetMessage("No record found..2.." + Variables.Vou_No));
                    return false;
                }

                #region SAVE
                if (Valid1 && Valid2)
                {
                    DataTableClass TargetTable1 = new(UserName, Tables.BillReceivable);
                    DataTableClass TargetTable2 = new(UserName, Tables.BillReceivable2);

                    Variables.Vou_No = GetNewVouNo(Variables.Vou_Date, "SL");
                    TargetTable1.NewRecord();
                    TargetTable1.CurrentRow["Vou_No"] = Variables.Vou_No;
                    TargetTable1.CurrentRow["ID"] = 0;
                    TargetTable1.Save(false);
                    ErrorMessages = TargetTable1.ErrorMessages;
                    if (ErrorMessages.Count > 0) { return false; }

                    Variables.TranID = (int)TargetTable1.CurrentRow["ID"];
                    foreach (DataRow Row in TempTable2.Rows)
                    {
                        Row2 = TargetTable2.NewRecord();
                        Row2.ItemArray = Row.ItemArray;
                        TargetTable2.CurrentRow = Row2;
                        Row2["TranID"] = Variables.TranID;
                        Row2["ID"] = 0;
                        TargetTable2.Save(false);
                        ErrorMessages = TargetTable2.ErrorMessages;
                        if (ErrorMessages.Count > 0) { return false; }

                    }
                    #endregion

                    #region Delete
                    TempTableClass.Delete(UserName, Tables.BillReceivable, (int)Row1["ID"]);
                    foreach (DataRow Row in TempTable2.Rows)
                    {
                        TempTableClass.Delete(UserName, Tables.BillReceivable2, (int)Row["ID"]);
                    }
                    #endregion
                }
                #endregion
            }
            return true;
        }

        #region New Voucher Number
        private string GetNewVouNo(DateTime _Date, string _VouTag)
        {
            string NewNum;
            StringBuilder _Text = new StringBuilder();
            _Text.Append("SELECT [Vou_No], ");
            _Text.Append("substr([Vou_No],1,2) AS Tag, ");
            _Text.Append("SubStr([Vou_No],3,2) AS Year, ");
            _Text.Append("SubStr([Vou_No],5,2) AS Month, ");
            _Text.Append("Cast((SubStr([Vou_No],8,4)) as integer) AS num ");
            _Text.Append("FROM [BillReceivable]");

            DataTable _Table = DataTableClass.GetTable(UserName, _Text.ToString(), "");
            DataView _View = _Table.AsDataView();

            var _Year = _Date.ToString("yy");
            var _Month = _Date.ToString("MM");
            _View.RowFilter = string.Concat("Vou_No like '", _VouTag, _Year, _Month, "%'");
            DataTable _VouList = _View.ToTable();
            var MaxNum = _VouList.Compute("Max(num)", "");
            if (MaxNum == DBNull.Value)
            {
                NewNum = string.Concat(_VouTag, _Year, _Month, "-", "0001");
            }
            else
            {
                var _MaxNum = int.Parse(MaxNum.ToString()) + 1;
                NewNum = string.Concat(_VouTag, _Year, _Month, "-", _MaxNum.ToString("0000"));
            }
            return NewNum;
        }


        #endregion

        public IActionResult OnPostPrint()
        {
            var TranID = Variables.TranID;
            return RedirectToPage("../ReportPrint/PrintReport", pageHandler: "SaleInvoice", routeValues: new { TranID });
        }
        public IActionResult OnPostBack()
        {
            return RedirectToPage("../Accounts/BillReceivableList");
        }
        #endregion

        #region Variables

        public class MyParameters
        {
            public int ID1 { get; set; }
            public int ID2 { get; set; }
            public string Vou_No { get; set; }
            public string Ref_No { get; set; }
            public string Inv_No { get; set; }
            public int TranID { get; set; }
            public int Sr_No { get; set; }
            public int Inventory { get; set; }
            public int Company { get; set; }
            public int Employee { get; set; }
            public int Project { get; set; }
            public int Tax { get; set; }

            public DateTime Vou_Date { get; set; }
            public DateTime Inv_Date { get; set; }
            public DateTime Pay_Date { get; set; }

            public string Remarks { get; set; }
            public string Comments { get; set; }
            public string Description { get; set; }
            public string Batch { get; set; }
            public string Status { get; set; }

            public decimal Qty { get; set; }
            public decimal Rate { get; set; }
            public decimal Amount { get; set; }
            public int TaxRate { get; set; }
            public decimal TaxAmount { get; set; }
            public decimal NetAmount { get; set; }

        }
        #endregion


    }
}
