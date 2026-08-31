using System.Data;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Empodera.Models;

namespace Empodera.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            PrepareIdentityFields();
            PrepareRandomPrimaryKeys();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            PrepareIdentityFields();
            await PrepareRandomPrimaryKeysAsync(cancellationToken);
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Perfil> Perfis { get; set; } = null!;
        public DbSet<Permissoes> Permissoes { get; set; } = null!;
        public DbSet<Comunidade> Comunidades { get; set; } = null!;
        public DbSet<Atores> Atores { get; set; } = null!;
        public DbSet<RedeRecursos> RedeRecursos { get; set; } = null!;
        public DbSet<Eixo> Eixos { get; set; } = null!;
        public DbSet<RedeEixo> RedeEixos { get; set; } = null!;
        public DbSet<AtorComunidade> AtorComunidades { get; set; } = null!;
        public DbSet<DiarioCampo> DiariosCampo { get; set; } = null!;
        public DbSet<DiarioAcoes> DiarioAcoes { get; set; } = null!;
        public DbSet<DiarioDAcoes> DiarioDAcoes { get; set; } = null!;
        public DbSet<DetalhesDAcoes> DetalhesDAcoes { get; set; } = null!;
        public DbSet<DetalhesEixos> DetalhesEixos { get; set; } = null!;
        public DbSet<DAAtores> DAAtores { get; set; } = null!;
        public DbSet<DiarioEixo> DiarioEixos { get; set; } = null!;
        public DbSet<Acoes> Acoes { get; set; } = null!;
        public DbSet<AcoesAtores> AcoesAtores { get; set; } = null!;
        public DbSet<AnexosDiario> AnexosDiario { get; set; } = null!;
        public DbSet<Vulnerabilidade> Vulnerabilidades { get; set; } = null!;
        public DbSet<VulnerabilidadesEixo> VulnerabilidadesEixo { get; set; } = null!;
        public DbSet<RedePrimaria> RedesPrimarias { get; set; } = null!;
        public DbSet<AvaliacaoPessoal> AvaliacaoPessoal { get; set; } = null!;
        public DbSet<FichaPrimeiroContato> FichasPrimeiroContato { get; set; } = null!;
        public DbSet<FonteInf> FontesInfo { get; set; } = null!;
        public DbSet<FichaCondicoes> FichaCondicoes { get; set; } = null!;
        public DbSet<FichaPeticoes> FichaPeticoes { get; set; } = null!;
        public DbSet<FichaResp> FichaRespostas { get; set; } = null!;
        public DbSet<FichaResult> FichaResultados { get; set; } = null!;
        public DbSet<Atividades> Atividades { get; set; } = null!;
        public DbSet<AtividadesEixo> AtividadesEixo { get; set; } = null!;
        public DbSet<RecursosAtores> RecursosAtores { get; set; } = null!;
        public DbSet<DiarioProcessoPessoal> DiariosProcessoPessoal { get; set; } = null!;
        public DbSet<DiarioProcessoEixo> DiariosProcessoEixos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>().HasKey(u => u.IdUsuario);
            modelBuilder.Entity<Usuario>().Property(u => u.ConcurrencyStamp).IsConcurrencyToken();
            modelBuilder.Entity<Usuario>().HasIndex(u => u.NormalizedUserName).IsUnique();
            modelBuilder.Entity<Usuario>().HasIndex(u => u.NormalizedEmail).IsUnique();
            modelBuilder.Entity<Perfil>().HasKey(p => p.IdPerfil);
            modelBuilder.Entity<Permissoes>().HasKey(p => p.IdPermissoes);
            modelBuilder.Entity<Comunidade>().HasKey(c => c.Id_Comunidade);
            modelBuilder.Entity<Atores>().HasKey(a => a.IdAtores);
            modelBuilder.Entity<RedeRecursos>().HasKey(r => r.Id_Rede);
            modelBuilder.Entity<Eixo>().HasKey(e => e.IdEixo);
            modelBuilder.Entity<RedeEixo>().HasKey(re => re.IdRedeEixo);
            modelBuilder.Entity<AtorComunidade>().HasKey(ac => ac.IdAtorComunidade);
            modelBuilder.Entity<DiarioCampo>().HasKey(dc => dc.IdDCampo);
            modelBuilder.Entity<DiarioAcoes>().HasKey(da => da.IdDAcoes);
            modelBuilder.Entity<DiarioDAcoes>().HasKey(dd => dd.IdDAcoes);
            modelBuilder.Entity<DetalhesDAcoes>().HasKey(dd => dd.Id);
            modelBuilder.Entity<DetalhesEixos>().HasKey(de => de.IdDiarioEixo);
            modelBuilder.Entity<DAAtores>().HasKey(d => d.Id);
            modelBuilder.Entity<DiarioEixo>().HasKey(de => de.IdDiarioEixo);
            modelBuilder.Entity<Acoes>().HasKey(a => a.IdAcoes);
            modelBuilder.Entity<AcoesAtores>().HasKey(aa => aa.IdAAtores);
            modelBuilder.Entity<AnexosDiario>().HasKey(ad => ad.IdAnexos);
            modelBuilder.Entity<Vulnerabilidade>().HasKey(v => v.IdVulnerabilidade);
            modelBuilder.Entity<VulnerabilidadesEixo>().HasKey(ve => ve.IdVEixo);
            modelBuilder.Entity<RedePrimaria>().HasKey(rp => rp.IdRedePrimaria);
            modelBuilder.Entity<AvaliacaoPessoal>().HasKey(ap => ap.IdAvaliacao);
            modelBuilder.Entity<FichaPrimeiroContato>().HasKey(fp => fp.IdFicha);
            modelBuilder.Entity<FonteInf>().HasKey(fi => fi.IdFonte);
            modelBuilder.Entity<FichaCondicoes>().HasKey(fc => fc.IdCondicoes);
            modelBuilder.Entity<FichaPeticoes>().HasKey(fp => fp.IdPeticoes);
            modelBuilder.Entity<FichaResp>().HasKey(fr => fr.IdCondicoes);
            modelBuilder.Entity<FichaResult>().HasKey(fr => fr.IdCondicoes);
            modelBuilder.Entity<Atividades>().HasKey(at => at.IdAtividade);
            modelBuilder.Entity<AtividadesEixo>().HasKey(ae => ae.IdAEixo);
            modelBuilder.Entity<RecursosAtores>().HasKey(ra => ra.Id_Recursos_Atores);
            modelBuilder.Entity<DiarioProcessoPessoal>().HasKey(d => d.IdDiarioProcesso);
            modelBuilder.Entity<DiarioProcessoEixo>().HasKey(d => d.IdDiarioProcessoEixo);

            modelBuilder.Entity<DiarioProcessoPessoal>().ToTable("diariosprocessopessoal");
            modelBuilder.Entity<DiarioProcessoEixo>().ToTable("diariosprocessoeixos");
            modelBuilder.Entity<DiarioProcessoEixo>()
                .HasIndex(d => new { d.FkIdDiarioProcesso, d.FkIdEixo })
                .IsUnique();

            // Perfil -> Usuario (many Perfis belong to one Usuario)
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Perfil)
                .WithMany(p => p.Usuarios)
                .HasForeignKey(u => u.FkIdPerfil)
                .OnDelete(DeleteBehavior.Restrict);


            // Permissoes -> Perfil (many Permissoes belong to one Perfil)
            modelBuilder.Entity<Permissoes>()
                .HasOne(per => per.Perfil)
                .WithMany(p => p.Permissoes)
                .HasForeignKey(per => per.FkIdPerfil)
                .OnDelete(DeleteBehavior.Cascade);

            // Comunidade -> Usuario (many Comunidades belong to one Usuario)
            modelBuilder.Entity<Comunidade>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.Comunidades)
                .HasForeignKey(c => c.FK_Id_Usuario)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comunidade>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(c => c.FK_Id_UsuarioM)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Atores>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.Atores)
                .HasForeignKey(c => c.FkIdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Atores>()
                 .HasOne<Usuario>()
                 .WithMany()
                 .HasForeignKey(c => c.FkIdUsuarioM)
                 .OnDelete(DeleteBehavior.Restrict);

            // RedeRecursos -> Atores and Comunidade and Usuario
            modelBuilder.Entity<RedeRecursos>()
                .HasOne(r => r.Ator)
                .WithMany(a => a.Redes)
                .HasForeignKey(r => r.FK_id_Atores)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RedeRecursos>()
                .HasOne(r => r.Comunidade)
                .WithMany(c => c.RedeRecursos)
                .HasForeignKey(r => r.FkIdComunidade)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RedeRecursos>()
                .HasOne(r => r.Usuario)
                .WithMany(u => u.RedeRecursos)
                .HasForeignKey(r => r.FkIdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            // RedeEixo -> RedeRecursos <-> Eixo
            modelBuilder.Entity<RedeEixo>()
                .HasOne(re => re.RedeRecursos)
                .WithMany(r => r.RedeEixos)
                .HasForeignKey(re => re.FkIdRede)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RedeEixo>()
                .HasOne(re => re.Eixo)
                .WithMany(e => e.RedeEixos)
                .HasForeignKey(re => re.FkIdEixo)
                .OnDelete(DeleteBehavior.Cascade);

            // AtorComunidade -> Comunidade <-> Atores
            modelBuilder.Entity<AtorComunidade>()
                .HasOne(ac => ac.Comunidade)
                .WithMany(c => c.AtorComunidades)
                .HasForeignKey(ac => ac.FkIdComunidade)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AtorComunidade>()
                .HasOne(ac => ac.Ator)
                .WithMany(a => a.Comunidades)
                .HasForeignKey(ac => ac.FK_id_Atores)
                .OnDelete(DeleteBehavior.Cascade);

            // DiarioCampo -> Comunidade & Usuario
            modelBuilder.Entity<DiarioCampo>()
                .HasOne(d => d.Comunidade)
                .WithMany(c => c.DiarioCampos)
                .HasForeignKey(d => d.FkIdComunidade)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiarioCampo>()
                .HasOne(d => d.Usuario)
                .WithMany(u => u.DiarioCampos)
                .HasForeignKey(d => d.FkIdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiarioAcoes>()
                .HasOne(da => da.Acoes)
                .WithMany(a => a.DiarioAcoes)
                .HasForeignKey(da => da.FkIdAcoes)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiarioAcoes>()
                .HasOne(da => da.Diario)
                .WithMany(d => d.DiarioAcoes)
                .HasForeignKey(da => da.FkIdDiario)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiarioDAcoes>()
                .HasOne(dd => dd.Diario)
                .WithMany(d => d.DiarioDAcoes)
                .HasForeignKey(dd => dd.FkIdDiario)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalhesDAcoes>()
                .HasOne(det => det.DiarioDAcoes)
                .WithMany(dd => dd.Detalhes)
                .HasForeignKey(det => det.FkIdDDacoes)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalhesEixos>()
                .HasOne(de => de.Detalhes)
                .WithMany(d => d.DetalhesEixos)
                .HasForeignKey(de => de.FkIdDetalhes)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalhesEixos>()
                .HasOne(de => de.Eixo)
                .WithMany(e => e.DetalhesEixos)
                .HasForeignKey(de => de.FkIdEixo)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DAAtores>()
                .HasOne(d => d.DiarioDAcoes)
                .WithMany(dd => dd.DAtores)
                .HasForeignKey(d => d.FkIdDDacoes)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DAAtores>()
                .HasOne(d => d.Ator)
                .WithMany(a => a.DAAtores)
                .HasForeignKey(d => d.FK_id_Atores)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiarioEixo>()
                .HasOne(de => de.Diario)
                .WithMany(d => d.DiarioEixos)
                .HasForeignKey(de => de.FkIdDiario)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiarioEixo>()
                .HasOne(de => de.Eixo)
                .WithMany(e => e.DiarioEixos)
                .HasForeignKey(de => de.FkIdEixo)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Acoes>()
                .HasOne(a => a.Atividades)
                .WithMany(at => at.Acoes)
                .HasForeignKey(a => a.FkIdAtividade)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AcoesAtores>()
                .HasOne(aa => aa.Acoes)
                .WithMany(a => a.AcoesAtores)
                .HasForeignKey(aa => aa.FkIdAcoes)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AcoesAtores>()
                .HasOne(aa => aa.Ator)
                .WithMany(a => a.AcoesAtores)
                .HasForeignKey(aa => aa.FK_id_Atores)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AnexosDiario>()
                .HasOne(ad => ad.Diario)
                .WithMany(d => d.Anexos)
                .HasForeignKey(ad => ad.FkIdDiario)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Vulnerabilidade>()
                .HasOne(v => v.Comunidade)
                .WithMany(c => c.Vulnerabilidades)
                .HasForeignKey(v => v.FkIdComunidade)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VulnerabilidadesEixo>()
                .HasOne(ve => ve.Eixo)
                .WithMany(e => e.VulnerabilidadesEixos)
                .HasForeignKey(ve => ve.FkIdEixo)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VulnerabilidadesEixo>()
                .HasOne(ve => ve.Vulnerabilidade)
                .WithMany(v => v.VulnerabilidadesEixos)
                .HasForeignKey(ve => ve.FkIdVulnerabilidade)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RedePrimaria>()
                .HasOne(rp => rp.AtorPrincipal)
                .WithMany()
                .HasForeignKey(rp => rp.FkIdAtorPrincipal)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RedePrimaria>()
                .HasOne(rp => rp.AtorRelacionado)
                .WithMany()
                .HasForeignKey(rp => rp.FkIdAtorRelacionados)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AvaliacaoPessoal>()
                .HasOne(ap => ap.Ator)
                .WithMany(a => a.Avaliacoes)
                .HasForeignKey(ap => ap.FK_id_Atores)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AvaliacaoPessoal>()
                .HasOne(ap => ap.Usuario)
                .WithMany(u => u.Avaliacoes)
                .HasForeignKey(ap => ap.FkIdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiarioProcessoPessoal>()
                .HasOne(d => d.Ator)
                .WithMany(a => a.DiariosProcessoPessoal)
                .HasForeignKey(d => d.FK_id_Atores)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiarioProcessoPessoal>()
                .HasOne(d => d.Usuario)
                .WithMany(u => u.DiariosProcessoPessoal)
                .HasForeignKey(d => d.FkIdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DiarioProcessoPessoal>()
                .HasOne(d => d.UsuarioModificacao)
                .WithMany()
                .HasForeignKey(d => d.FkIdUsuarioM)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DiarioProcessoEixo>()
                .HasOne(d => d.DiarioProcesso)
                .WithMany(d => d.Eixos)
                .HasForeignKey(d => d.FkIdDiarioProcesso)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiarioProcessoEixo>()
                .HasOne(d => d.Eixo)
                .WithMany(e => e.DiariosProcessoEixos)
                .HasForeignKey(d => d.FkIdEixo)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FichaPrimeiroContato>()
                 .HasOne(fp => fp.Comunidade)
                 .WithMany(c => c.FichasPrimeiroContato)
                 .HasForeignKey(fp => fp.FkIdComunidade)
                 .OnDelete(DeleteBehavior.Cascade);

            // FichaPrimeiroContato -> Ator
            modelBuilder.Entity<FichaPrimeiroContato>()
                .HasOne(fp => fp.Ator)
                .WithMany(a => a.FichasPrimeiroContato)
                .HasForeignKey(fp => fp.FK_id_Atores)
                .OnDelete(DeleteBehavior.Cascade);

            // FichaPrimeiroContato -> Usuario
            modelBuilder.Entity<FichaPrimeiroContato>()
                .HasOne(fp => fp.Usuario)
                .WithMany(u => u.FichasPrimeiroContato)
                .HasForeignKey(fp => fp.FkIdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FonteInf>()
                .HasOne(fi => fi.Ficha)
                .WithMany(f => f.Fontes)
                .HasForeignKey(fi => fi.FkIdFicha)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FichaCondicoes>()
                .HasOne(fc => fc.Ficha)
                .WithMany(f => f.FichaCondicoes)
                .HasForeignKey(fc => fc.FkIdFicha)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FichaPeticoes>()
                .HasOne(fp => fp.Ficha)
                .WithMany(f => f.FichaPeticoes)
                .HasForeignKey(fp => fp.FkIdFicha)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FichaResp>()
                .HasOne(fr => fr.Ficha)
                .WithMany(f => f.FichaRespostas)
                .HasForeignKey(fr => fr.FkIdFicha)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FichaResult>()
                .HasOne(fr => fr.Ficha)
                .WithMany(f => f.FichaResultados)
                .HasForeignKey(fr => fr.FkIdFicha)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Atividades>()
                .HasOne(at => at.Comunidade)
                .WithMany(c => c.Atividades)
                .HasForeignKey(at => at.FkIdComunidade)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Atividades>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.Atividades)
                .HasForeignKey(c => c.FkIdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Atividades>()
                 .HasOne<Usuario>()
                 .WithMany()
                 .HasForeignKey(c => c.FkIdUsuarioM)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AtividadesEixo>()
                .HasOne(ae => ae.Atividades)
                .WithMany(at => at.AtividadesEixos)
                .HasForeignKey(ae => ae.FkIdAtividade)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AtividadesEixo>()
                .HasOne(ae => ae.Eixo)
                .WithMany(e => e.AtividadesEixo)
                .HasForeignKey(ae => ae.FkIdEixo)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecursosAtores>()
                .HasOne(ra => ra.Atores)
                .WithMany(a => a.RecursosAtores)
                .HasForeignKey(ra => ra.FK_id_Atores)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Usuario>().HasData(
                IdentitySeed(1, "joao", "joao@email.com", "Coordenador", 1, 1, "S", "foto1.jpg", new DateTime(1990, 1, 1)),
                IdentitySeed(2, "Usuario Dois", "u2@example.com", "Pesquisador", 2, 2, "S", "foto2.jpg", new DateTime(1985, 2, 2)),
                IdentitySeed(3, "Usuario Tres", "u3@example.com", "Voluntario", 1, 3, "S", "foto3.jpg", new DateTime(1995, 3, 3)),
                IdentitySeed(4, "Usuario Quatro", "u4@example.com", "Analista", 2, 4, "N", "foto4.jpg", new DateTime(1992, 4, 4)),
                IdentitySeed(5, "Usuario Cinco", "u5@example.com", "Gerente", 1, 5, "N", "foto5.jpg", new DateTime(1988, 5, 5))
            );

            // Perfis
            modelBuilder.Entity<Perfil>().HasData(
                new Perfil { IdPerfil = 1, FkIdUsuario = 1, Nome = "Admin" },
                new Perfil { IdPerfil = 2, FkIdUsuario = 2, Nome = "Editor" },
                new Perfil { IdPerfil = 3, FkIdUsuario = 3, Nome = "Colaborador" },
                new Perfil { IdPerfil = 4, FkIdUsuario = 4, Nome = "Visualizador" },
                new Perfil { IdPerfil = 5, FkIdUsuario = 5, Nome = "Supervisor" }
            );

            // Permissoes
            modelBuilder.Entity<Permissoes>().HasData(
                // ===================== ADMIN =====================
                new Permissoes { IdPermissoes = 1, FkIdPerfil = 1, Modulo = "Usuarios", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "S" },
                new Permissoes { IdPermissoes = 2, FkIdPerfil = 1, Modulo = "Perfis", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "S" },
                new Permissoes { IdPermissoes = 3, FkIdPerfil = 1, Modulo = "Atividades", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "S" },
                new Permissoes { IdPermissoes = 4, FkIdPerfil = 1, Modulo = "Comunidades", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "S" },
                new Permissoes { IdPermissoes = 5, FkIdPerfil = 1, Modulo = "Vulnerabilidades", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "S" },
                new Permissoes { IdPermissoes = 6, FkIdPerfil = 1, Modulo = "Recursos", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "S" },
                new Permissoes { IdPermissoes = 7, FkIdPerfil = 1, Modulo = "DiariosCampo", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "S" },
                new Permissoes { IdPermissoes = 8, FkIdPerfil = 1, Modulo = "Atores", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "S" },
                new Permissoes { IdPermissoes = 9, FkIdPerfil = 1, Modulo = "Ficha1Contato", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "S" },
                new Permissoes { IdPermissoes = 10, FkIdPerfil = 1, Modulo = "DiariosProcessoPessoal", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "S" },
                new Permissoes { IdPermissoes = 11, FkIdPerfil = 1, Modulo = "AvaliacoesPessoais", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "S" },
                new Permissoes { IdPermissoes = 12, FkIdPerfil = 1, Modulo = "SER", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "S" },

                // ===================== EDITOR =====================
                new Permissoes { IdPermissoes = 13, FkIdPerfil = 2, Modulo = "Usuarios", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 14, FkIdPerfil = 2, Modulo = "Perfis", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 15, FkIdPerfil = 2, Modulo = "Atividades", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 16, FkIdPerfil = 2, Modulo = "Comunidades", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 17, FkIdPerfil = 2, Modulo = "Vulnerabilidades", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 18, FkIdPerfil = 2, Modulo = "Recursos", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 19, FkIdPerfil = 2, Modulo = "DiariosCampo", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 20, FkIdPerfil = 2, Modulo = "Atores", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 21, FkIdPerfil = 2, Modulo = "Ficha1Contato", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 22, FkIdPerfil = 2, Modulo = "DiariosProcessoPessoal", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 23, FkIdPerfil = 2, Modulo = "AvaliacoesPessoais", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 24, FkIdPerfil = 2, Modulo = "SER", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },

                // ===================== COLABORADOR =====================
                new Permissoes { IdPermissoes = 25, FkIdPerfil = 3, Modulo = "Usuarios", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 26, FkIdPerfil = 3, Modulo = "Perfis", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 27, FkIdPerfil = 3, Modulo = "Atividades", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 28, FkIdPerfil = 3, Modulo = "Comunidades", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 29, FkIdPerfil = 3, Modulo = "Vulnerabilidades", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 30, FkIdPerfil = 3, Modulo = "Recursos", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 31, FkIdPerfil = 3, Modulo = "DiariosCampo", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 32, FkIdPerfil = 3, Modulo = "Atores", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 33, FkIdPerfil = 3, Modulo = "Ficha1Contato", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 34, FkIdPerfil = 3, Modulo = "DiariosProcessoPessoal", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 35, FkIdPerfil = 3, Modulo = "AvaliacoesPessoais", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 36, FkIdPerfil = 3, Modulo = "SER", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },

                // ===================== VISUALIZADOR =====================
                new Permissoes { IdPermissoes = 37, FkIdPerfil = 4, Modulo = "Usuarios", PodeListar = "S", PodeDetalhar = "N", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 38, FkIdPerfil = 4, Modulo = "Perfis", PodeListar = "S", PodeDetalhar = "N", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 39, FkIdPerfil = 4, Modulo = "Atividades", PodeListar = "S", PodeDetalhar = "N", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 40, FkIdPerfil = 4, Modulo = "Comunidades", PodeListar = "S", PodeDetalhar = "N", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 41, FkIdPerfil = 4, Modulo = "Vulnerabilidades", PodeListar = "S", PodeDetalhar = "N", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 42, FkIdPerfil = 4, Modulo = "Recursos", PodeListar = "S", PodeDetalhar = "N", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 43, FkIdPerfil = 4, Modulo = "DiariosCampo", PodeListar = "S", PodeDetalhar = "N", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 44, FkIdPerfil = 4, Modulo = "Atores", PodeListar = "S", PodeDetalhar = "N", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 45, FkIdPerfil = 4, Modulo = "Ficha1Contato", PodeListar = "S", PodeDetalhar = "N", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 46, FkIdPerfil = 4, Modulo = "DiariosProcessoPessoal", PodeListar = "S", PodeDetalhar = "N", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 47, FkIdPerfil = 4, Modulo = "AvaliacoesPessoais", PodeListar = "S", PodeDetalhar = "N", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 48, FkIdPerfil = 4, Modulo = "SER", PodeListar = "S", PodeDetalhar = "N", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },

                // ===================== SUPERVISOR =====================
                new Permissoes { IdPermissoes = 49, FkIdPerfil = 5, Modulo = "Usuarios", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 50, FkIdPerfil = 5, Modulo = "Perfis", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 51, FkIdPerfil = 5, Modulo = "Atividades", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 52, FkIdPerfil = 5, Modulo = "Comunidades", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 53, FkIdPerfil = 5, Modulo = "Vulnerabilidades", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 54, FkIdPerfil = 5, Modulo = "Recursos", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 55, FkIdPerfil = 5, Modulo = "DiariosCampo", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 56, FkIdPerfil = 5, Modulo = "Atores", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 57, FkIdPerfil = 5, Modulo = "Ficha1Contato", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 58, FkIdPerfil = 5, Modulo = "DiariosProcessoPessoal", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 59, FkIdPerfil = 5, Modulo = "AvaliacoesPessoais", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 60, FkIdPerfil = 5, Modulo = "SER", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" }
            );
        }

        private static Usuario IdentitySeed(
            int id,
            string name,
            string email,
            string occupation,
            int? gender,
            int profileId,
            string active,
            string photo,
            DateTime birthDate)
        {
            var normalizedEmail = email.ToUpperInvariant();
            return new Usuario
            {
                IdUsuario = id,
                UserName = email,
                NormalizedUserName = normalizedEmail,
                Email = email,
                NormalizedEmail = normalizedEmail,
                EmailConfirmed = true,
                Senha = "AQAAAAIAAYagAAAAEJcfohm0J9StjpodK4pthBMssFrYtCteqHFi8rtfIPs+0mjn9jbeYSGV2ri/Iq2tIA==",
                SecurityStamp = $"seed-security-stamp-{id}",
                ConcurrencyStamp = $"seed-concurrency-stamp-{id}",
                LockoutEnabled = true,
                Nome = name,
                Foto = photo,
                Ocupacao = occupation,
                Genero = gender,
                DtNascimento = birthDate,
                DtCriacao = new DateTime(2024, Math.Min(id, 12), 1),
                DtAtualizacao = new DateTime(2025, Math.Min(id, 12), 1),
                FkIdPerfil = profileId,
                Ativo = active,
                IdiomaPreferido = IdiomaPreferido.Default
            };
        }

        private void PrepareIdentityFields()
        {
            foreach (var entry in ChangeTracker.Entries<Usuario>()
                         .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
            {
                var user = entry.Entity;
                user.Email = user.Email.Trim();
                user.UserName = string.IsNullOrWhiteSpace(user.UserName) ? user.Email : user.UserName.Trim();
                user.NormalizedEmail = user.Email.ToUpperInvariant();
                user.NormalizedUserName = user.UserName.ToUpperInvariant();
                user.SecurityStamp = string.IsNullOrWhiteSpace(user.SecurityStamp)
                    ? Guid.NewGuid().ToString("N")
                    : user.SecurityStamp;
                user.ConcurrencyStamp = string.IsNullOrWhiteSpace(user.ConcurrencyStamp)
                    ? Guid.NewGuid().ToString("N")
                    : user.ConcurrencyStamp;
            }
        }

        private void PrepareRandomPrimaryKeys()
        {
            var targets = GetRandomKeyTargets();
            if (targets.Count == 0)
                return;

            var reservedKeys = GetReservedKeys(targets);
            var connection = Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;

            if (shouldClose)
                connection.Open();

            try
            {
                foreach (var target in targets.Where(target => target.NeedsGeneration))
                {
                    var key = GenerateAvailableKey(target, reservedKeys);
                    var propertyEntry = target.Entry.Property(target.PropertyName);
                    propertyEntry.CurrentValue = key;
                    propertyEntry.IsTemporary = false;
                    reservedKeys.Add(target.ReservationKey(key));
                }
            }
            finally
            {
                if (shouldClose)
                    connection.Close();
            }

            ChangeTracker.DetectChanges();
        }

        private async Task PrepareRandomPrimaryKeysAsync(CancellationToken cancellationToken)
        {
            var targets = GetRandomKeyTargets();
            if (targets.Count == 0)
                return;

            var reservedKeys = GetReservedKeys(targets);
            var connection = Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;

            if (shouldClose)
                await connection.OpenAsync(cancellationToken);

            try
            {
                foreach (var target in targets.Where(target => target.NeedsGeneration))
                {
                    var key = await GenerateAvailableKeyAsync(target, reservedKeys, cancellationToken);
                    var propertyEntry = target.Entry.Property(target.PropertyName);
                    propertyEntry.CurrentValue = key;
                    propertyEntry.IsTemporary = false;
                    reservedKeys.Add(target.ReservationKey(key));
                }
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }

            ChangeTracker.DetectChanges();
        }

        private List<RandomKeyTarget> GetRandomKeyTargets()
        {
            ChangeTracker.DetectChanges();
            var targets = new List<RandomKeyTarget>();

            foreach (var entry in ChangeTracker.Entries().Where(entry => entry.State == EntityState.Added))
            {
                var primaryKey = entry.Metadata.FindPrimaryKey();
                if (primaryKey?.Properties.Count != 1 || primaryKey.Properties[0].ClrType != typeof(int))
                    continue;

                var property = primaryKey.Properties[0];
                var tableName = entry.Metadata.GetTableName();
                if (tableName is null)
                    continue;

                var schema = entry.Metadata.GetSchema();
                var storeObject = StoreObjectIdentifier.Table(tableName, schema);
                var columnName = property.GetColumnName(storeObject) ?? property.Name;
                var propertyEntry = entry.Property(property.Name);
                var currentValue = (int)(propertyEntry.CurrentValue ?? 0);
                targets.Add(new RandomKeyTarget(
                    entry,
                    property.Name,
                    tableName,
                    schema,
                    columnName,
                    currentValue,
                    propertyEntry.IsTemporary || currentValue == 0));
            }

            return targets;
        }

        private static HashSet<string> GetReservedKeys(IEnumerable<RandomKeyTarget> targets) =>
            targets
                .Where(target => !target.NeedsGeneration)
                .Select(target => target.ReservationKey(target.CurrentValue))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

        private int GenerateAvailableKey(RandomKeyTarget target, ISet<string> reservedKeys)
        {
            for (var attempt = 0; attempt < 128; attempt++)
            {
                var candidate = RandomNumberGenerator.GetInt32(100000, 1000000);
                if (reservedKeys.Contains(target.ReservationKey(candidate)) || KeyExists(target, candidate))
                    continue;

                return candidate;
            }

            throw new InvalidOperationException($"Não foi possível gerar uma chave aleatória livre para {target.TableName}.");
        }

        private async Task<int> GenerateAvailableKeyAsync(
            RandomKeyTarget target,
            ISet<string> reservedKeys,
            CancellationToken cancellationToken)
        {
            for (var attempt = 0; attempt < 128; attempt++)
            {
                var candidate = RandomNumberGenerator.GetInt32(100000, 1000000);
                if (reservedKeys.Contains(target.ReservationKey(candidate)) ||
                    await KeyExistsAsync(target, candidate, cancellationToken))
                    continue;

                return candidate;
            }

            throw new InvalidOperationException($"Não foi possível gerar uma chave aleatória livre para {target.TableName}.");
        }

        private bool KeyExists(RandomKeyTarget target, int candidate)
        {
            using var command = CreateKeyLookupCommand(target, candidate);
            return command.ExecuteScalar() is not null;
        }

        private async Task<bool> KeyExistsAsync(
            RandomKeyTarget target,
            int candidate,
            CancellationToken cancellationToken)
        {
            await using var command = CreateKeyLookupCommand(target, candidate);
            return await command.ExecuteScalarAsync(cancellationToken) is not null;
        }

        private System.Data.Common.DbCommand CreateKeyLookupCommand(RandomKeyTarget target, int candidate)
        {
            var command = Database.GetDbConnection().CreateCommand();
            var qualifiedTable = string.IsNullOrWhiteSpace(target.Schema)
                ? QuoteIdentifier(target.TableName)
                : $"{QuoteIdentifier(target.Schema)}.{QuoteIdentifier(target.TableName)}";
            command.CommandText =
                $"SELECT 1 FROM {qualifiedTable} WHERE {QuoteIdentifier(target.ColumnName)} = @id LIMIT 1";
            command.Transaction = Database.CurrentTransaction?.GetDbTransaction();

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@id";
            parameter.Value = candidate;
            command.Parameters.Add(parameter);
            return command;
        }

        private static string QuoteIdentifier(string identifier) => $"`{identifier.Replace("`", "``")}`";

        private sealed record RandomKeyTarget(
            EntityEntry Entry,
            string PropertyName,
            string TableName,
            string? Schema,
            string ColumnName,
            int CurrentValue,
            bool NeedsGeneration)
        {
            public string ReservationKey(int value) => $"{Schema}.{TableName}.{ColumnName}:{value}";
        }
    }
}
