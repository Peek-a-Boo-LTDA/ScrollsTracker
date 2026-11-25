using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ScrollsTracker.Domain.Enum;
using ScrollsTracker.Domain.Models;
using System.Text.Json;

namespace ScrollsTracker.Infra.Repository.Context
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<Obra> Obras { get; set; }
        public DbSet<UsuarioObra> UsuarioObras { get; set; }
        public DbSet<LancamentoCapitulo> Lancamentos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var obra = modelBuilder.Entity<Obra>();
            obra.HasIndex(i => i.Titulo)
                .IsUnique();
            obra.Property(i => i.Titulo).IsConcurrencyToken();
            obra.Property(i => i.TotalCapitulos).IsConcurrencyToken();
            obra.HasIndex(i => i.DataVerificacao);
            obra.Property(i => i.UltimoCapituloDetectadoEm)
                .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);
            obra.PrimitiveCollection(i => i.TitulosAlternativos).HasDefaultValueSql("'[]'");
            obra.Property(i => i.VinculosExternos)
                .HasConversion(
                    vinculos => JsonSerializer.Serialize(vinculos, (JsonSerializerOptions?)null),
                    json => JsonSerializer.Deserialize<Dictionary<EnumSources, string>>(json, (JsonSerializerOptions?)null)!,
                    new ValueComparer<Dictionary<EnumSources, string>>(
                        (a, b) => a != null && b != null && a.OrderBy(v => v.Key).SequenceEqual(b.OrderBy(v => v.Key)),
                        vinculos => vinculos.OrderBy(v => v.Key).Aggregate(0, (hash, v) => HashCode.Combine(hash, v)),
                        vinculos => new Dictionary<EnumSources, string>(vinculos)))
                .HasDefaultValueSql("'{}'")
                .IsConcurrencyToken();

            // Dados da biblioteca anterior à criação de contas, transferidos na configuração do administrador.
            obra.Property<string>("UltimoCapituloLido");
            obra.Property<string>("StatusLeitura");

            var leitura = modelBuilder.Entity<UsuarioObra>();
            leitura.HasKey(i => new { i.UsuarioId, i.ObraId });
            leitura.HasOne<IdentityUser>().WithMany().HasForeignKey(i => i.UsuarioId);
            leitura.HasOne<Obra>().WithMany().HasForeignKey(i => i.ObraId);

            var lancamento = modelBuilder.Entity<LancamentoCapitulo>();
            lancamento.HasKey(i => new { i.ObraId, i.Numero });
            lancamento.HasIndex(i => i.DetectadoEm);
            lancamento.Property(i => i.Numero).HasMaxLength(40);
            lancamento.Property(i => i.DetectadoEm)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            lancamento.HasOne(i => i.Obra).WithMany().HasForeignKey(i => i.ObraId);
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
