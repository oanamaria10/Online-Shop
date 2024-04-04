using System.ComponentModel.DataAnnotations;

namespace proiect.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Numele categoriei este obligatoriu")]
        public string CategoryName { get; set; }

        // o categorie poate avea o colectie de produse
        public virtual ICollection<Product>? Products { get; set; }
    }
}
