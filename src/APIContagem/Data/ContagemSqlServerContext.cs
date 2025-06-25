using Microsoft.EntityFrameworkCore;

namespace APIContagem.Data;

public class ContagemSqlServerContext : DbContext
{
    public DbSet<HistoricoContagemSecundario>? Historicos { get; set; }

    public ContagemSqlServerContext(DbContextOptions<ContagemSqlServerContext> options) :
        base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HistoricoContagemSecundario>(entity =>
        {
            entity.ToTable("HistoricoContagem");
            entity.HasKey(c => c.Id);
        });
    }
}