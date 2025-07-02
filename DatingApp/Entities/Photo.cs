using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DatingApp.Entities
{
    [Table("Photos")]
    public class Photo
    {
        public int Id { get; set; }
        public required string Url { get; set; }
        public bool IsMain { get; set; }
        public string? PublicId { get; set; }
        public bool IsApproved { get; set; } = false;
        public AppUser AppUser { get; set; }=null!;
        public int AppUserId { get; set; }
        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        //public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        //public void UpdateTimestamp()
        //{
        //    UpdatedAt = DateTime.UtcNow;
        //}



    }
}