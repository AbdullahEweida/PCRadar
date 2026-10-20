using System.ComponentModel.DataAnnotations;

namespace PCRadar.Models
{
    public class TrackedCategory
    {
        [Key]
        public Guid ID { get; set; }

        public Guid StoreID { get; set; }
        public Store Store { get; set; }

        public ProductCategory Category { get; set; }
        public string CategoryUrl { get; set; }
    }
}