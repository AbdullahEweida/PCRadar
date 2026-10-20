using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PCRadar.Models
{
    public class PriceHistory
    {
        [Key]
        public Guid ID { get; set; }

        public Guid ProductStoreID { get; set; }
        public ProductStore ProductStore { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Column(TypeName = "decimal(18,2)")] 
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Availability is required")]
        public bool IsAvailable { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}