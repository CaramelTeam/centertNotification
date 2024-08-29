using BOM_centerNotification.Models;
using Microsoft.EntityFrameworkCore;
namespace BOM_centerNotification.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<DTO_QueueMessage> DTO_QueueMessage { get; set; }
    }
}
