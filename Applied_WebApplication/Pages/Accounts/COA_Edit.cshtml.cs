using Applied_WebApplication.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace Applied_WebApplication.Pages.Accounts
{
    public class COA_EditModel : PageModel
    {
        public DataTableClass COA; // = new DataTableClass(Tables.COA.ToString());
        public DataTableClass COA_Nature; // = new DataTableClass(Tables.COA_Nature.ToString());
        public DataTableClass COA_Class; // = new DataTableClass(Tables.COA_Class.ToString());
        public DataTableClass COA_Notes; // = new DataTableClass(Tables.COA_Notes.ToString());
        public TableValidationClass Validation;
        public Record _Record  = new Record();
        public string Title_Nature, Title_Class, Title_Notes;
        public int Test;


        public IActionResult OnGetEdit(int id, string UserName)
        {
            COA = new DataTableClass(UserName,Tables.COA.ToString());
            COA_Nature = new DataTableClass(UserName, Tables.COA_Nature.ToString());
            COA_Class = new DataTableClass(UserName, Tables.COA_Class.ToString());
            COA_Notes = new DataTableClass(UserName, Tables.COA_Notes.ToString());
            
            COA.CurrentRow = COA.SeekRecord(id);
            _Record.ID = (int)COA.CurrentRow["ID"];
            _Record.Code = (string)COA.CurrentRow["Code"];
            _Record.Title = (string)COA.CurrentRow["Title"];
            _Record.COA_Nature = (int)COA.CurrentRow["Nature"];
            _Record.COA_Class = (int)COA.CurrentRow["Class"];
            _Record.COA_Notes = (int)COA.CurrentRow["Notes"];
            _Record.OBal = (decimal)COA.CurrentRow["Opening_Balance"];

            Title_Class = COA_Class.Title(_Record.COA_Class);
            Title_Nature = COA_Nature.Title(_Record.COA_Nature);
            Title_Notes = COA_Notes.Title(_Record.COA_Notes);

            return Page();
        }

        public IActionResult OnPostSubmit(Record? _FillRecord)
        {
            int _RecID = _FillRecord.ID;
            COA.CurrentRow = COA.SeekRecord(_RecID);

            if (ModelState.IsValid)
            {
                Validation = new();
                //COA.SeekRecord(_RecID);
                COA.CurrentRow["ID"] = COA.CurrentRow["ID"];
                COA.CurrentRow["Code"] = _FillRecord.Code;
                COA.CurrentRow["Title"] = _FillRecord.Title;
                COA.CurrentRow["Nature"] = _FillRecord.COA_Nature;
                COA.CurrentRow["Class"] = _FillRecord.COA_Class;
                COA.CurrentRow["Notes"] = _FillRecord.COA_Notes;
                COA.CurrentRow["OPENING_BALANCE"] = _FillRecord.OBal;
                Validation = COA.Save();


                string i = this.Request.Form["ID"].ToString();
            }

            if (Validation.success)
            {
                return RedirectToPage("COA");
            }
            else
            {
                return Page();
            }
        }

        public IActionResult OnPostBack(Record _FillRecord)
        {
            return RedirectToPage("COA");
        }
        public class Record
        {
            public int ID { get; set; }
            public string Code { get; set; }
            public string Title { get; set; }
            public int COA_Nature { get; set; }
            public int COA_Class { get; set; }
            public int COA_Notes { get; set; }
            public decimal OBal { get; set; }

        }
    }
}