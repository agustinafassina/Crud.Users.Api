using Microsoft.EntityFrameworkCore;
using UsersApi.Configurations.ClientsDB.SqlServer.Dto;

namespace UsersApi.Configurations.ClientsDB.SqlServer
{
    public class UserDbContext : DbContext
    {
        public UserDbContext() : base() { }

        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        public DbSet<UserDtoContext> Users { get; set; }
        public DbSet<StatusDtoContext> Status { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserDtoContext>()
                .HasOne(u => u.Status)
                .WithMany()
                .HasForeignKey(u => u.StatusId);

            modelBuilder.Entity<StatusDtoContext>().HasData(
                new StatusDtoContext { Id = 1, Name = "Disable" },
                new StatusDtoContext { Id = 2, Name = "Enable" }
            );

            modelBuilder.Entity<UserDtoContext>()
                .HasIndex(u => u.Email)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}