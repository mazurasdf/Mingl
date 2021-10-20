using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mingl.Models
{
    public class Conversation
    {
        [Key]
        public int ConversationId {get;set;}
        public int SenderId {get;set;}
        public int ReceiverId {get;set;}

        [ForeignKey("SenderId")]
        [InverseProperty("ConversationsSent")]
        public User Sender {get;set;}
        [ForeignKey("ReceiverId")]
        [InverseProperty("ConversationsReceived")]
        public User Reveiver {get;set;}

        public int? Location1Id {get;set;}
        [ForeignKey("Location1Id")]
        [InverseProperty("ConversationsInvolved1")]
        public Location Location1 {get;set;}

        public List<Message> Messages{get;set;}

        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;
    }
}