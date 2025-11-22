using Microsoft.EntityFrameworkCore;
using Empodera.Models;

namespace Empodera.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

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
        public DbSet<AvaliacaoPessoal> AvaliacoesPessoais { get; set; } = null!;
        public DbSet<FichaPrimeiroContato> FichasPrimeiroContato { get; set; } = null!;
        public DbSet<FonteInf> FontesInfo { get; set; } = null!;
        public DbSet<FichaCondicoes> FichaCondicoes { get; set; } = null!;
        public DbSet<FichaPeticoes> FichaPeticoes { get; set; } = null!;
        public DbSet<FichaResp> FichaRespostas { get; set; } = null!;
        public DbSet<FichaResult> FichaResultados { get; set; } = null!;
        public DbSet<Atividades> Atividades { get; set; } = null!;
        public DbSet<AtividadesEixo> AtividadesEixo { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ============================
            // Primary keys (conformes aos models)
            // ============================
            modelBuilder.Entity<Usuario>().HasKey(u => u.IdUsuario);
            modelBuilder.Entity<Perfil>().HasKey(p => p.IdPerfil);
            modelBuilder.Entity<Permissoes>().HasKey(p => p.IdPermissoes);
            modelBuilder.Entity<Comunidade>().HasKey(c => c.IdComunidade);
            modelBuilder.Entity<Atores>().HasKey(a => a.IdAtores);
            modelBuilder.Entity<RedeRecursos>().HasKey(r => r.IdRede);
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

            // ============================
            // RELACIONAMENTOS (Fluent API)
            // Obs: navegações usadas conforme os models fornecidos
            // ============================

            // Perfil -> Usuario (many Perfis belong to one Usuario)
            modelBuilder.Entity<Perfil>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.Perfis)
                .HasForeignKey(p => p.FkIdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

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
                .HasForeignKey(c => c.FkIdUsuario)
                .OnDelete(DeleteBehavior.SetNull);

            // Atores -> Usuario (many Atores belong to one Usuario)
            modelBuilder.Entity<Atores>()
                .HasOne(a => a.Usuario)
                .WithMany(u => u.Atores)
                .HasForeignKey(a => a.FkIdUsuario)
                .OnDelete(DeleteBehavior.SetNull);

            // RedeRecursos -> Atores and Comunidade and Usuario
            modelBuilder.Entity<RedeRecursos>()
                .HasOne(r => r.Ator)
                .WithMany(a => a.Redes)
                .HasForeignKey(r => r.FKidAtores)
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
                .OnDelete(DeleteBehavior.SetNull);

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
                .HasForeignKey(ac => ac.FKidAtores)
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
                .OnDelete(DeleteBehavior.SetNull);

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
                .HasForeignKey(d => d.FKidAtores)
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
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AcoesAtores>()
                .HasOne(aa => aa.Acoes)
                .WithMany(a => a.AcoesAtores)
                .HasForeignKey(aa => aa.FkIdAcoes)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AcoesAtores>()
                .HasOne(aa => aa.Ator)
                .WithMany(a => a.AcoesAtores)
                .HasForeignKey(aa => aa.FKidAtores)
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
                .HasForeignKey(ap => ap.FKidAtores)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AvaliacaoPessoal>()
                .HasOne(ap => ap.Usuario)
                .WithMany(u => u.Avaliacoes)
                .HasForeignKey(ap => ap.FkIdUsuario)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FichaPrimeiroContato>()
                .HasOne(fp => fp.Ator)
                .WithMany(a => a.FichasPrimeiroContato)
                .HasForeignKey(fp => fp.FKidAtores)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FichaPrimeiroContato>()
                .HasOne(fp => fp.Usuario)
                .WithMany(u => u.FichasPrimeiroContato)
                .HasForeignKey(fp => fp.FkIdUsuario)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FonteInf>()
                .HasOne(fi => fi.Ficha)
                .WithMany(f => f.Fontes)
                .HasForeignKey(fi => fi.FkIdFicha)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FichaCondicoes>()
                .HasOne(fc => fc.Ficha)
                .WithMany(f => f.Condicoes)
                .HasForeignKey(fc => fc.FkIdFicha)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FichaPeticoes>()
                .HasOne(fp => fp.Ficha)
                .WithMany(f => f.Peticoes)
                .HasForeignKey(fp => fp.FkIdFicha)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FichaResp>()
                .HasOne(fr => fr.Ficha)
                .WithMany(f => f.Respostas)
                .HasForeignKey(fr => fr.FkIdFicha)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FichaResult>()
                .HasOne(fr => fr.Ficha)
                .WithMany(f => f.Resultados)
                .HasForeignKey(fr => fr.FkIdFicha)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Atividades>()
                .HasOne(at => at.Comunidade)
                .WithMany(c => c.Atividades)
                .HasForeignKey(at => at.FkIdComunidade)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Atividades>()
                .HasOne(at => at.Usuario)
                .WithMany(u => u.Atividades)
                .HasForeignKey(at => at.FkIdUsuario)
                .OnDelete(DeleteBehavior.SetNull);

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

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>().HasData(
                new Usuario { IdUsuario = 1, Nome = "joao", Senha = "123456", Foto = "foto1.jpg", Email = "joao@email.com", Ocupacao = "Coordenador", Genero = "M", DtNascimento = new DateTime(1990,1,1), NivelPermissao = 1, DtCriacao = new DateTime(2024,1,1), DtAtualizacao = new DateTime(2025,1,1) },
                new Usuario { IdUsuario = 2, Nome = "Usuario Dois", Senha = "senha2", Foto = "foto2.jpg", Email = "u2@example.com", Ocupacao = "Pesquisador", Genero = "F", DtNascimento = new DateTime(1985,2,2), NivelPermissao = 2, DtCriacao = new DateTime(2024,2,1), DtAtualizacao = new DateTime(2025,2,1) },
                new Usuario { IdUsuario = 3, Nome = "Usuario Tres", Senha = "senha3", Foto = "foto3.jpg", Email = "u3@example.com", Ocupacao = "Voluntario", Genero = "M", DtNascimento = new DateTime(1995,3,3), NivelPermissao = 1, DtCriacao = new DateTime(2024,3,1), DtAtualizacao = new DateTime(2025,3,1) },
                new Usuario { IdUsuario = 4, Nome = "Usuario Quatro", Senha = "senha4", Foto = "foto4.jpg", Email = "u4@example.com", Ocupacao = "Analista", Genero = "F", DtNascimento = new DateTime(1992,4,4), NivelPermissao = 2, DtCriacao = new DateTime(2024,4,1), DtAtualizacao = new DateTime(2025,4,1) },
                new Usuario { IdUsuario = 5, Nome = "Usuario Cinco", Senha = "senha5", Foto = "foto5.jpg", Email = "u5@example.com", Ocupacao = "Gerente", Genero = "M", DtNascimento = new DateTime(1988,5,5), NivelPermissao = 3, DtCriacao = new DateTime(2024,5,1), DtAtualizacao = new DateTime(2025,5,1) }
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
                new Permissoes { IdPermissoes = 1, FkIdPerfil = 1, Permissao = "Todas", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "S" },
                new Permissoes { IdPermissoes = 2, FkIdPerfil = 2, Permissao = "Conteudo", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 3, FkIdPerfil = 3, Permissao = "Campo", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 4, FkIdPerfil = 4, Permissao = "Leitura", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "N", PodeAtualizar = "N", PodeDeletar = "N" },
                new Permissoes { IdPermissoes = 5, FkIdPerfil = 5, Permissao = "Gerencia", PodeListar = "S", PodeDetalhar = "S", PodeCriar = "S", PodeAtualizar = "S", PodeDeletar = "N" }
            );

            // Comunidades
            modelBuilder.Entity<Comunidade>().HasData(
                new Comunidade { IdComunidade = 1, Nome = "Comunidade Alpha", Local = "Bairro A", Status = "Ativa", Complemento = "", Descricao = "Comunidade piloto", DescricaoAcessibilidade = "Rampa", DtCriacao = new DateTime(2023,1,1), DtModificacao = new DateTime(2025,1,1), FkIdUsuario = 1 },
                new Comunidade { IdComunidade = 2, Nome = "Comunidade Beta", Local = "Bairro B", Status = "Ativa", Complemento = "Sala 2", Descricao = "Comunidade secundária", DescricaoAcessibilidade = "Elevador", DtCriacao = new DateTime(2023,2,1), DtModificacao = new DateTime(2025,2,1), FkIdUsuario = 2 },
                new Comunidade { IdComunidade = 3, Nome = "Comunidade Gamma", Local = "Bairro C", Status = "Inativa", Complemento = "", Descricao = "Comunidade remota", DescricaoAcessibilidade = "Rampas", DtCriacao = new DateTime(2023,3,1), DtModificacao = new DateTime(2025,3,1), FkIdUsuario = 3 },
                new Comunidade { IdComunidade = 4, Nome = "Comunidade Delta", Local = "Bairro D", Status = "Ativa", Complemento = "Anexo", Descricao = "Comunidade urbana", DescricaoAcessibilidade = "Acesso", DtCriacao = new DateTime(2023,4,1), DtModificacao = new DateTime(2025,4,1), FkIdUsuario = 4 },
                new Comunidade { IdComunidade = 5, Nome = "Comunidade Epsilon", Local = "Bairro E", Status = "Ativa", Complemento = "", Descricao = "Comunidade rural", DescricaoAcessibilidade = "Sem acesso especial", DtCriacao = new DateTime(2023,5,1), DtModificacao = new DateTime(2025,5,1), FkIdUsuario = 5 }
            );

            // Atores
            modelBuilder.Entity<Atores>().HasData(
                new Atores { IdAtores = 1, Nome = "Ator 1", Genero = "M", DtNascimento = new DateTime(1990,1,1), PapelSocial1 = "Lider", PapelSocial2 = "Voluntario", Telefone = "11900000001", Extra = "", DtCriacao = new DateTime(2024,1,1), DtModificacao = new DateTime(2025,1,1), Status = "Ativo", MotivoStatus = "", FkIdUsuario = 1 },
                new Atores { IdAtores = 2, Nome = "Ator 2", Genero = "F", DtNascimento = new DateTime(1992,2,2), PapelSocial1 = "Beneficiario", PapelSocial2 = "Membro", Telefone = "11900000002", Extra = "", DtCriacao = new DateTime(2024,2,1), DtModificacao = new DateTime(2025,2,1), Status = "Ativo", MotivoStatus = "", FkIdUsuario = 1 },
                new Atores { IdAtores = 3, Nome = "Ator 3", Genero = "M", DtNascimento = new DateTime(1985,3,3), PapelSocial1 = "Parceiro", PapelSocial2 = "Voluntario", Telefone = "11900000003", Extra = "", DtCriacao = new DateTime(2024,3,1), DtModificacao = new DateTime(2025,3,1), Status = "Ativo", MotivoStatus = "", FkIdUsuario = 2 },

                new Atores { IdAtores = 4, Nome = "Ator 4", Genero = "F", DtNascimento = new DateTime(1991,4,4), PapelSocial1 = "Lider", PapelSocial2 = "Coordenador", Telefone = "11900000004", Extra = "", DtCriacao = new DateTime(2024,4,1), DtModificacao = new DateTime(2025,4,1), Status = "Ativo", MotivoStatus = "", FkIdUsuario = 2 },
                new Atores { IdAtores = 5, Nome = "Ator 5", Genero = "M", DtNascimento = new DateTime(1988,5,5), PapelSocial1 = "Beneficiario", PapelSocial2 = "Voluntario", Telefone = "11900000005", Extra = "", DtCriacao = new DateTime(2024,5,1), DtModificacao = new DateTime(2025,5,1), Status = "Ativo", MotivoStatus = "", FkIdUsuario = 3 },
                new Atores { IdAtores = 6, Nome = "Ator 6", Genero = "F", DtNascimento = new DateTime(1993,6,6), PapelSocial1 = "Parceiro", PapelSocial2 = "Membro", Telefone = "11900000006", Extra = "", DtCriacao = new DateTime(2024,6,1), DtModificacao = new DateTime(2025,6,1), Status = "Ativo", MotivoStatus = "", FkIdUsuario = 3 },

                new Atores { IdAtores = 7, Nome = "Ator 7", Genero = "M", DtNascimento = new DateTime(1994,7,7), PapelSocial1 = "Lider", PapelSocial2 = "Voluntario", Telefone = "11900000007", Extra = "", DtCriacao = new DateTime(2024,7,1), DtModificacao = new DateTime(2025,7,1), Status = "Ativo", MotivoStatus = "", FkIdUsuario = 4 },
                new Atores { IdAtores = 8, Nome = "Ator 8", Genero = "F", DtNascimento = new DateTime(1995,8,8), PapelSocial1 = "Beneficiario", PapelSocial2 = "Membro", Telefone = "11900000008", Extra = "", DtCriacao = new DateTime(2024,8,1), DtModificacao = new DateTime(2025,8,1), Status = "Ativo", MotivoStatus = "", FkIdUsuario = 4 },
                new Atores { IdAtores = 9, Nome = "Ator 9", Genero = "M", DtNascimento = new DateTime(1996,9,9), PapelSocial1 = "Parceiro", PapelSocial2 = "Voluntario", Telefone = "11900000009", Extra = "", DtCriacao = new DateTime(2024,9,1), DtModificacao = new DateTime(2025,9,1), Status = "Ativo", MotivoStatus = "", FkIdUsuario = 5 },

                new Atores { IdAtores = 10, Nome = "Ator 10", Genero = "F", DtNascimento = new DateTime(1987,10,10), PapelSocial1 = "Lider", PapelSocial2 = "Coordenador", Telefone = "11900000010", Extra = "", DtCriacao = new DateTime(2024,10,1), DtModificacao = new DateTime(2025,10,1), Status = "Ativo", MotivoStatus = "", FkIdUsuario = 5 },
                new Atores { IdAtores = 11, Nome = "Ator 11", Genero = "M", DtNascimento = new DateTime(1986,11,11), PapelSocial1 = "Beneficiario", PapelSocial2 = "Membro", Telefone = "11900000011", Extra = "", DtCriacao = new DateTime(2024,11,1), DtModificacao = new DateTime(2025,11,1), Status = "Ativo", MotivoStatus = "", FkIdUsuario = 1 },
                new Atores { IdAtores = 12, Nome = "Ator 12", Genero = "F", DtNascimento = new DateTime(1989,12,12), PapelSocial1 = "Parceiro", PapelSocial2 = "Voluntario", Telefone = "11900000012", Extra = "", DtCriacao = new DateTime(2024,12,1), DtModificacao = new DateTime(2025,12,1), Status = "Ativo", MotivoStatus = "", FkIdUsuario = 2 },

                new Atores { IdAtores = 13, Nome = "Ator 13", Genero = "M", DtNascimento = new DateTime(1997,1,13), PapelSocial1 = "Membro", PapelSocial2 = "", Telefone = "11900000013", Extra = "", DtCriacao = new DateTime(2025,1,1), DtModificacao = new DateTime(2025,1,1), Status = "Ativo", MotivoStatus = "", FkIdUsuario = 3 },
                new Atores { IdAtores = 14, Nome = "Ator 14", Genero = "F", DtNascimento = new DateTime(1998,2,14), PapelSocial1 = "Membro", PapelSocial2 = "", Telefone = "11900000014", Extra = "", DtCriacao = new DateTime(2025,2,1), DtModificacao = new DateTime(2025,2,1), Status = "Ativo", MotivoStatus = "", FkIdUsuario = 4 },
                new Atores { IdAtores = 15, Nome = "Ator 15", Genero = "M", DtNascimento = new DateTime(1979,3,15), PapelSocial1 = "Membro", PapelSocial2 = "", Telefone = "11900000015", Extra = "", DtCriacao = new DateTime(2025,3,1), DtModificacao = new DateTime(2025,3,1), Status = "Ativo", MotivoStatus = "", FkIdUsuario = 5 }
            );
            modelBuilder.Entity<AtorComunidade>().HasData(
                new AtorComunidade { IdAtorComunidade = 1, FkIdComunidade = 1, FKidAtores = 1 },
                new AtorComunidade { IdAtorComunidade = 2, FkIdComunidade = 1, FKidAtores = 2 },
                new AtorComunidade { IdAtorComunidade = 3, FkIdComunidade = 1, FKidAtores = 3 },

                new AtorComunidade { IdAtorComunidade = 4, FkIdComunidade = 2, FKidAtores = 4 },
                new AtorComunidade { IdAtorComunidade = 5, FkIdComunidade = 2, FKidAtores = 5 },
                new AtorComunidade { IdAtorComunidade = 6, FkIdComunidade = 2, FKidAtores = 6 },

                new AtorComunidade { IdAtorComunidade = 7, FkIdComunidade = 3, FKidAtores = 7 },
                new AtorComunidade { IdAtorComunidade = 8, FkIdComunidade = 3, FKidAtores = 8 },
                new AtorComunidade { IdAtorComunidade = 9, FkIdComunidade = 3, FKidAtores = 9 },

                new AtorComunidade { IdAtorComunidade = 10, FkIdComunidade = 4, FKidAtores = 10 },
                new AtorComunidade { IdAtorComunidade = 11, FkIdComunidade = 4, FKidAtores = 11 },
                new AtorComunidade { IdAtorComunidade = 12, FkIdComunidade = 4, FKidAtores = 12 },

                new AtorComunidade { IdAtorComunidade = 13, FkIdComunidade = 5, FKidAtores = 13 },
                new AtorComunidade { IdAtorComunidade = 14, FkIdComunidade = 5, FKidAtores = 14 },
                new AtorComunidade { IdAtorComunidade = 15, FkIdComunidade = 5, FKidAtores = 15 }
            );

            // RedeRecursos
            modelBuilder.Entity<RedeRecursos>().HasData(
                new RedeRecursos { IdRede = 1, FKidAtores = 1, FkIdComunidade = 1, Tipo = "Wifi", Dispositivo = "Router", Servicos = "Internet", DtCriacao = new DateTime(2024,1,1), DtModificacao = new DateTime(2025,1,1), FkIdUsuario = 1 },
                new RedeRecursos { IdRede = 2, FKidAtores = 5, FkIdComunidade = 2, Tipo = "Ponto", Dispositivo = "Switch", Servicos = "Conexão", DtCriacao = new DateTime(2024,2,1), DtModificacao = new DateTime(2025,2,1), FkIdUsuario = 2 },
                new RedeRecursos { IdRede = 3, FKidAtores = 9, FkIdComunidade = 3, Tipo = "Fibra", Dispositivo = "OLT", Servicos = "Backbone", DtCriacao = new DateTime(2024,3,1), DtModificacao = new DateTime(2025,3,1), FkIdUsuario = 3 },
                new RedeRecursos { IdRede = 4, FKidAtores = 12, FkIdComunidade = 4, Tipo = "4G", Dispositivo = "Modem", Servicos = "Dados", DtCriacao = new DateTime(2024,4,1), DtModificacao = new DateTime(2025,4,1), FkIdUsuario = 4 },
                new RedeRecursos { IdRede = 5, FKidAtores = 15, FkIdComunidade = 5, Tipo = "Sat", Dispositivo = "Dish", Servicos = "Satélite", DtCriacao = new DateTime(2024,5,1), DtModificacao = new DateTime(2025,5,1), FkIdUsuario = 5 }
            );

            // Eixos
            modelBuilder.Entity<Eixo>().HasData(
                new Eixo { IdEixo = 1, Nome = "prevenção" },
                new Eixo { IdEixo = 2, Nome = "ocupação" },
                new Eixo { IdEixo = 3, Nome = "lazer" },
                new Eixo { IdEixo = 4, Nome = "segurança social" },
                new Eixo { IdEixo = 5, Nome = "educação" },
                new Eixo { IdEixo = 6, Nome = "saúde" },
                new Eixo { IdEixo = 7, Nome = "assistência básica" }
            );

            // RedeEixo
            modelBuilder.Entity<RedeEixo>().HasData(
                new RedeEixo { IdRedeEixo = 1, FkIdRede = 1, FkIdEixo = 1 },
                new RedeEixo { IdRedeEixo = 2, FkIdRede = 2, FkIdEixo = 2 },
                new RedeEixo { IdRedeEixo = 3, FkIdRede = 3, FkIdEixo = 3 },
                new RedeEixo { IdRedeEixo = 4, FkIdRede = 4, FkIdEixo = 4 },
                new RedeEixo { IdRedeEixo = 5, FkIdRede = 5, FkIdEixo = 5 }
            );

            // DiarioCampo
            modelBuilder.Entity<DiarioCampo>().HasData(
                new DiarioCampo { IdDCampo = 1, FkIdComunidade = 1, Data = new DateTime(2025,1,10), Descricao = "Visita inicial", Localizacao = "Ponto A", DtCriacao = new DateTime(2025,1,10), DtModificacao = new DateTime(2025,1,10), Foto = "d1.jpg", FkIdUsuario = 1 },
                new DiarioCampo { IdDCampo = 2, FkIdComunidade = 2, Data = new DateTime(2025,2,11), Descricao = "Reunião", Localizacao = "Ponto B", DtCriacao = new DateTime(2025,2,11), DtModificacao = new DateTime(2025,2,11), Foto = "d2.jpg", FkIdUsuario = 2 },
                new DiarioCampo { IdDCampo = 3, FkIdComunidade = 3, Data = new DateTime(2025,3,12), Descricao = "Diagnóstico", Localizacao = "Ponto C", DtCriacao = new DateTime(2025,3,12), DtModificacao = new DateTime(2025,3,12), Foto = "d3.jpg", FkIdUsuario = 3 },
                new DiarioCampo { IdDCampo = 4, FkIdComunidade = 4, Data = new DateTime(2025,4,13), Descricao = "Intervenção", Localizacao = "Ponto D", DtCriacao = new DateTime(2025,4,13), DtModificacao = new DateTime(2025,4,13), Foto = "d4.jpg", FkIdUsuario = 4 },
                new DiarioCampo { IdDCampo = 5, FkIdComunidade = 5, Data = new DateTime(2025,5,14), Descricao = "Acompanhamento", Localizacao = "Ponto E", DtCriacao = new DateTime(2025,5,14), DtModificacao = new DateTime(2025,5,14), Foto = "d5.jpg", FkIdUsuario = 5 }
            );

            // DiarioAcoes
            modelBuilder.Entity<DiarioAcoes>().HasData(
                new DiarioAcoes { IdDAcoes = 1, FkIdAcoes = 1, FkIdDiario = 1 },
                new DiarioAcoes { IdDAcoes = 2, FkIdAcoes = 2, FkIdDiario = 2 },
                new DiarioAcoes { IdDAcoes = 3, FkIdAcoes = 3, FkIdDiario = 3 },
                new DiarioAcoes { IdDAcoes = 4, FkIdAcoes = 4, FkIdDiario = 4 },
                new DiarioAcoes { IdDAcoes = 5, FkIdAcoes = 5, FkIdDiario = 5 }
            );

            // DiarioDAcoes
            modelBuilder.Entity<DiarioDAcoes>().HasData(
                new DiarioDAcoes { IdDAcoes = 1, FkIdDiario = 1, Nome = "Coleta", PeovedorEx = "Local", Quantidade = 10 },
                new DiarioDAcoes { IdDAcoes = 2, FkIdDiario = 2, Nome = "Distribuicao", PeovedorEx = "Externo", Quantidade = 5 },
                new DiarioDAcoes { IdDAcoes = 3, FkIdDiario = 3, Nome = "Treinamento", PeovedorEx = "Equipe", Quantidade = 8 },
                new DiarioDAcoes { IdDAcoes = 4, FkIdDiario = 4, Nome = "Levantamento", PeovedorEx = "Parceiro", Quantidade = 12 },
                new DiarioDAcoes { IdDAcoes = 5, FkIdDiario = 5, Nome = "Monitoramento", PeovedorEx = "Equipe", Quantidade = 7 }
            );

            // DetalhesDAcoes
            modelBuilder.Entity<DetalhesDAcoes>().HasData(
                new DetalhesDAcoes { Id = 1, Nome = "Detalhe A", FkIdDDacoes = 1 },
                new DetalhesDAcoes { Id = 2, Nome = "Detalhe B", FkIdDDacoes = 2 },
                new DetalhesDAcoes { Id = 3, Nome = "Detalhe C", FkIdDDacoes = 3 },
                new DetalhesDAcoes { Id = 4, Nome = "Detalhe D", FkIdDDacoes = 4 },
                new DetalhesDAcoes { Id = 5, Nome = "Detalhe E", FkIdDDacoes = 5 }
            );

            // DetalhesEixos
            modelBuilder.Entity<DetalhesEixos>().HasData(
                new DetalhesEixos { IdDiarioEixo = 1, FkIdDetalhes = 1, FkIdEixo = 1 },
                new DetalhesEixos { IdDiarioEixo = 2, FkIdDetalhes = 2, FkIdEixo = 2 },
                new DetalhesEixos { IdDiarioEixo = 3, FkIdDetalhes = 3, FkIdEixo = 3 },
                new DetalhesEixos { IdDiarioEixo = 4, FkIdDetalhes = 4, FkIdEixo = 4 },
                new DetalhesEixos { IdDiarioEixo = 5, FkIdDetalhes = 5, FkIdEixo = 5 }
            );

            // DAAtores
            modelBuilder.Entity<DAAtores>().HasData(
                new DAAtores { Id = 1, FkIdDDacoes = 1, FKidAtores = 1 },
                new DAAtores { Id = 2, FkIdDDacoes = 2, FKidAtores = 4 },
                new DAAtores { Id = 3, FkIdDDacoes = 3, FKidAtores = 7 },
                new DAAtores { Id = 4, FkIdDDacoes = 4, FKidAtores = 10 },
                new DAAtores { Id = 5, FkIdDDacoes = 5, FKidAtores = 13 }
            );

            // DiarioEixo
            modelBuilder.Entity<DiarioEixo>().HasData(
                new DiarioEixo { IdDiarioEixo = 1, FkIdDiario = 1, FkIdEixo = 1 },
                new DiarioEixo { IdDiarioEixo = 2, FkIdDiario = 2, FkIdEixo = 2 },
                new DiarioEixo { IdDiarioEixo = 3, FkIdDiario = 3, FkIdEixo = 3 },
                new DiarioEixo { IdDiarioEixo = 4, FkIdDiario = 4, FkIdEixo = 4 },
                new DiarioEixo { IdDiarioEixo = 5, FkIdDiario = 5, FkIdEixo = 5 }
            );

            // Acoes
            modelBuilder.Entity<Acoes>().HasData(
                new Acoes { IdAcoes = 1, Quantidade = 10, FkIdAtividade = 1, Nome = "Ação 1", Provedor = "Fornecedor A" },
                new Acoes { IdAcoes = 2, Quantidade = 5, FkIdAtividade = 2, Nome = "Ação 2", Provedor = "Fornecedor B" },
                new Acoes { IdAcoes = 3, Quantidade = 8, FkIdAtividade = 3, Nome = "Ação 3", Provedor = "Fornecedor C" },
                new Acoes { IdAcoes = 4, Quantidade = 12, FkIdAtividade = 4, Nome = "Ação 4", Provedor = "Fornecedor D" },
                new Acoes { IdAcoes = 5, Quantidade = 7, FkIdAtividade = 5, Nome = "Ação 5", Provedor = "Fornecedor E" }
            );

            // AcoesAtores
            modelBuilder.Entity<AcoesAtores>().HasData(
                new AcoesAtores { IdAAtores = 1, FKidAtores = 1, FkIdAcoes = 1 },
                new AcoesAtores { IdAAtores = 2, FKidAtores = 4, FkIdAcoes = 2 },
                new AcoesAtores { IdAAtores = 3, FKidAtores = 7, FkIdAcoes = 3 },
                new AcoesAtores { IdAAtores = 4, FKidAtores = 10, FkIdAcoes = 4 },
                new AcoesAtores { IdAAtores = 5, FKidAtores = 13, FkIdAcoes = 5 }
            );

            // AnexosDiario
            modelBuilder.Entity<AnexosDiario>().HasData(
                new AnexosDiario { IdAnexos = 1, FkIdDiario = 1, Caminho = "anexo1.jpg" },
                new AnexosDiario { IdAnexos = 2, FkIdDiario = 2, Caminho = "anexo2.jpg" },
                new AnexosDiario { IdAnexos = 3, FkIdDiario = 3, Caminho = "anexo3.jpg" },
                new AnexosDiario { IdAnexos = 4, FkIdDiario = 4, Caminho = "anexo4.jpg" },
                new AnexosDiario { IdAnexos = 5, FkIdDiario = 5, Caminho = "anexo5.jpg" }
            );

            // Vulnerabilidade
            modelBuilder.Entity<Vulnerabilidade>().HasData(
                new Vulnerabilidade { IdVulnerabilidade = 1, Nome = "Vuln 1", Localizacao = "Local 1", Servicos = "Energia", FkIdComunidade = 1 },
                new Vulnerabilidade { IdVulnerabilidade = 2, Nome = "Vuln 2", Localizacao = "Local 2", Servicos = "Agua", FkIdComunidade = 2 },
                new Vulnerabilidade { IdVulnerabilidade = 3, Nome = "Vuln 3", Localizacao = "Local 3", Servicos = "Saude", FkIdComunidade = 3 },
                new Vulnerabilidade { IdVulnerabilidade = 4, Nome = "Vuln 4", Localizacao = "Local 4", Servicos = "Transporte", FkIdComunidade = 4 },
                new Vulnerabilidade { IdVulnerabilidade = 5, Nome = "Vuln 5", Localizacao = "Local 5", Servicos = "Comunicacao", FkIdComunidade = 5 }
            );

            // VulnerabilidadesEixo
            modelBuilder.Entity<VulnerabilidadesEixo>().HasData(
                new VulnerabilidadesEixo { IdVEixo = 1, FkIdEixo = 1, FkIdVulnerabilidade = 1 },
                new VulnerabilidadesEixo { IdVEixo = 2, FkIdEixo = 2, FkIdVulnerabilidade = 2 },
                new VulnerabilidadesEixo { IdVEixo = 3, FkIdEixo = 3, FkIdVulnerabilidade = 3 },
                new VulnerabilidadesEixo { IdVEixo = 4, FkIdEixo = 4, FkIdVulnerabilidade = 4 },
                new VulnerabilidadesEixo { IdVEixo = 5, FkIdEixo = 5, FkIdVulnerabilidade = 5 }
            );

            // RedePrimaria
            modelBuilder.Entity<RedePrimaria>().HasData(
                new RedePrimaria { IdRedePrimaria = 1, FkIdAtorPrincipal = 1, FkIdAtorRelacionados = 2, TipoRelacao = "Parceria" },
                new RedePrimaria { IdRedePrimaria = 2, FkIdAtorPrincipal = 4, FkIdAtorRelacionados = 5, TipoRelacao = "Suporte" },
                new RedePrimaria { IdRedePrimaria = 3, FkIdAtorPrincipal = 7, FkIdAtorRelacionados = 8, TipoRelacao = "Rede" },
                new RedePrimaria { IdRedePrimaria = 4, FkIdAtorPrincipal = 10, FkIdAtorRelacionados = 11, TipoRelacao = "Par" },
                new RedePrimaria { IdRedePrimaria = 5, FkIdAtorPrincipal = 13, FkIdAtorRelacionados = 14, TipoRelacao = "Ligacao" }
            );

            // AvaliacaoPessoal
            modelBuilder.Entity<AvaliacaoPessoal>().HasData(
                new AvaliacaoPessoal { IdAvaliacao = 1, FKidAtores = 1, CCrimes = 1, Substancias = 0, Moradia = 2, Prevencao = 3, AssBasica = 4, Educacao = 3, Saude = 2, Ocupacao = 1, Lazer = 2, DtCriacao = new DateTime(2025,1,1), DtModificacao = new DateTime(2025,1,2), FkIdUsuario = 1 },
                new AvaliacaoPessoal { IdAvaliacao = 2, FKidAtores = 5, CCrimes = 0, Substancias = 1, Moradia = 3, Prevencao = 2, AssBasica = 3, Educacao = 4, Saude = 3, Ocupacao = 2, Lazer = 1, DtCriacao = new DateTime(2025,2,1), DtModificacao = new DateTime(2025,2,2), FkIdUsuario = 2 },
                new AvaliacaoPessoal { IdAvaliacao = 3, FKidAtores = 9, CCrimes = 2, Substancias = 1, Moradia = 2, Prevencao = 3, AssBasica = 2, Educacao = 2, Saude = 3, Ocupacao = 1, Lazer = 2, DtCriacao = new DateTime(2025,3,1), DtModificacao = new DateTime(2025,3,2), FkIdUsuario = 3 },
                new AvaliacaoPessoal { IdAvaliacao = 4, FKidAtores = 12, CCrimes = 0, Substancias = 0, Moradia = 4, Prevencao = 4, AssBasica = 4, Educacao = 4, Saude = 4, Ocupacao = 3, Lazer = 3, DtCriacao = new DateTime(2025,4,1), DtModificacao = new DateTime(2025,4,2), FkIdUsuario = 4 },
                new AvaliacaoPessoal { IdAvaliacao = 5, FKidAtores = 15, CCrimes = 3, Substancias = 2, Moradia = 1, Prevencao = 2, AssBasica = 1, Educacao = 1, Saude = 1, Ocupacao = 1, Lazer = 1, DtCriacao = new DateTime(2025,5,1), DtModificacao = new DateTime(2025,5,2), FkIdUsuario = 5 }
            );

            // FichaPrimeiroContato
            modelBuilder.Entity<FichaPrimeiroContato>().HasData(
                new FichaPrimeiroContato { IdFicha = 1, FKidAtores = 1, Localizacao = "Local A", Data = new DateTime(2025,1,2), LContato = "Nome A", FonteDados = "Entrevista", EstaFamiliar = "Sim", EstruFamiliar = "Nuclear", NFIlhos = 1, NFilhas = 0, AEscolar = 10, SLer = "Sim", SCalc = "Sim", SComp = "Sim", QReabili = 0, LTrat = "Nenhum", Coment = "Info A", CPrimeiroContato = "Contato A", EParceiro = "Nao", FPeloParceirto = "Nao", DtContato = new DateTime(2025,1,2), DtCriacao = new DateTime(2025,1,2), DtModificacao = new DateTime(2025,1,3), FkIdUsuario = 1 },
                new FichaPrimeiroContato { IdFicha = 2, FKidAtores = 5, Localizacao = "Local B", Data = new DateTime(2025,2,2), LContato = "Nome B", FonteDados = "Formulario", EstaFamiliar = "Nao", EstruFamiliar = "Extendida", NFIlhos = 2, NFilhas = 1, AEscolar = 8, SLer = "Nao", SCalc = "Sim", SComp = "Nao", QReabili = 1, LTrat = "Sim", Coment = "Info B", CPrimeiroContato = "Contato B", EParceiro = "Sim", FPeloParceirto = "Sim", DtContato = new DateTime(2025,2,2), DtCriacao = new DateTime(2025,2,2), DtModificacao = new DateTime(2025,2,3), FkIdUsuario = 2 },
                new FichaPrimeiroContato { IdFicha = 3, FKidAtores = 9, Localizacao = "Local C", Data = new DateTime(2025,3,2), LContato = "Nome C", FonteDados = "Observacao", EstaFamiliar = "Sim", EstruFamiliar = "Nuclear", NFIlhos = 0, NFilhas = 0, AEscolar = 12, SLer = "Sim", SCalc = "Sim", SComp = "Sim", QReabili = 0, LTrat = "Nao", Coment = "Info C", CPrimeiroContato = "Contato C", EParceiro = "Nao", FPeloParceirto = "Nao", DtContato = new DateTime(2025,3,2), DtCriacao = new DateTime(2025,3,2), DtModificacao = new DateTime(2025,3,3), FkIdUsuario = 3 },
                new FichaPrimeiroContato { IdFicha = 4, FKidAtores = 12, Localizacao = "Local D", Data = new DateTime(2025,4,2), LContato = "Nome D", FonteDados = "Sistema", EstaFamiliar = "Nao", EstruFamiliar = "Extendida", NFIlhos = 3, NFilhas = 2, AEscolar = 6, SLer = "Nao", SCalc = "Nao", SComp = "Nao", QReabili = 2, LTrat = "Sim", Coment = "Info D", CPrimeiroContato = "Contato D", EParceiro = "Sim", FPeloParceirto = "Sim", DtContato = new DateTime(2025,4,2), DtCriacao = new DateTime(2025,4,2), DtModificacao = new DateTime(2025,4,3), FkIdUsuario = 4 },
                new FichaPrimeiroContato { IdFicha = 5, FKidAtores = 15, Localizacao = "Local E", Data = new DateTime(2025,5,2), LContato = "Nome E", FonteDados = "Formulario", EstaFamiliar = "Sim", EstruFamiliar = "Nuclear", NFIlhos = 0, NFilhas = 1, AEscolar = 9, SLer = "Sim", SCalc = "Nao", SComp = "Sim", QReabili = 0, LTrat = "Nao", Coment = "Info E", CPrimeiroContato = "Contato E", EParceiro = "Nao", FPeloParceirto = "Nao", DtContato = new DateTime(2025,5,2), DtCriacao = new DateTime(2025,5,2), DtModificacao = new DateTime(2025,5,3), FkIdUsuario = 5 }
            );

            // FonteInf
            modelBuilder.Entity<FonteInf>().HasData(
                new FonteInf { IdFonte = 1, FkIdFicha = 1, Nome = "Fonte A", Genero = "M", Idade = 40, PapelSocial1 = "Parente", PapelSocial2 = "", Telefone = "11911111111", Extra = "" },
                new FonteInf { IdFonte = 2, FkIdFicha = 2, Nome = "Fonte B", Genero = "F", Idade = 35, PapelSocial1 = "Vizin", PapelSocial2 = "", Telefone = "11922222222", Extra = "" },
                new FonteInf { IdFonte = 3, FkIdFicha = 3, Nome = "Fonte C", Genero = "M", Idade = 50, PapelSocial1 = "Agente", PapelSocial2 = "", Telefone = "11933333333", Extra = "" },
                new FonteInf { IdFonte = 4, FkIdFicha = 4, Nome = "Fonte D", Genero = "F", Idade = 28, PapelSocial1 = "Amigo", PapelSocial2 = "", Telefone = "11944444444", Extra = "" },
                new FonteInf { IdFonte = 5, FkIdFicha = 5, Nome = "Fonte E", Genero = "M", Idade = 60, PapelSocial1 = "Lider", PapelSocial2 = "", Telefone = "11955555555", Extra = "" }
            );

            // FichaCondicoes
            modelBuilder.Entity<FichaCondicoes>().HasData(
                new FichaCondicoes { IdCondicoes = 1, FkIdFicha = 1, Cond = "Cond A" },
                new FichaCondicoes { IdCondicoes = 2, FkIdFicha = 2, Cond = "Cond B" },
                new FichaCondicoes { IdCondicoes = 3, FkIdFicha = 3, Cond = "Cond C" },
                new FichaCondicoes { IdCondicoes = 4, FkIdFicha = 4, Cond = "Cond D" },
                new FichaCondicoes { IdCondicoes = 5, FkIdFicha = 5, Cond = "Cond E" }
            );

            // FichaPeticoes
            modelBuilder.Entity<FichaPeticoes>().HasData(
                new FichaPeticoes { IdPeticoes = 1, FkIdFicha = 1, Pet = "Pet A" },
                new FichaPeticoes { IdPeticoes = 2, FkIdFicha = 2, Pet = "Pet B" },
                new FichaPeticoes { IdPeticoes = 3, FkIdFicha = 3, Pet = "Pet C" },
                new FichaPeticoes { IdPeticoes = 4, FkIdFicha = 4, Pet = "Pet D" },
                new FichaPeticoes { IdPeticoes = 5, FkIdFicha = 5, Pet = "Pet E" }
            );

            // FichaResp 
            modelBuilder.Entity<FichaResp>().HasData(
                new FichaResp { IdCondicoes = 1, FkIdFicha = 1, Resp = "Resp A" },
                new FichaResp { IdCondicoes = 2, FkIdFicha = 2, Resp = "Resp B" },
                new FichaResp { IdCondicoes = 3, FkIdFicha = 3, Resp = "Resp C" },
                new FichaResp { IdCondicoes = 4, FkIdFicha = 4, Resp = "Resp D" },
                new FichaResp { IdCondicoes = 5, FkIdFicha = 5, Resp = "Resp E" }
            );

            // FichaResult
            modelBuilder.Entity<FichaResult>().HasData(
                new FichaResult { IdCondicoes = 1, FkIdFicha = 1, Result = "Result A" },
                new FichaResult { IdCondicoes = 2, FkIdFicha = 2, Result = "Result B" },
                new FichaResult { IdCondicoes = 3, FkIdFicha = 3, Result = "Result C" },
                new FichaResult { IdCondicoes = 4, FkIdFicha = 4, Result = "Result D" },
                new FichaResult { IdCondicoes = 5, FkIdFicha = 5, Result = "Result E" }
            );

            // Atividades
            modelBuilder.Entity<Atividades>().HasData(
                new Atividades { IdAtividade = 1, Nome = "Ativ 1", Descricao = "Descricao 1", Foto = "a1.jpg", FkIdComunidade = 1, FkIdUsuario = 1 },
                new Atividades { IdAtividade = 2, Nome = "Ativ 2", Descricao = "Descricao 2", Foto = "a2.jpg", FkIdComunidade = 2, FkIdUsuario = 2 },
                new Atividades { IdAtividade = 3, Nome = "Ativ 3", Descricao = "Descricao 3", Foto = "a3.jpg", FkIdComunidade = 3, FkIdUsuario = 3 },
                new Atividades { IdAtividade = 4, Nome = "Ativ 4", Descricao = "Descricao 4", Foto = "a4.jpg", FkIdComunidade = 4, FkIdUsuario = 4 },
                new Atividades { IdAtividade = 5, Nome = "Ativ 5", Descricao = "Descricao 5", Foto = "a5.jpg", FkIdComunidade = 5, FkIdUsuario = 5 }
            );

            // AtividadesEixo
            modelBuilder.Entity<AtividadesEixo>().HasData(
                new AtividadesEixo { IdAEixo = 1, FkIdEixo = 1, FkIdAtividade = 1 },
                new AtividadesEixo { IdAEixo = 2, FkIdEixo = 2, FkIdAtividade = 2 },
                new AtividadesEixo { IdAEixo = 3, FkIdEixo = 3, FkIdAtividade = 3 },
                new AtividadesEixo { IdAEixo = 4, FkIdEixo = 4, FkIdAtividade = 4 },
                new AtividadesEixo { IdAEixo = 5, FkIdEixo = 5, FkIdAtividade = 5 }
            );


        }
    }
}
