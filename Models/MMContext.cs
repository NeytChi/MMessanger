using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace miniMessanger.Models
{
    public partial class MMContext : DbContext
    {
        private bool manual_control = false;
        public MMContext()
        {
        }
        public MMContext(bool manual_control)
        {
            this.manual_control = manual_control;
        }

        public MMContext(DbContextOptions<MMContext> options)
            : base(options)
        {
        }

        public virtual DbSet<BlockedUsers> BlockedUsers { get; set; }
        public virtual DbSet<Chatroom> Chatroom { get; set; }
        public virtual DbSet<Complaints> Complaints { get; set; }
        public virtual DbSet<Files> Files { get; set; }
        public virtual DbSet<miniMessanger.Models.LogMessage> Logs { get; set; }
        public virtual DbSet<Messages> Messages { get; set; }
        public virtual DbSet<Participants> Participants { get; set; }
        public virtual DbSet<Profiles> Profiles { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (manual_control)
            {
                if (!optionsBuilder.IsConfigured)
                {
                    //System.Console.WriteLine(Configuration.GetConnectionString("Instasoft"));
                    //optionsBuilder.UseMySql(ConfigurationManager.ConnectionStrings["Instasoft"].ConnectionString);
                    optionsBuilder.UseMySql(Common.Config.GetDatabaseConfigConnection());
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BlockedUsers>(entity =>
            {
                entity.HasKey(e => e.BlockedId)
                    .HasName("PRIMARY");

                entity.ToTable("blocked_users");

                entity.HasIndex(e => e.BlockedUserId)
                    .HasName("blocked_user_id");

                entity.HasIndex(e => e.UserId)
                    .HasName("user_id");

                entity.Property(e => e.BlockedId)
                    .HasColumnName("blocked_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BlockedDeleted)
                    .HasColumnName("blocked_deleted")
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.BlockedReason)
                    .HasColumnName("blocked_reason")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.BlockedUserId)
                    .HasColumnName("blocked_user_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.BlockedUser)
                    .WithMany(p => p.BlockedUsersBlockedUser)
                    .HasForeignKey(d => d.BlockedUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("blocked_users_ibfk_2");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.BlockedUsersUser)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("blocked_users_ibfk_1");
            });

            modelBuilder.Entity<Chatroom>(entity =>
            {
                entity.HasKey(e => e.ChatId)
                    .HasName("PRIMARY");

                entity.ToTable("chatroom");

                entity.Property(e => e.ChatId)
                    .HasColumnName("chat_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ChatToken)
                    .HasColumnName("chat_token")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'")
                    .ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<Complaints>(entity =>
            {
                entity.HasKey(e => e.ComplaintId)
                    .HasName("PRIMARY");

                entity.ToTable("complaints");

                entity.HasIndex(e => e.BlockedId)
                    .HasName("blocked_id");

                entity.HasIndex(e => e.MessageId)
                    .HasName("message_id");

                entity.HasIndex(e => e.UserId)
                    .HasName("user_id");

                entity.Property(e => e.ComplaintId)
                    .HasColumnName("complaint_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BlockedId)
                    .HasColumnName("blocked_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Complaint)
                    .HasColumnName("complaint")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.MessageId)
                    .HasColumnName("message_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Blocked)
                    .WithMany(p => p.Complaints)
                    .HasForeignKey(d => d.BlockedId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("complaints_ibfk_2");

                entity.HasOne(d => d.Message)
                    .WithMany(p => p.Complaints)
                    .HasForeignKey(d => d.MessageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("complaints_ibfk_3");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Complaints)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("complaints_ibfk_1");
            });

            modelBuilder.Entity<Files>(entity =>
            {
                entity.HasKey(e => e.FileId)
                    .HasName("PRIMARY");

                entity.ToTable("files");

                entity.Property(e => e.FileId)
                    .HasColumnName("file_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.FileExtension)
                    .HasColumnName("file_extension")
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.FileFullpath)
                    .HasColumnName("file_fullpath")
                    .HasColumnType("varchar(256)");

                entity.Property(e => e.FileLastName)
                    .HasColumnName("file_last_name")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasColumnName("file_name")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.FilePath)
                    .HasColumnName("file_path")
                    .HasColumnType("varchar(256)");

                entity.Property(e => e.FileType)
                    .HasColumnName("file_type")
                    .HasColumnType("varchar(10)");
            });

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

            modelBuilder.Entity<Messages>(entity =>
            {
                entity.HasKey(e => e.MessageId)
                    .HasName("PRIMARY");

                entity.ToTable("messages");

                entity.Property(e => e.MessageId)
                    .HasColumnName("message_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.ChatId)
                    .HasColumnName("chat_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.MessageText)
                    .HasColumnName("message_text")
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.MessageViewed)
                    .HasColumnName("message_viewed")
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("int(11)");
            });

            modelBuilder.Entity<Participants>(entity =>
            {
                entity.HasKey(e => e.ParticipantId)
                    .HasName("PRIMARY");

                entity.ToTable("participants");

                entity.Property(e => e.ParticipantId)
                    .HasColumnName("participant_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.ChatId)
                    .HasColumnName("chat_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.OpposideId)
                    .HasColumnName("opposide_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("int(11)");
            });

            modelBuilder.Entity<Profiles>(entity =>
            {
                entity.HasKey(e => e.ProfileId)
                    .HasName("PRIMARY");

                entity.ToTable("profiles");

                entity.HasIndex(e => e.UserId)
                    .HasName("user_id");

                entity.Property(e => e.ProfileId)
                    .HasColumnName("profile_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ProfileAge)
                    .HasColumnName("profile_age")
                    .HasColumnType("tinyint(3)");

                entity.Property(e => e.ProfileSex)
                    .HasColumnName("profile_sex")
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.UrlPhoto)
                    .HasColumnName("url_photo")
                    .HasColumnType("varchar(256)");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Profiles)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("profiles_ibfk_1");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PRIMARY");

                entity.ToTable("users");

                entity.HasIndex(e => e.UserEmail)
                    .HasName("user_email")
                    .IsUnique();

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Activate)
                    .HasColumnName("activate")
                    .HasColumnType("tinyint(4)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LastLoginAt)
                    .HasColumnName("last_login_at")
                    .HasColumnType("int(11)");

                entity.Property(e => e.RecoveryCode)
                    .HasColumnName("recovery_code")
                    .HasColumnType("int(11)");

                entity.Property(e => e.RecoveryToken)
                    .HasColumnName("recovery_token")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.UserEmail)
                    .HasColumnName("user_email")
                    .HasColumnType("varchar(256)");

                entity.Property(e => e.UserHash)
                    .HasColumnName("user_hash")
                    .HasColumnType("varchar(120)");

                entity.Property(e => e.UserLogin)
                    .HasColumnName("user_login")
                    .HasColumnType("varchar(256)");

                entity.Property(e => e.UserPassword)
                    .HasColumnName("user_password")
                    .HasColumnType("varchar(256)");

                entity.Property(e => e.UserPublicToken)
                    .HasColumnName("user_public_token")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.UserToken)
                    .HasColumnName("user_token")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.UserType)
                    .HasColumnName("user_type")
                    .HasColumnType("varchar(25)");

                entity.Property(e => e.Deleted)
                    .HasColumnName("deleted")
                    .HasColumnType("boolean");
            });
        }
    }
}
