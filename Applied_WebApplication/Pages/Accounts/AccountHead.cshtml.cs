using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using static Applied_WebApplication.Data.MessageClass;

namespace Applied_WebApplication.Pages.Accounts
{
    [Authorize]
    public class AccountHeadModel : PageModel
    {
        [BindProperty]
        public AccounHead Record { get; set; } = new();
        public string MyPageAction { get; set; } = "Add";
        public int RecordID = 0;
        public bool IsError;
        public string UserName => User.Identity.Name;
        public List<Message> ErrorMessages = new();

        public void OnPostAdd()
        {
            MyPageAction = "Add";
            Record = new AccounHead();
        }

        public void OnGetEdit(int id)
        {

            MyPageAction = "Edit";
            RecordID = id;
            DataTableClass COA = new(UserName, Tables.COA);
            COA.SeekRecord(RecordID);
            Record.ID = (int)COA.CurrentRow["ID"];
            Record.Code = (string)COA.CurrentRow["Code"];
            Record.Title = (string)COA.CurrentRow["Title"];
            Record.Class = (int)COA.CurrentRow["Class"];
            Record.Nature = (int)COA.CurrentRow["Nature"];
            Record.Notes = (int)COA.CurrentRow["Notes"];
            Record.OPENING_BALANCE = (decimal)COA.CurrentRow["OPENING_BALANCE"];
        }

        public void OnGetDelete(int id)
        {

            MyPageAction = "Delete";
            RecordID = id;
            DataTableClass COA = new(UserName, Tables.COA);
            COA.SeekRecord(RecordID);
            Record.ID = (int)COA.CurrentRow["ID"];
            Record.Code = (string)COA.CurrentRow["Code"];
            Record.Title = (string)COA.CurrentRow["Title"];
            Record.Class = (int)COA.CurrentRow["Class"];
            Record.Nature = (int)COA.CurrentRow["Nature"];
            Record.Notes = (int)COA.CurrentRow["Notes"];
            Record.OPENING_BALANCE = (decimal)COA.CurrentRow["OPENING_BALANCE"];
        }

        public IActionResult OnPostSave(int id)
        {
            try
            {
                RecordID = id;

                DataTableClass COA = new(UserName, Tables.COA);
                if (COA.Seek(RecordID))
                {
                    COA.SeekRecord(RecordID);
                }
                else
                {
                    COA.NewRecord();
                    COA.CurrentRow["ID"] = 0;
                }

                COA.CurrentRow["Code"] = Record.Code;
                COA.CurrentRow["Title"] = Record.Title;
                COA.CurrentRow["Class"] = Record.Class;
                COA.CurrentRow["Nature"] = Record.Nature;
                COA.CurrentRow["Notes"] = Record.Notes;
                COA.CurrentRow["Opening_Balance"] = Record.OPENING_BALANCE;

                COA.Save();
                ErrorMessages = COA.TableValidation.MyMessages;
                if (ErrorMessages.Count == 0)
                {
                    ErrorMessages.Add(MessageClass.SetMessage("Record has been saved", Color.Green));
                    //return RedirectToPage("../Accounts/Directory/COA"); 

                }
                else { IsError = true; }

            }
            catch (Exception e)
            {
                ErrorMessages.Add(MessageClass.SetMessage(e.Message, Color.Red));
            }
            return Page();

        }

        public IActionResult OnPostDelete(AccounHead _Record)
        {
            RecordID = _Record.ID;
            DataTableClass COA = new(UserName, Tables.COA);
            if (COA.Seek(RecordID))
            {
                COA.SeekRecord(RecordID);                   // Assign a record for delete
                COA.Delete();                                           // Delete a record.
                return RedirectToPage("./Directory/COA");
            }

            return Page();
        }


        public class AccounHead
        {
            public int ID { get; set; }
            [Required]
            public string Code { get; set; }
            [Required]
            public string Title { get; set; }
            public int Nature { get; set; }
            public int Class { get; set; }
            public int Notes { get; set; }
            public decimal OPENING_BALANCE { get; set; }
        }
    }
}
