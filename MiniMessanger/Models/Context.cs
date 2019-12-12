using Common;
using Microsoft.EntityFrameworkCore;

namespace miniMessanger.Models
{
    public partial class Context : DbContext
    {
        public bool manualControl = false;
        public bool useInMemoryDatabase = false;
        public Context()
        {
        }
        public Context(bool manualControl)
        {
            this.manualControl = manualControl;
        }
        public Context(bool manualControl, bool useInMemoryDatabase)
        {
            this.manualControl = manualControl;
            this.useInMemoryDatabase = useInMemoryDatabase;
        }
        public Context(DbContextOptions<Context> options)
            : base(options)
        {
        }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<BlockedUser> BlockedUsers { get; set; }
        public virtual DbSet<Chatroom> Chatroom { get; set; }
        public virtual DbSet<Complaint> Complaint { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<Participants> Participants { get; set; }
        public virtual DbSet<Profile> Profile { get; set; }
        public virtual DbSet<LikeProfiles> LikeProfile { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (useInMemoryDatabase)
            {
                optionsBuilder.UseInMemoryDatabase("messanger");
            }
            optionsBuilder.EnableSensitiveDataLogging();
            if (manualControl)
            {
                if (!optionsBuilder.IsConfigured)
                {
                    Config config = new Config();
                    optionsBuilder.UseMySql(config.GetDatabaseConfigConnection());
                }
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(user => user.UserId)
                    .HasName("PRIMARY");

                entity.ToTable("users");

                entity.HasIndex(user => user.UserEmail)
                    .HasName("user_email")
                    .IsUnique();

                entity.Property(user => user.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("int(11)");

                entity.Property(user => user.Activate)
                    .HasColumnName("activate")
                    .HasColumnType("tinyint(4)")
                    .HasDefaultValueSql("'0'");

                entity.Property(user => user.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("int(11)");

                entity.Property(user => user.LastLoginAt)
                    .HasColumnName("last_login_at")
                    .HasColumnType("int(11)");

                entity.Property(user => user.RecoveryCode)
                    .HasColumnName("recovery_code")
                    .HasColumnType("int(11)");

                entity.Property(user => user.RecoveryToken)
                    .HasColumnName("recovery_token")
                    .HasColumnType("varchar(50)");

                entity.Property(user => user.UserEmail)
                    .HasColumnName("user_email")
                    .HasColumnType("varchar(100)");

                entity.Property(user => user.UserHash)
                    .HasColumnName("user_hash")
                    .HasColumnType("varchar(120)");

                entity.Property(user => user.UserLogin)
                    .HasColumnName("user_login")
                    .HasColumnType("varchar(256)");

                entity.Property(user => user.UserPassword)
                    .HasColumnName("user_password")
                    .HasColumnType("varchar(256)");

                entity.Property(user => user.UserPublicToken)
                    .HasColumnName("user_public_token")
                    .HasColumnType("varchar(20)");

                entity.Property(user => user.UserToken)
                    .HasColumnName("user_token")
                    .HasColumnType("varchar(50)");

                entity.Property(user => user.ProfileToken)
                    .HasColumnName("profile_token")
                    .HasColumnType("varchar(50)");

                entity.Property(user => user.Deleted)
                    .HasColumnName("deleted")
                    .HasColumnType("boolean");
            });
            modelBuilder.Entity<BlockedUser>(entity =>
            {
                entity.HasKey(block => block.BlockedId)
                    .HasName("PRIMARY");

                entity.ToTable("blocked_users");

                entity.HasIndex(block => block.BlockedUserId)
                    .HasName("blocked_user_id");

                entity.HasIndex(block => block.UserId)
                    .HasName("user_id");
                
                entity.Property(block => block.BlockedId)
                    .HasColumnName("blocked_id")
                    .HasColumnType("int(11)");
                
                entity.Property(block => block.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("int(11)");
                
                entity.Property(block => block.BlockedUserId)
                    .HasColumnName("blocked_user_id")
                    .HasColumnType("int(11)");
                
                entity.Property(block => block.BlockedDeleted)
                    .HasColumnName("blocked_deleted")
                    .HasColumnType("boolean");

                entity.Property(block => block.BlockedReason)
                    .HasColumnName("blocked_reason")
                    .HasColumnType("varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci");

                entity.HasOne(block => block.Blocked)
                    .WithMany(user => user.BlockedUsers)
                    .HasForeignKey(block => block.BlockedUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("blocked_users_ibfk_2");

                entity.HasOne(block => block.User)
                    .WithMany(user => user.UsersBlocks)
                    .HasForeignKey(block => block.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("blocked_users_ibfk_1");
            });

            modelBuilder.Entity<LikeProfiles>(entity =>
            {
                entity.HasKey(e => e.LikeId)
                    .HasName("PRIMARY");

                entity.ToTable("like_profiles");

                entity.HasIndex(e => e.UserId)
                    .HasName("user_id");

                entity.HasIndex(e => e.ToUserId)
                    .HasName("to_user_id");

                entity.Property(e => e.LikeId)
                    .HasColumnName("like_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ToUserId)
                    .HasColumnName("to_user_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Like)
                    .HasColumnName("like")
                    .HasColumnType("boolean");

                entity.Property(e => e.Dislike)
                    .HasColumnName("dislike")
                    .HasColumnType("boolean");

                
                /*entity.HasOne(like => like.User)
                    .WithMany(user => user.)
                    .HasForeignKey(d => d.BlockedUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("blocked_users_ibfk_2");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UsersBlocks)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("blocked_users_ibfk_1");*/
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
                    .HasColumnType("timestamp");
                    //.HasDefaultValue("2000-01-01 10:00:00")
                    //.HasDefaultValueSql("'CURRENT_TIMESTAMP'");
                    //.ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<Complaint>(entity =>
            {
                entity.HasKey(e => e.ComplaintId)
                    .HasName("PRIMARY");

                entity.ToTable("complaints");

                entity.HasIndex(e => e.BlockId)
                    .HasName("blocked_id");

                entity.HasIndex(e => e.MessageId)
                    .HasName("message_id");

                entity.HasIndex(e => e.UserId)
                    .HasName("user_id");

                entity.Property(e => e.ComplaintId)
                    .HasColumnName("complaint_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BlockId)
                    .HasColumnName("blocked_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ComplaintText)
                    .HasColumnName("complaint")
                    .HasColumnType("varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci");

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
                    .WithOne(p => p.Complaints)
                    .HasForeignKey<Complaint>(d => d.BlockId)
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
            modelBuilder.Entity<Message>(entity =>
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
                    .HasColumnType("timestamp");
                    //.HasDefaultValue("2000-01-01 10:00:00")
                    //.HasDefaultValueSql("'CURRENT_TIMESTAMP'");
                    //.ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.MessageType)
                    .HasColumnName("message_type")
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.MessageText)
                    .HasColumnName("message_text")
                    .HasColumnType("varchar(500) CHARACTER SET utf8 COLLATE utf8_general_ci")
                    .IsUnicode(true);

                entity.Property(e => e.UrlFile)
                    .HasColumnName("url_file")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.MessageViewed)
                    .HasColumnName("message_viewed")
                    .HasColumnType("boolean");

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

            modelBuilder.Entity<Profile>(entity =>
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

                entity.Property(e => e.ProfileGender)
                    .HasColumnName("profile_gender")
                    .HasDefaultValue(true)
                    .HasColumnType("boolean");

                entity.Property(e => e.UrlPhoto)
                    .HasColumnName("url_photo")
                    .HasColumnType("varchar(256)");

                entity.Property(e => e.ProfileCity)
                    .HasColumnName("profile_city")
                    .HasColumnType("varchar(256)");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.User)
                    .WithOne(p => p.Profile)
                    .HasForeignKey<Profile>(e => e.UserId)
                    .IsRequired()
                    .HasConstraintName("profiles_ibfk_1");
            });
        }
    }
}
