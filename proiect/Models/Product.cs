using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace proiect.Models
{
    public class Product
    {

        [Key]
        public int ProductId { get; set; }
        
        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [StringLength(100, ErrorMessage = "Titlul nu poate avea mai mult de 100 de caractere")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Descrierea produsului este obligatorie")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public string? Image { get; set; }

        [Required(ErrorMessage = "Pretul produsului este obligatoriu")]
        public int Price { get; set; }

        [Required]
        public int FinalRating { get; set; }

        public DateTime Date { get; set; }

        // un produs poate avea o colectie de review-uri
        public virtual ICollection<Review>? Reviews { get; set; }

        // un produs este postat de catre un user
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        // un produs are asociata o categorie
        [Required(ErrorMessage = "Categoria este obligatorie")]
        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        [NotMapped] 
        public IEnumerable<SelectListItem>? Categ { get; set; }

        // un produs trebuie aprobat de catre adminstrator
        public bool Approved { get; set; }
    }
}
