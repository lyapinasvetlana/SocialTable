using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace SocialNetWork.Models
{
    public class User
    {
        public int Id { get; set; }
        
        public string IdThirdPartyApp { get; set; }
        public string Name { get; set; }
        public string NetWork { get; set; }
        
        public string FirstEntryTime { get; set; }
        public string LastActivityTime { get; set; }
        public string Status { get; set; }
        
        
    }
}