using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace PCRadar.Models
{
    public class Store
    {
        [Key]
        public Guid ID { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Website URL is required")]
        public string WebsiteUrl { get; set; }

        public string CardXPath { get; set; }
        public string NameXPath { get; set; }
        public string PriceXPath { get; set; } 
        public string LinkXPath { get; set; }
        public string AvailabilityXPath { get; set; }

        public ICollection<ProductStore> ProductStores { get; set; }
    }
}