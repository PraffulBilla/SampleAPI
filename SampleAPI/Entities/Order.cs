using System.ComponentModel.DataAnnotations;

namespace SampleAPI.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderName { get; set; }
        public string OrderDescription { get; set; }
        public DateTime EntryDate { get; set; } = DateTime.UtcNow;
        public bool IsInvoiced { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }

}
