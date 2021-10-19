using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mingl.Models
{
    public class Message
    {
        //text, sender, conversation
        [Key]
        public int MessageId {get;set;}
        [Required]
        public string Text {get;set;}

        public int UserId {get;set;}
        public User Sender {get;set;}
        public int ConversationId {get;set;}
        public Conversation Conversation {get;set;}
        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;
    }
}