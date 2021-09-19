using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetWork.Models;

namespace SocialNetWork.Controllers
{
   
    public class HomeController : Controller
    {
       
        private ApplicationContext db;
        public HomeController (ApplicationContext context)
        {
            db = context; 
        }
        public IActionResult Index()
        {
            return View(db.Users.ToList()); 
        }

        [Authorize]
        public IActionResult Data()
        {
            
            return User is {Identity: {IsAuthenticated: true}} ? View() : View("Index");
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
        
        [HttpPost]
        public ActionResult Block(User user)
        {
            user.Status = "Blocked";
            db.SaveChanges();
            return new EmptyResult();
        }
        
        public ActionResult UnBlock(User user)
        {
            user.Status = "Active";
            db.SaveChanges();
            return new EmptyResult();
        }
        
        [HttpPost]
        public ActionResult Delete(User user)
        {
            db.Entry(user).State = EntityState.Deleted;
            db.SaveChanges();
            return new EmptyResult();
        }
        
        [HttpPost]
        public async Task<IActionResult> UpdateDB(string values, string nameOfAction)
        {
            bool statusOfBlock = false;
            List<int> replyId = values.Split(',').Select(int.Parse).ToList();
            foreach (var id in replyId)
            {
                User user = db.Users.SingleOrDefault(u => u.Id == id);
                if (nameOfAction == "Block") Block(user);
                else if (nameOfAction == "Unblock") UnBlock(user);
                else if (nameOfAction == "Delete") Delete(user);
                statusOfBlock = CheckingBlock(user, statusOfBlock);
            }
            return await ReturnJsonLink(statusOfBlock);
        }

        public virtual ActionResult UpdateLastActivityDate(string userName)
        {
            User user = db.Users.FirstOrDefault(u => u.Name == userName);
            if (user != null)
            {
                user.LastActivityTime = new DateTimeOffset(DateTime.Now).ToString();
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
            return new EmptyResult();
        }
        
        private bool CheckingBlock(User user, bool status)
        {
            ClaimsPrincipal currentUser = this.User;
            var currentUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userCurrent = db.Users.FirstOrDefault(b => b.IdThirdPartyApp == currentUserId);
            return user.IdThirdPartyApp == currentUserId && (user.Status == "Blocked"  || userCurrent == null) || status;
        }

        public async Task<IActionResult>  ReturnJsonLink(bool statusOfBlock)
        {
            if (statusOfBlock)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                string redirectUrl = Url.Action("Index");
                return Json(new { redirectUrl });
            }
            else
            {
                string redirectUrl = Url.Action("Data");
                return Json(new {redirectUrl});
            }
        }
    }
}