namespace ScanAndGo.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}