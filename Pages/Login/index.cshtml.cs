using G_AUTHORIZATION.Helpers;
using G_ON_WEBAPP.Models;
using G_ON_WEBAPP.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Web;

namespace G_ON_WEBAPP.Pages.Login
{
    public class IndexModel : PageModel
    {
        private readonly Models.AppSettings _appSettings;

        public IndexModel(IOptions<Models.AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public List<Translation> trans_g_on { get; set; }
        public List<Translation> trans_g_us { get; set; }
        private string lang { get; set; }
        private string g_us { get; set; }
        private string g_trs { get; set; }

        public string msg { get; set; }
        public string sTitleLogin { get; set; }
        public string sContentLogin { get; set; }
        public string sPassword { get; set; }
        public string sForgotPassword { get; set; }
        public string sSubmit { get; set; }

        public void OnGet()
        {
            if (!string.IsNullOrWhiteSpace(HttpContext.Request.Cookies["gtk"]))
                Response.Redirect("/");
            else
            {
                LoadLoginPageAsync();
            }
        }

        public async void OnPost()
        {
            LoadLoginPageAsync();

            if (string.IsNullOrWhiteSpace(Request.Form["email"]) || string.IsNullOrWhiteSpace(Request.Form["password"]))
                msg = TranslationService.GetText(trans_g_on, "g_on_login_email_hoac_mat_khau_khong_duoc_rong");
            else
            {
                try
                {
                    trans_g_us = await TranslationService.Translator(this, g_trs, "g-us", lang, "trs_g-us");
                    var apiResponse = UserService.Authlogin(g_us, Request.Form["email"], Request.Form["password"]);
                    if (apiResponse.StatusCode == 200)
                    {
                        var authenticate = JsonSerializer.Deserialize<AuthenticateResponse>(apiResponse.Data);
                        Cookie.Set(this, "gtk", authenticate.jwtToken);
                        Cookie.Set(this, "gtkid", await HashText.EncryptString(authenticate.tokenID.ToString()));
                        Cookie.Set(this, "UserID", await HashText.EncryptString(authenticate.id.ToString()));
                        Cookie.Set(this, "Company", await HashText.EncryptString(authenticate.company.ToString()));
                        Cookie.Set(this, "FullName", await HashText.EncryptString($"{authenticate.lastName} {authenticate.firstName}"));
                        Cookie.Set(this, "UserName", await HashText.EncryptString(authenticate.username));
                        Cookie.Set(this, "lang", authenticate.language);
                        Cookie.Set(this, "UserRl", await HashText.EncryptString(authenticate.userRoles));

                        if (authenticate.servers != null)
                        {
                            foreach (var server in authenticate.servers)
                            {
                                Cookie.Set(this, server.appCode, await HashText.EncryptString(server.hostURL));
                            }
                        }
                        msg = String.Empty;
                        if (string.IsNullOrWhiteSpace(Request.Query["url"].ToString()))
                            Response.Redirect("/");
                        else
                            Response.Redirect(HttpUtility.UrlDecode(Request.Query["url"].ToString()));

                    }
                    else
                        msg = TranslationService.GetText(trans_g_us, apiResponse.Message);
                }
                catch { }
            }
        }

        private async Task LoadLoginPageAsync()
        {
            g_us = await HashText.DecryptString(Cookie.Get(this, "g-us", _appSettings.g_us));
            g_trs = await HashText.DecryptString(Cookie.Get(this, "g-trs", _appSettings.g_trs));
            lang = Cookie.Get(this, "lang", "vi");

            trans_g_on = await TranslationService.Translator(this, g_trs, "g-on", lang, "trs_g-on");

            sTitleLogin = TranslationService.GetText(trans_g_on, "g_on_login_tieu_de_trang_dang_nhap");
            sContentLogin = TranslationService.GetText(trans_g_on, "g_on_login_noi_dung_trang_dang_nhap");
            sPassword = TranslationService.GetText(trans_g_on, "g_on_login_mat_khau");
            sForgotPassword = TranslationService.GetText(trans_g_on, "g_on_login_quen_mat_khau");
            sSubmit = TranslationService.GetText(trans_g_on, "g_on_login_nut_dang_nhap");
        }
    }
}
