using Microsoft.EntityFrameworkCore;
using DerotMyBrain.Core.Entities;

namespace DerotMyBrain.Infrastructure.Data;

public class DerotDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserPreferences> UserPreferences { get; set; }
    public DbSet<UserActivity> Activities { get; set; }
    public DbSet<TrackedTopic> TrackedTopics { get; set; }
    
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
            
            // JSON column for FavoriteCategories (List<WikipediaCategory>)
             entity.Property(e => e.FavoriteCategories)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<WikipediaCategory>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<WikipediaCategory>()
                );
            
            // Foreign key relationship (1-to-1)
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
            entity.Property(e => e.LastAttemptDate).IsRequired();
            
            // Required fields
            entity.Property(e => e.Title).IsRequired();
            // SourceUrl is optional in entity (string?), but if we want it required for now:
            // entity.Property(e => e.SourceUrl).IsRequired(); 
            // In Entity it is nullable string? SourceUrl. So IsRequired() might be wrong if we allow file uploads without URL.
            // Let's matching Entity: nullable.
            
            entity.HasIndex(e => new { e.UserId, e.LastAttemptDate });
            entity.HasIndex(e => new { e.UserId, e.Title, e.LastAttemptDate });
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.Activities)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TrackedTopic configuration
        modelBuilder.Entity<TrackedTopic>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Topic).IsRequired();
            
            entity.HasIndex(e => new { e.UserId, e.Topic }).IsUnique();
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.TrackedTopics)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
