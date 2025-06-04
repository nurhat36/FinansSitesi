using FinansSitesi.Models;
using Microsoft.AspNetCore.Identity;
using System;
namespace FinansSitesi.Models { 
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public string ProfileImageUrl { get; set; }
    
    }
}