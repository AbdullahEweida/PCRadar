using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace PCRadar.Models
{
    public class Product
    {
        [Key]
        public Guid ID { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(255, ErrorMessage = "Name cannot be longer than 255 characters.")]
        public string Name { get; set; }

        public ProductCategory Category { get; set; }

        public ICollection<ProductStore> ProductStores { get; set; }
    }
}