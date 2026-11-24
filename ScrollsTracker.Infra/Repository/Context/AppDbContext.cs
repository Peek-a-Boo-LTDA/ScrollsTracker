using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ScrollsTracker.Domain.Models;

namespace ScrollsTracker.Infra.Repository.Context
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<Obra> Obras { get; set; }
        public DbSet<UsuarioObra> UsuarioObras { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var obra = modelBuilder.Entity<Obra>();
            obra.HasIndex(i => i.Titulo)
                .IsUnique();

            // Dados da biblioteca anterior à criação de contas, transferidos na configuração do administrador.
            obra.Property<string>("UltimoCapituloLido");
            obra.Property<string>("StatusLeitura");

            var leitura = modelBuilder.Entity<UsuarioObra>();
            leitura.HasKey(i => new { i.UsuarioId, i.ObraId });
            leitura.HasOne<IdentityUser>().WithMany().HasForeignKey(i => i.UsuarioId);
            leitura.HasOne<Obra>().WithMany().HasForeignKey(i => i.ObraId);
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
