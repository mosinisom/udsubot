using Microsoft.EntityFrameworkCore;

public class DataContext : DbContext
{
    protected readonly IConfiguration Configuration;

    public DataContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(Configuration.GetConnectionString("ConnStr"));
    }

    public DbSet<Institutes> Institutes { get; set; }
    public DbSet<Likes> Likes { get; set; }
    public DbSet<Messages> Messages { get; set; }
    public DbSet<Photos> Photos { get; set; }
    public DbSet<Students> Students { get; set; }
    public DbSet<Users> Users { get; set; }


}