using Microsoft.EntityFrameworkCore;

namespace Common
{
    public partial class LogContext : DbContext
    {
        public LogContext()
        {

        }
        public virtual DbSet<miniMessanger.Models.LogMessage> Logs { get; set; }
        protected override void OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(Config.GetDatabaseConfigConnection());
            }   
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<miniMessanger.Models.LogMessage>(entity =>
            {
                entity.HasKey(e => e.log_id)
                    .HasName("PRIMARY");

                entity.ToTable("logs");

                entity.Property(e => e.log_id)
                    .HasColumnName("log_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.message)
                    .HasColumnName("message")
                    .HasColumnType("varchar(2000)");

                entity.Property(e => e.user_computer)
                    .HasColumnName("user_computer")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.time)
                    .HasColumnName("time")
                    .HasColumnType("DATETIME");
                
                entity.Property(e => e.level)
                    .HasColumnName("level")
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.user_id)
                    .HasColumnName("user_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.thread_id)
                    .HasColumnName("thread_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.user_ip)
                    .HasColumnName("user_ip")
                    .HasColumnType("varchar(20)");
            });
        }
    }
}