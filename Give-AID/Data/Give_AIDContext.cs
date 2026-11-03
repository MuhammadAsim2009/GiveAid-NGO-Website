using Microsoft.EntityFrameworkCore;

public class Give_AIDContext(DbContextOptions<Give_AIDContext> options) : DbContext(options)
{
    public DbSet<Give_AID.Models.Users> Users { get; set; } = default!;
}
