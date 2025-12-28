using System.Data;
using BlastTicket.Core.Interfaces;
using BlastTicket.Core.Models;
using BlastTicket.Infra.Data;
using Dapper;

namespace BlastTicket.Infra.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public OrderRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task CreateAsync(Order order)
        {
            using var conn = _dbFactory.CreateConnection();

            const string sql = @"
                INSERT INTO orders (id, item_id, user_id, quantity, created_at)
                VALUES (@Id, @ItemId, @UserId, @Quantity, @CreatedAt);
            ";
            await conn.ExecuteAsync(sql, order);
        }
    }
}
