namespace B_BW1_Prog.Models
{
    public class ProductModel
    {
        public Guid Id { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public decimal? price { get; set; }
        public int? inStock { get; set; }
    }
}