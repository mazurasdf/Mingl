using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Mingl.Models
{
    public class ViewUser
    {
        public int UserId {get;set;}
        [Required]
        [EmailAddress]
        public string Email {get;set;}
        [Required]
        public string Name {get;set;}
        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage="You need at least 8 characters for the password!")]
        public string Password {get;set;}
        [NotMapped]
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword {get;set;}
        [Required]
        public string Gender {get;set;}
        [Range(1,3)]
        //1 for "show me everyone", 2 for "show me women", 3 for "show me men" 
        public int PreferredUsers {get;set;}
        public string Likes {get;set;}
        public string Bio {get;set;}
        [Required]
        public IFormFile ProfilePicUrl {get;set;}
        //date type 1
        [Required]
        public bool DatePhysical {get;set;}
        //date type 2
        [Required]
        public bool DateCasual {get;set;}
        //date type 3
        [Required]
        public bool DateFood {get;set;}
        //date type 4
        [Required]
        public bool DateCoffee {get;set;}
        //date type 5
        [Required]
        public bool DateBar {get;set;}
    }
}