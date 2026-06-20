using Microsoft.EntityFrameworkCore;
using MyOwnChatApi.Domain.Models;

namespace MyOwnChatApi.Context
{
    public class MyContext:DbContext
    {
        public MyContext(DbContextOptions<MyContext> options): base(options) { }

        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
