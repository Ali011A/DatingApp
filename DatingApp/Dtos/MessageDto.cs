﻿using DatingApp.Entities;

namespace DatingApp.Dtos
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public required string SenderUsername { get; set; }
           public required string SenderPhotoUrl { get; set; }
        public int RecipientId { get; set; }
        public required string RecipientUsername { get; set; }
        public required   string RecipientPhotoUrl { get; set; }
        public required string Content { get; set; }
       // public bool IsRead { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime MessageSent { get; set; } 
        public bool SenderDeleted { get; set; }
        public bool RecipientDeleted { get; set; }
        //navigation properties
       
    }
}
