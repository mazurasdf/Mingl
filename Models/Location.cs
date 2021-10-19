using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mingl.Models
{
    public class Location
    {
        //name, address, neighborhood, date type
        [Key]
        public int LocationId {get;set;}
        [Required]
        public string Name {get;set;}
        [Required]
        public string Address {get;set;}
        [Required]
        public string Neighborhood {get;set;}
        [Required]
        public string LocationType {get;set;}
        [Required]
        [Range(1,5)]
        public int DateType {get;set;}

        public List<Conversation> ConversationsInvolved1 {get;set;}

        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;
    }
}