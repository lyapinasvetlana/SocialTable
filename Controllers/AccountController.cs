using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task Login(string yahoo,string google, string github)
        {
            
            if (!string.IsNullOrEmpty(google)) { await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties(){RedirectUri = Url.Action("Response")});}
            else if (!string.IsNullOrEmpty(yahoo)) { await HttpContext.ChallengeAsync(YahooAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties() { RedirectUri = Url.Action("Response")}); }
            else if (!string.IsNullOrEmpty(github)) { await HttpContext.ChallengeAsync(GitHubAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties() {RedirectUri = Url.Action("Response")}); }

            ;
        }
                
        public async Task<IActionResult> Response()
        {
            
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new {claim.OriginalIssuer, claim.Value}).ToArray();
            string date = new DateTimeOffset(DateTime.Now).ToString();
            User user = new User {IdThirdPartyApp = claims[0].Value, Name = User.Identity.Name, NetWork = claims[0].OriginalIssuer, Status = "Active", FirstEntryTime = date, LastActivityTime = date};
            /*var replyInBase = db.Users.FirstOrDefault(b => b.IdThirdPartyApp == user.IdThirdPartyApp);
            if(replyInBase != null) { replyInBase.Name = user.Name; replyInBase.NetWork = user.NetWork; replyInBase.Status = user.Status; replyInBase.LastActivityTime = date;
            }
            else
            {
                db.Users.Add(user);
            }
            await db.SaveChangesAsync();*/
            
            /*ClaimsPrincipal currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;*/
            
            await InsertOrUpdate(user);
            return Redirect("~/Home/Data");
        }

        public async Task<IActionResult> InsertOrUpdate(User user)
        {
            var replyInBase = db.Users.FirstOrDefault(b => b.IdThirdPartyApp == user.IdThirdPartyApp);
            if(replyInBase != null) { replyInBase.Name = user.Name; replyInBase.NetWork = user.NetWork; replyInBase.Status = user.Status; replyInBase.LastActivityTime = new DateTimeOffset(DateTime.Now).ToString();}
            else
            {
                db.Users.Add(user);
            }
            await db.SaveChangesAsync();
            return new EmptyResult();
        }
        
        
        public string Serialize(string userName)
        {
            return JsonConvert.SerializeObject(db.Users, Formatting.Indented, new JsonSerializerSettings{ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
            
        }
        
        
        public string  NumberOfUsers()
        {
            var numberUsers = new List<int>();
            
                foreach (var item in new string[] {"Google", "GitHub", "Yahoo"})
                {
                    numberUsers.Add(db.Users.Count(p => p.NetWork == item)); 
                }
            
            return JsonConvert.SerializeObject(numberUsers);
        }
        
        
        public ActionResult CheckingBlock()
        {
            var user = db.Users.SingleOrDefault(b => b.Name == User.Identity.Name);
            if (user is {Status: "Blocked"}) SignOut();
            return Redirect("~/Home/Index");
        }
    }
}