using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace PCRadar.Models
{
    public class ProductStore
    {
        [Key]
        public Guid ID { get; set; } 

        public Guid ProductID { get; set; }
        public Product Product { get; set; }

        public Guid StoreID { get; set; }
        public Store Store { get; set; }

        [Required(ErrorMessage = "Scrape URL is required")]
        public string ScrapeUrl { get; set; }

        public ICollection<PriceHistory> PriceHistories { get; set; }
    }
}