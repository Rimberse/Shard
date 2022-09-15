using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Shard.API.Models
{
    public class ShardContext : DbContext
    {
        public ShardContext(DbContextOptions<ShardContext> options) : base(options)
        {
        }

        public DbSet<System> systems { get; set; } = null!;
    }
}