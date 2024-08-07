using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Data.SQLite;

namespace Applied_WebApplication.Pages.Stock
{
    [Authorize]
    public class ProductionModel : PageModel
    {
        [BindProperty]
        public Parameters Variables { get; set; }
        public string UserName => User.Identity.Name;
        public DataTableClass Class_ProductsView { get; set; }
        public DataTableClass Class_Products { get; set; }
        public DataTableClass Class_Products2 { get; set; }
        public DataTable tb_Products { get; set; }

        public List<Message> ErrorMessages = new();
        public bool IsError => GetError();

        #region GET

        public void OnGet()
        {
            Class_ProductsView = new DataTableClass(UserName, Tables.view_Production, "ID1 < 0");
            tb_Products = Class_ProductsView.MyDataTable;

            Variables = new Parameters();
            {
                Variables.ID1 = 0;
                Variables.ID2 = 0;
                Variables.Vou_No = "NEW";
                Variables.Vou_Date = DateTime.Now;
                Variables.Batch = string.Empty;
                Variables.Flow = string.Empty;
                Variables.StockID = 0;
                Variables.UOM = 0;
                Variables.Qty = 0.00M;
                Variables.Rate = 0.00M;
            }
        }

        public void OnGetRefresh(string Vou_No, int ID2)
        {
            var _Filter = $"Vou_No='{Vou_No}'";
            Class_ProductsView = new DataTableClass(UserName, Tables.view_Production, _Filter);
            tb_Products = Class_ProductsView.MyDataTable;

            if (Class_ProductsView.CountTable > 0)
            {
                if (ID2 > 0)
                {
                    Class_ProductsView.MyDataView.RowFilter = $"ID2={ID2}";
                    if (Class_ProductsView.CountView > 0)
                    {
                        Class_ProductsView.CurrentRow = Class_ProductsView.MyDataView[0].Row;

                    }
                    else
                    {
                        Class_ProductsView.CurrentRow = Class_ProductsView.MyDataTable.Rows[0];

                    }

                    GetVariables(Class_ProductsView.CurrentRow);
                }
                else
                {
                    Variables = new Parameters();
                    GetNewRow(Class_ProductsView.CurrentRow);
                }
            }

        }
        #endregion

        #region Post

        public IActionResult OnPostNew()
        {
            return RedirectToPage("Production", "Refresh", new { Variables.Vou_No, ID2 = 0 });
        }

        public IActionResult OnPostEdit(int ID2)
        {
            return RedirectToPage("Production", "Refresh", new { Variables.Vou_No, ID2 });
        }

        public IActionResult OnPostDelete(int ID2)
        {
            var _ID1 = 0;
            if (ID2 > 0)
            {
                Class_Products2 = new DataTableClass(UserName, Tables.Production2, $"ID = {ID2}");
                if (Class_Products2.CountTable > 0)
                {
                    _ID1 = (int)Class_Products2.CurrentRow["TranID"];
                    Class_Products2.SeekRecord(ID2);
                    if (Class_Products2.IsFound)
                    {
                        Class_Products2.Delete();
                        Class_Products2 = new DataTableClass(UserName, Tables.Production2, $"TranID = {_ID1}");              // Refresh Data after Delete 
                        if (Class_Products2.CountTable == 0)
                        {
                            ErrorMessages.Add(MessageClass.SetMessage("Record Deleted Sucessfully.", ConsoleColor.Green));
                            Class_Products = new DataTableClass(UserName, Tables.Production, $"ID = {_ID1}");
                            if ((int)Class_Products.CurrentRow["ID"] == _ID1)
                            {
                                Class_Products.Delete();                 // Delete From mater products table if details are empty.
                                ErrorMessages.Add(MessageClass.SetMessage("Production Transaction has been Deleted Sucessfully.", ConsoleColor.Green));
                                return RedirectToPage("ProductionList");
                            }
                        }

                    }
                    else
                    {
                        ErrorMessages.Add(MessageClass.SetMessage("Record NOT Deleted.", ConsoleColor.Red));
                    }
                }
            }
            else
            {
                ErrorMessages.Add(MessageClass.SetMessage("Record NOT found to Delete", ConsoleColor.Yellow));
            }

            var _Filter = $"Vou_No='{Variables.Vou_No}'";
            Class_ProductsView = new DataTableClass(UserName, Tables.view_Production, _Filter);
            return Page();
        }
        public IActionResult OnPostBack()
        {
            return RedirectToPage("ProductionList");

        }
        public IActionResult OnPostSave(int ID2)
        {
            Class_Products = new DataTableClass(UserName, Tables.Production);
            Class_Products2 = new DataTableClass(UserName, Tables.Production2);
            GetRow();

            var Validate1 = Class_Products.TableValidation.Validation(Class_Products.CurrentRow);  // Validate the current row
            var Validate2 = Class_Products2.TableValidation.Validation(Class_Products2.CurrentRow);  // Validate the current row

            if (Validate1 && Validate2)
            {
                if (Class_Products.CurrentRow["Vou_No"].ToString().ToUpper() == "NEW")
                {
                    Class_Products.CurrentRow["Vou_No"] = NewVoucher.GetNewVoucher(Class_Products.MyDataTable, "PD");
                }

                Class_Products.Save(false);                 // Do not Validate again
                Variables.Vou_No = (string)Class_Products.CurrentRow["Vou_No"];

                Class_Products2.CurrentRow["TranID"] = Class_Products.CurrentRow["ID"];
                Class_Products2.Save(false);                // Do not Validate again
                Variables.ID2 = (int)Class_Products2.CurrentRow["ID"];

                return RedirectToPage("Production", "Refresh", new { Variables.Vou_No, Variables.ID2 });

            }
            {

                ErrorMessages.AddRange(Class_Products.ErrorMessages);
                ErrorMessages.AddRange(Class_Products2.ErrorMessages);
            }

            var _Filter = $"Vou_No='{Variables.Vou_No}'";
            Class_ProductsView = new DataTableClass(UserName, Tables.view_Production, _Filter);


            return Page();

        }

        public IActionResult OnPostEqual(int ID2)
        {

            bool IsUpdated = false;
            var _Filter = $"Vou_No='{Variables.Vou_No}'";
            Class_ProductsView = new DataTableClass(UserName, Tables.view_Production, _Filter);
            tb_Products = Class_ProductsView.MyDataTable;

            decimal Tot_In_Amount = 0.00M;
            decimal Tot_Out_Amount = 0.00M;

            foreach (DataRow Row in tb_Products.Rows)
            {
                var _Flow = Row["Flow"].ToString();
                var _true = decimal.TryParse(Row["Amount"].ToString(), out decimal _Amount);

                if (_Flow == "In") { Tot_In_Amount += _Amount; }
                if (_Flow == "Out") 
                {if ((int)Row["ID2"] == ID2) { Tot_Out_Amount += _Amount; }; }
            }

            decimal _Difference = Math.Round(Tot_In_Amount, 6) - Math.Round(Tot_Out_Amount, 6);
            if (_Difference != 0)
            {

                foreach (DataRow Row in tb_Products.Rows)
                {
                    if ((int)Row["ID2"] == ID2)
                    {
                        decimal _Qty = (decimal)Row["Qty"];
                        decimal _Rate = Math.Round(_Difference / _Qty, 6);
                        SQLiteCommand _Command = new(ConnectionClass.AppConnection(UserName));
                        _Command.CommandText = "UPDATE [Production2] SET [Rate] = @Rate WHERE [ID] = @ID;";
                        _Command.Parameters.AddWithValue("@Rate", _Rate);
                        _Command.Parameters.AddWithValue("@ID", ID2);
                        int Effacted = _Command.ExecuteNonQuery();
                        if (Effacted > 0) { IsUpdated = true; }
                    }
                }
                if (!IsUpdated)
                {
                    ErrorMessages = new()
                    {
                        MessageClass.SetMessage("Rate not saved due to some error.")
                    };
                    return Page();
                }
            }
            return RedirectToPage("Production", "Refresh", new { Variables.Vou_No, ID2 });
        }

        #endregion

        #region Methods
        private void GetRow()
        {
            Class_Products.NewRecord();
            Class_Products.CurrentRow["ID"] = Variables.ID1;
            Class_Products.CurrentRow["Vou_No"] = Variables.Vou_No;
            Class_Products.CurrentRow["Vou_Date"] = Variables.Vou_Date;
            Class_Products.CurrentRow["Batch"] = Variables.Batch;
            Class_Products.CurrentRow["Remarks"] = Variables.Remarks1;
            Class_Products.CurrentRow["Comments"] = Variables.Comments;

            Class_Products2.NewRecord();
            Class_Products2.CurrentRow["ID"] = Variables.ID2;
            Class_Products2.CurrentRow["TranID"] = Variables.TranID;
            Class_Products2.CurrentRow["Stock"] = Variables.StockID;
            Class_Products2.CurrentRow["Flow"] = Variables.Flow;
            Class_Products2.CurrentRow["UOM"] = Variables.UOM;
            Class_Products2.CurrentRow["Qty"] = Variables.Qty;
            Class_Products2.CurrentRow["Rate"] = Variables.Rate;
            Class_Products2.CurrentRow["Remarks"] = Variables.Remarks2;
        }

        private void GetNewRow(DataRow Row)
        {
            DataTableClass.RemoveNull(Row);
            Variables.ID1 = (int)Row["ID1"];
            Variables.Vou_No = (string)Row["Vou_No"];
            Variables.Vou_Date = (DateTime)Row["Vou_Date"];
            Variables.Batch = (string)Row["Batch"];
            Variables.Remarks1 = (string)Row["Remarks"];
            Variables.Comments = (string)Row["Comments"];
            Variables.ID2 = 0;
            Variables.TranID = 0;
            Variables.Flow = string.Empty;
            Variables.StockID = 0;
            Variables.Qty = 0.00M;
            Variables.Rate = 0.00M;
            Variables.Remarks2 = string.Empty;

        }

        private void GetVariables(DataRow _Row)
        {

            _Row ??= Class_ProductsView.NewRecord();
            _Row = DataTableClass.RemoveNull(_Row);

            Variables = new();
            {
                Variables.ID1 = (int)_Row["ID1"];
                Variables.Vou_No = (string)_Row["Vou_No"];
                Variables.Vou_Date = (DateTime)_Row["Vou_Date"];
                Variables.Batch = (string)_Row["Batch"];
                Variables.Remarks1 = (string)_Row["Remarks"];
                Variables.Comments = (string)_Row["Comments"];
                Variables.ID2 = (int)_Row["ID2"];
                Variables.TranID = (int)_Row["TranID"];
                Variables.Flow = (string)_Row["Flow"];
                Variables.StockID = (int)_Row["Stock"];
                Variables.Qty = (decimal)_Row["Qty"];
                Variables.Rate = (decimal)_Row["Rate"];
                Variables.Remarks2 = (string)_Row["Remarks2"];
            }
        }

        private bool GetError()
        {
            if (ErrorMessages.Count > 0) { return true; }
            return false;
        }


        #endregion
    }

    public class Parameters
    {
        public string UserName { get; set; } = string.Empty;
        public int ID1 { get; set; }
        public int ID2 { get; set; }
        public string Vou_No { get; set; }
        public DateTime Vou_Date { get; set; }
        public int TranID { get; set; }
        public string Batch { get; set; }
        public string Remarks1 { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public string Remarks2 { get; set; } = string.Empty;
        public string Flow { get; set; }
        public int StockID { get; set; }
        public decimal Qty { get; set; }
        public int UOM { get; set; }
        public decimal Rate { get; set; }
        public string Status { get; set; }
        public decimal Amount => Qty * Rate;


        // Non DataBase Parameters
        public string NumberFormat => AppRegistry.Currency6d;
        public string QtyFormat => Qty.ToString(NumberFormat);
        public string RateFormat => Rate.ToString(NumberFormat);
        public string AmountFormat => Amount.ToString(NumberFormat);
        public string StockTitle => GetStockTitle();
        public string UOMTitle => GetUOMTitle();

        private string GetStockTitle()
        {
            try
            {
                if (UserName.Length > 0)
                {
                    return AppFunctions.GetTitle(UserName, Tables.Inventory, StockID);
                }
            }
            catch (Exception)
            {
                return "";
            }
            return "";
        }

        private string GetUOMTitle()
        {
            try
            {
                if (UserName.Length > 0)
                {
                    return AppFunctions.GetTitle(UserName, Tables.Inv_UOM, UOM);
                }
                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
