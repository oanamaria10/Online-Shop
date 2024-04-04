using System.ComponentModel.DataAnnotations;

namespace proiect.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "Continutul review-ului este obligatoriu")]
        public string Content { get; set; }
        public DateTime Date { get; set; }

        // user-ul lasa un rating la produs
        public int Rating { get; set; }

        // un review apartine unui produs
        public int? ProductId { get; set; }
        public virtual Product? Product { get; set; }

        // un review este postat de catre un user
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

    }
}
