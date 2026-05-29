using Microsoft.EntityFrameworkCore;
using TheUnraveller.Core.Entities;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Infrastructure.Data;

namespace TheUnraveller.Infrastructure.Repositories;

public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId) =>
        await _dbSet.Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
}
