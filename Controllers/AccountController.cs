using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OAuth.GitHub;
using AspNet.Security.OAuth.Yahoo;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SocialNetWork.Models;

namespace SocialNetWork.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private ApplicationContext db;
        
        public AccountController(ApplicationContext context)
        {
            db = context;
        }

        
        [HttpPost]
        public async Task<IActionResult> Login(string yahoo,string google, string github)
        {
            if (!string.IsNullOrEmpty(google)) { await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties(){RedirectUri = Url.Action("Response")});}
            else if (!string.IsNullOrEmpty(yahoo)) { await HttpContext.ChallengeAsync(YahooAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties() { RedirectUri = Url.Action("Response")}); }
            else if (!string.IsNullOrEmpty(github)) { await HttpContext.ChallengeAsync(GitHubAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties() {RedirectUri = Url.Action("Response")}); }
            return new EmptyResult();
        }

        public async Task<IActionResult> Response()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new {claim.OriginalIssuer, claim.Value}).ToArray();
            string date = new DateTimeOffset(DateTime.Now).ToString();
            User user = new User {IdThirdPartyApp = claims[0].Value, Name = User.Identity.Name, NetWork = claims[0].OriginalIssuer, Status = "Active", FirstEntryTime = date, LastActivityTime = date};
            InsertOrUpdate(user);
            return CheckingBlock(InsertOrUpdate(user)) ? Redirect("/") : Redirect("~/Home/Data");
        }
        
        public string InsertOrUpdate(User user)
        {
            var replyInBase = db.Users.FirstOrDefault(b => b.IdThirdPartyApp == user.IdThirdPartyApp);
            if (replyInBase != null) { replyInBase.Name = user.Name; replyInBase.NetWork = user.NetWork;}
            else
            {
                db.Users.Add(user);
                return user.Status;
            }
            return replyInBase.Status;
        }
        
        public string Serialize(string userName)
        {
            return JsonConvert.SerializeObject(db.Users, Formatting.Indented, new JsonSerializerSettings{ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
            
        }
        
        public string  NumberOfUsers()
        {
            var numberUsers = new string[] {"Google", "GitHub", "Yahoo"}.Select(item => db.Users.Count(p => p.NetWork == item)).ToList();
            return JsonConvert.SerializeObject(numberUsers);
        }
        
        public bool CheckingBlock(string status)
        {
            db.SaveChanges();
            return status == "Blocked";
        }

    }
}