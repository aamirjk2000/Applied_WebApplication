using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Applied_WebApplication.Pages
{
   
    public class AccountsModel : PageModel
    {
        public IActionResult OnGetSubmit(string? _UserName)
        {
            return RedirectToPage("COA_Add", _UserName);
        }

    }
}
