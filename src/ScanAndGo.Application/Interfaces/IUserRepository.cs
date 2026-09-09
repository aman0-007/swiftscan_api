using ScanAndGo.Domain.Entities;

namespace ScanAndGo.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<int> CreateAsync(User user);
    }
}