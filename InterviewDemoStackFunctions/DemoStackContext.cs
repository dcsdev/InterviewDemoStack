using Microsoft.EntityFrameworkCore;
using Domain;

public class DemoStackContext : DbContext
{
    public DemoStackContext(DbContextOptions<DemoStackContext> options)
        : base(options)
    {
    }

    public DbSet<GraphEmailMessage> Messages { get; set; }
    public DbSet<EventGrid> EventGridLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseCosmos(
            "https://azuredemostack.documents.azure.com:443/",
            "fWrRLucU03hHLVdK3RAvmmmdLnCB1pzxqb6Dyv8SgyIX0GqvkfkTA3o5WYjtRX14Oa3R1XjbUCz6ACDbMvpAyQ= =",
            "demo"
        );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GraphEmailMessage>()
            .ToContainer("Messages");

        modelBuilder.Entity<GraphEmailMessage>()
            .HasPartitionKey(u => u.Id);

        modelBuilder.Entity<EventGrid>()
            .ToContainer("EventGridLogs");

        modelBuilder.Entity<EventGrid>()
            .HasPartitionKey(u => u.Id);
    }
}
