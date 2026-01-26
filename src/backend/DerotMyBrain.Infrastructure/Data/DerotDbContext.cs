using Microsoft.EntityFrameworkCore;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Infrastructure.Data;

public class DerotDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserPreferences> UserPreferences { get; set; }
    public DbSet<UserActivity> Activities { get; set; }
    public DbSet<Source> Sources { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<UserSession> Sessions { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<BacklogItem> BacklogItems { get; set; }
    
    public DerotDbContext(DbContextOptions<DerotDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.LastConnectionAt).IsRequired();
        });
        
        // UserPreferences configuration
        modelBuilder.Entity<UserPreferences>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.QuestionsPerQuiz).HasDefaultValue(5);
            entity.Property(e => e.Theme).HasDefaultValue("system");
            entity.Property(e => e.Language).HasDefaultValue("en");
            
            entity.Property(e => e.FavoriteCategories)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<WikipediaCategory>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<WikipediaCategory>(),
                    new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<WikipediaCategory>>(
                        (c1, c2) => c1!.SequenceEqual(c2!),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));
            
            entity.HasOne(e => e.User)
                .WithOne(u => u.Preferences)
                .HasForeignKey<UserPreferences>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Source configuration
        modelBuilder.Entity<Source>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.ExternalId).IsRequired();
            entity.Property(e => e.DisplayTitle).IsRequired();
            entity.Property(e => e.IsTracked).HasDefaultValue(false);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Topic)
                .WithMany(t => t.Sources)
                .HasForeignKey(e => e.TopicId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Topic configuration
        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Title).IsRequired();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserSession configuration
        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.StartedAt).IsRequired();
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TargetSource)
                .WithMany(s => s.Sessions)
                .HasForeignKey(e => e.TargetSourceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TargetTopic)
                .WithMany()
                .HasForeignKey(e => e.TargetTopicId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // UserActivity configuration
        modelBuilder.Entity<UserActivity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.UserSessionId).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            
            entity.Property(e => e.Title).IsRequired();
            
            // Map SessionDateEnd as optional
            entity.Property(e => e.SessionDateEnd).IsRequired(false);
            
            // Indexes
            entity.HasIndex(e => new { e.UserId, e.SessionDateStart });
            entity.HasIndex(e => new { e.UserId, e.SessionDateEnd });
            entity.HasIndex(e => e.UserSessionId);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.Activities)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UserSession)
                .WithMany(s => s.Activities)
                .HasForeignKey(e => e.UserSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Source)
                .WithMany(s => s.Activities)
                .HasForeignKey(e => e.SourceId)
                .OnDelete(DeleteBehavior.SetNull); // Keep activity if source is somehow missing
        });



        // Document configuration
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.FileName).IsRequired();
            entity.Property(e => e.StoragePath).IsRequired();
            entity.Property(e => e.FileType).IsRequired();
            entity.Property(e => e.SourceId).IsRequired();

            entity.HasIndex(e => new { e.UserId, e.SourceId }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany(u => u.Documents)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Source)
                .WithMany(s => s.Documents)
                .HasForeignKey(e => e.SourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // BacklogItem configuration
        modelBuilder.Entity<BacklogItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.SourceId).IsRequired();

            entity.HasIndex(e => new { e.UserId, e.SourceId }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany(u => u.BacklogItems)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Source)
                .WithMany(s => s.BacklogItems)
                .HasForeignKey(e => e.SourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
