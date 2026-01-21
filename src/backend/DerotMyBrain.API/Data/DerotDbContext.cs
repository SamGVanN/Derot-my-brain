using Microsoft.EntityFrameworkCore;
using DerotMyBrain.API.Models;

namespace DerotMyBrain.API.Data;

/// <summary>
/// Database context for the Derot My Brain application.
/// </summary>
public class DerotDbContext : DbContext
{
    /// <summary>
    /// Users table.
    /// </summary>
    public DbSet<User> Users { get; set; }
    
    /// <summary>
    /// User preferences table.
    /// </summary>
    public DbSet<UserPreferences> UserPreferences { get; set; }
    
    /// <summary>
    /// User activities table.
    /// </summary>
    public DbSet<UserActivity> Activities { get; set; }

    /// <summary>
    /// Tracked topics table.
    /// </summary>
    public DbSet<TrackedTopic> TrackedTopics { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the DerotDbContext class.
    /// </summary>
    /// <param name="options">Database context options.</param>
    public DerotDbContext(DbContextOptions<DerotDbContext> options) : base(options)
    {
    }
    
    /// <summary>
    /// Configures the database schema using Fluent API.
    /// </summary>
    /// <param name="modelBuilder">Model builder instance.</param>
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
            entity.Property(e => e.QuestionCount).HasDefaultValue(10);
            entity.Property(e => e.PreferredTheme).HasDefaultValue("derot-brain");
            entity.Property(e => e.Language).HasDefaultValue("auto");
            
            // JSON column for SelectedCategories
            entity.Property(e => e.SelectedCategories)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
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
            
            // Required fields
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Topic).IsRequired();
            entity.Property(e => e.WikipediaUrl).IsRequired();
            entity.Property(e => e.Type).IsRequired().HasDefaultValue("Read");
            entity.Property(e => e.SessionDate).IsRequired();
            
            // Index for user's activity history (most common query)
            entity.HasIndex(e => new { e.UserId, e.SessionDate })
                .HasDatabaseName("IX_UserActivity_UserId_SessionDate");
            
            // Index for topic-specific history (evolution queries)
            entity.HasIndex(e => new { e.UserId, e.Topic, e.SessionDate })
                .HasDatabaseName("IX_UserActivity_UserId_Topic_SessionDate");
            
            // Index for type filtering
            entity.HasIndex(e => new { e.UserId, e.Type, e.SessionDate })
                .HasDatabaseName("IX_UserActivity_UserId_Type_SessionDate");
            
            // Foreign key
            entity.HasOne(e => e.User)
                .WithMany(u => u.Activities)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TrackedTopic configuration
        modelBuilder.Entity<TrackedTopic>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Required fields
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Topic).IsRequired();
            entity.Property(e => e.TrackedDate).IsRequired();
            
            // Unique constraint: One tracked topic per user
            entity.HasIndex(e => new { e.UserId, e.Topic })
                .IsUnique()
                .HasDatabaseName("IX_TrackedTopic_UserId_Topic_Unique");
            
            // Index for tracked topics list sorted by last attempt
            entity.HasIndex(e => new { e.UserId, e.LastAttemptDate })
                .HasDatabaseName("IX_TrackedTopic_UserId_LastAttemptDate");
            
            // Index for tracked topics list sorted by best score
            entity.HasIndex(e => new { e.UserId, e.BestScore })
                .HasDatabaseName("IX_TrackedTopic_UserId_BestScore");
            
            // Foreign key
            entity.HasOne(e => e.User)
                .WithMany(u => u.TrackedTopics)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

}
