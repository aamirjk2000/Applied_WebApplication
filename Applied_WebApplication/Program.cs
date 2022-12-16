using Applied_WebApplication;
using Applied_WebApplication.Data;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation(); ;
builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth", options =>
{
    options.Cookie.Name = "MyCookieAuth";
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Logout";

});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", Policy => Policy.RequireClaim("Admin"));
    options.AddPolicy("AccountsManager", Policy => Policy.RequireClaim("AccountsManager"));
    options.AddPolicy("AccountsAssistant", policy => policy.RequireClaim("AccountAssistants"));
    options.AddPolicy("MustBelongToHRPolicy", policy => policy.RequireClaim("Department", "HR"));
    options.AddPolicy("StoreOnly", policy => policy.RequireClaim("Stock", "Store"));
    options.AddPolicy("Client", policy => policy.RequireClaim("Client", "Customer"));

});

//builder.Services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}




app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseCreateDatabase();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
