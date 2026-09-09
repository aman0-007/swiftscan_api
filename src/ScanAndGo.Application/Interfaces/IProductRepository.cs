using ScanAndGo.Domain.Entities;

namespace ScanAndGo.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id);
        Task<Product?> GetByBarcodeAsync(string barcode);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<int> CreateAsync(Product product);
        Task UpdateStockAsync(Guid productId, int newStockQuantity);
    }
}