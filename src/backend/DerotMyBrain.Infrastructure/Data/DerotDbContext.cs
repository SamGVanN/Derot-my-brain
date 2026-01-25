using Microsoft.EntityFrameworkCore;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Infrastructure.Data;

public class DerotDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserPreferences> UserPreferences { get; set; }
    public DbSet<UserActivity> Activities { get; set; }
    public DbSet<UserFocus> UserFocuses { get; set; }
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
        
        // UserActivity configuration
        modelBuilder.Entity<UserActivity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.SourceId).IsRequired();
            entity.Property(e => e.SourceType).IsRequired();
            entity.Property(e => e.SourceHash).IsRequired();
            
            // Map SessionDateEnd as optional
            entity.Property(e => e.SessionDateEnd).IsRequired(false);
            
            // Indexes
            entity.HasIndex(e => new { e.UserId, e.SessionDateStart }); // For history sorting by start
            entity.HasIndex(e => new { e.UserId, e.SessionDateEnd }); // For history sorting by end
            entity.HasIndex(e => new { e.UserId, e.SourceHash });
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.Activities)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserFocus configuration
        modelBuilder.Entity<UserFocus>(entity =>
        {
            entity.ToTable("UserFocuses"); // renaming the table too
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.SourceId).IsRequired();
            entity.Property(e => e.SourceType).IsRequired();
            entity.Property(e => e.SourceHash).IsRequired();
            
            entity.HasIndex(e => new { e.UserId, e.SourceHash }).IsUnique();
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserFocuses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Document configuration
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.FileName).IsRequired();
            entity.Property(e => e.StoragePath).IsRequired();
            entity.Property(e => e.FileType).IsRequired();
            entity.Property(e => e.SourceHash).IsRequired();

            entity.HasIndex(e => new { e.UserId, e.SourceHash }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany(u => u.Documents)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // BacklogItem configuration
        modelBuilder.Entity<BacklogItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.SourceId).IsRequired();
            entity.Property(e => e.SourceType).IsRequired();
            entity.Property(e => e.SourceHash).IsRequired();

            entity.HasIndex(e => new { e.UserId, e.SourceHash }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany(u => u.BacklogItems)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
