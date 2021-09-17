using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis;
using SocialNetWork.Models;

namespace SocialNetWork.Controllers
{
    public class HomeController : Controller
    {
       
        private ApplicationContext db;
        //в Start разобраться с менедежерами
        public HomeController (ApplicationContext context)
        {
            db = context;
            //_userManager = userManager;
            /*_signInManager = signInManager;
            db = context;
            _roleManager = manager;*/ }
        
        
        /*public async Task<IActionResult> GetRoles()
        {
            db.Add(new IdentityRole("simpleUser"));
            await db.SaveChangesAsync();
            return View(await db.Roles.ToListAsync());
        }*/
        

        public IActionResult Data()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View();
            }         
            else
            {
                return View("Index");
            }
            
        }
        
     
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
       
    
        public async Task<IActionResult> Index()
        {
            CheckingBlock();
            //CheckingBlock();
            return View(await db.Users.ToListAsync()); 
        }
        public IActionResult Create() //Views/Home добавим новое представление Create.cshtml:
        {
            return View();
            
        }
        
        [HttpPost]
        public async Task <IActionResult> Create(User user)
        {
            db.Users.Add(user); //db.Users.Add() для данных из объекта user формируется sql-выражение INSERT
            await db.SaveChangesAsync(); //выполняет это выражение, тем самым добавляя данные в базу данных.
            return RedirectToAction("Index");
        }
        
        /*//вывод информации о модели
        public async Task<IActionResult> Details(int? id)
        {
            if (id != null)
            {
                User user = await db.Users.FirstOrDefaultAsync(p => p.Id == id);
                if (user != null)
                    return View(user);
            }
            return NotFound();
        }*/
        //
        //Редактирование объекта
        /*public async Task<IActionResult> Edit(int? id)
        {
            if(id!=null)
            {
                User user = await db.Users.FirstOrDefaultAsync(p=>p.Id==id);
                if (user != null)
                    return View(user);
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> Edit(User user)
        {
            db.Users.Update(user);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }*/
        /////////////
        ///
        
        ////Удаление
        
        /*[HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id) //извлекается из бд и передается в представление
        {
            if (id != null)
            {
                User user = await db.Users.FirstOrDefaultAsync(p => p.Id == id);
                if (user != null)
                    return View(user);
            }
            return NotFound();
        }*/
 
        /*[HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                User user = await db.Users.FirstOrDefaultAsync(p => p.Id == id);
                if (user != null)
                {
                    db.Users.Remove(user); //Данный метод генерирует sql-выражение DELETE,
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            return NotFound();
        }*/
        ///////
        ///
        
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
        
       
        public ActionResult UpdateDB(string values, string nameOfAction)
        {
            List<int> replyId = values.Split(',').Select(int.Parse).ToList();
            foreach (var id in replyId)
            {
                User user = db.Users.SingleOrDefault(u => u.Id == id);
                if (nameOfAction == "Block") Block(user);
                else if (nameOfAction == "Unblock") UnBlock(user);
                else if (nameOfAction == "Delete") Delete(user);
            }
            return RedirectToAction("Index");
        }
        
        public virtual ActionResult UpdateLastActivityDate(string userName)
        {
            User user = db.Users.FirstOrDefault(u => u.Name == userName);
            if(user!=null) user.LastActivityTime = new DateTimeOffset(DateTime.Now).ToString();
            //b.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            return new EmptyResult();
        }
        
        
        public ActionResult CheckingBlock()
        {
            var user = db.Users.SingleOrDefault(b => b.Name == User.Identity.Name);
            if (user is null or {Status: "Blocked"}) SignOut();
            return RedirectToAction("Index");
        }

    }
}