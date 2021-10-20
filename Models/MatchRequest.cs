using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mingl.Models
{
    public class MatchRequest
    {
        [Key]
        public int MatchRequestId {get;set;}
        public int SenderId {get;set;}
        public int ReceiverId {get;set;}

        [ForeignKey("SenderId")]
        [InverseProperty("RequestsSent")]
        public User Sender {get;set;}
        [ForeignKey("ReceiverId")]
        [InverseProperty("RequestsReceived")]
        public User Receiver {get;set;}

        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;
    }
}