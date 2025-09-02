using Microsoft.EntityFrameworkCore;
using Entre.Models;

namespace Entre.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Acao> Acoes { get; set; }
        public DbSet<AcoesAtores> AcoesAtores { get; set; }
        public BdSet<AnexoDiario> AnexosDiario { get; set; }
        public DbSet<Atividade> Atividades { get; set; }
        public DbSet<AtividadeEixo> AtividadeEixos { get; set; }
        public DbSet<Ator> Atores { get; set; }
        public DbSet<AtorComunidade> AtorComunidades { get; set; }
        public DbSet<AvaliacaoPessoa> AvaliacaoPessoa { get; set; }
        public DbSet<Comunidade> Comunidades { get; set; }
        public DbSet<DiarioAcoes> DiarioAcoes { get; set; }
        public DbSet<DiarioCampo> DiariosCampo { get; set; }
        public DbSet<DiarioEixo> DiarioEixos { get; set; }
        public DbSet<Eixo> Eixos { get; set; }
        public DbSet<FichaCondicoes> FichasCondicoes { get; set; }
        public DbSet<FichaPeticoes> FichaPeticoes { get; set; }
        public DbSet<FichaResp> FichaResp { get; set; }
        public DbSet<FichaResult> FichaResult { get; set; }
        public DbSet<PerfilAcesso> PerfilAcesso { get; set; }
        public DbSet<Rede> Redes { get; set; }
        public DbSet<RedeEixo> RedeEixos { get; set; }
        public DbSet<RedePrimaria> RedePrimaria { get; set; }
        public DbSet<RedeRecurso> RedeRecursos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<UsuarioPerfil> UsuarioPerfis { get; set; }
        public DbSet<Vulnerabilidade> Vulnerabilidade { get; set; }
        public DbSet<VulnerabilidadesEixo> VulnerabilidadesEixo { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UsuarioPerfil -> Usuario (N:1)
            modelBuilder.Entity<RedePrimaria>()
            .HasOne<Ator>()
            .WithMany()
            .HasForeignKey(rp => rp.AtorRelacionadoId)
            .OnDelete(DeleteBehavior.Restrict);


            // AvaliacaoPessoal -> Ator (N:1)
            modelBuilder.Entity<AvaliacaoPessoal>()
            .HasOne<Ator>()
            .WithMany()
            .HasForeignKey(ap => ap.AtorId)
            .OnDelete(DeleteBehavior.Cascade);


            // FichaPrimeiroContato -> Ator (N:1)
            modelBuilder.Entity<FichaPrimeiroContato>()
            .HasOne<Ator>()
            .WithMany()
            .HasForeignKey(fp => fp.AtorId)
            .OnDelete(DeleteBehavior.Cascade);


            // FichaCondicoes -> FichaPrimeiroContato (N:1)
            modelBuilder.Entity<FichaCondicoes>()
            .HasOne<FichaPrimeiroContato>()
            .WithMany()
            .HasForeignKey(fc => fc.FichaId)
            .OnDelete(DeleteBehavior.Cascade);


            // FichaPeticoes -> FichaPrimeiroContato (N:1)
            modelBuilder.Entity<FichaPeticoes>()
            .HasOne<FichaPrimeiroContato>()
            .WithMany()
            .HasForeignKey(fp => fp.FichaId)
            .OnDelete(DeleteBehavior.Cascade);


            // FichaResp -> FichaPrimeiroContato (N:1)
            modelBuilder.Entity<FichaResp>()
            .HasOne<FichaPrimeiroContato>()
            .WithMany()
            .HasForeignKey(fr => fr.FichaId)
            .OnDelete(DeleteBehavior.Cascade);


            // FichaResult -> FichaPrimeiroContato (N:1)
            modelBuilder.Entity<FichaResult>()
            .HasOne<FichaPrimeiroContato>()
            .WithMany()
            .HasForeignKey(fr => fr.FichaId)
            .OnDelete(DeleteBehavior.Cascade);


            // AtividadesEixo -> Eixo (N:1)
            modelBuilder.Entity<AtividadesEixo>()
            .HasOne<Eixo>()
            .WithMany()
            .HasForeignKey(ae => ae.EixoId)
            .OnDelete(DeleteBehavior.Cascade);


            // AtividadesEixo -> Atividade (N:1)
            modelBuilder.Entity<AtividadesEixo>()
            .HasOne<Atividade>()
            .WithMany()
            .HasForeignKey(ae => ae.AtividadeId)
            .OnDelete(DeleteBehavior.Cascade);

            // Usuario
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario { IdUsuario = 1, Nome = "João Silva", Senha = "123456", Foto = "joao.png", Email = "joao@email.com", Ocupacao = "Professor", Genero = "Masculino", DtNascimento = new DateTime(1990, 5, 12), NivelPermissao = 1, DtCriacao = DateTime.Now },
                new Usuario { IdUsuario = 2, Nome = "Maria Souza", Senha = "123456", Foto = "maria.png", Email = "maria@email.com", Ocupacao = "Engenheira", Genero = "Feminino", DtNascimento = new DateTime(1988, 3, 22), NivelPermissao = 2, DtCriacao = DateTime.Now },
                new Usuario { IdUsuario = 3, Nome = "Carlos Lima", Senha = "123456", Foto = "carlos.png", Email = "carlos@email.com", Ocupacao = "Estudante", Genero = "Masculino", DtNascimento = new DateTime(2000, 7, 15), NivelPermissao = 1, DtCriacao = DateTime.Now },
                new Usuario { IdUsuario = 4, Nome = "Ana Pereira", Senha = "123456", Foto = "ana.png", Email = "ana@email.com", Ocupacao = "Médica", Genero = "Feminino", DtNascimento = new DateTime(1995, 11, 5), NivelPermissao = 3, DtCriacao = DateTime.Now },
                new Usuario { IdUsuario = 5, Nome = "Pedro Santos", Senha = "123456", Foto = "pedro.png", Email = "pedro@email.com", Ocupacao = "Advogado", Genero = "Masculino", DtNascimento = new DateTime(1985, 1, 30), NivelPermissao = 2, DtCriacao = DateTime.Now }
            );

            modelBuilder.Entity<PerfilAcesso>().HasData(
            new PerfilAcesso { IdPAcesso = 1 },
            new PerfilAcesso { IdPAcesso = 2 },
            new PerfilAcesso { IdPAcesso = 3 },
            new PerfilAcesso { IdPAcesso = 4 },
            new PerfilAcesso { IdPAcesso = 5 }
            );


            // ===================== Usuario_Perfil =====================
            modelBuilder.Entity<UsuarioPerfil>().HasData(
            new UsuarioPerfil { IdPUsuario = 1, UsuarioId = 1, PerfilAcessoId = 1 },
            new UsuarioPerfil { IdPUsuario = 2, UsuarioId = 2, PerfilAcessoId = 2 },
            new UsuarioPerfil { IdPUsuario = 3, UsuarioId = 3, PerfilAcessoId = 3 },
            new UsuarioPerfil { IdPUsuario = 4, UsuarioId = 4, PerfilAcessoId = 4 },
            new UsuarioPerfil { IdPUsuario = 5, UsuarioId = 5, PerfilAcessoId = 5 }
            );


            // ===================== Permissoes =====================
            modelBuilder.Entity<Permissao>().HasData(
            new Permissao { IdPermissoes = 1, PerfilAcessoId = 1, PermissaoNome = "Ler" },
            new Permissao { IdPermissoes = 2, PerfilAcessoId = 2, PermissaoNome = "Escrever" },
            new Permissao { IdPermissoes = 3, PerfilAcessoId = 3, PermissaoNome = "Editar" },
            new Permissao { IdPermissoes = 4, PerfilAcessoId = 4, PermissaoNome = "Excluir" },
            new Permissao { IdPermissoes = 5, PerfilAcessoId = 5, PermissaoNome = "Administrador" }
            );


            // ===================== Comunidade =====================
            modelBuilder.Entity<Comunidade>().HasData(
            new Comunidade { IdComunidade = 1, Nome = "Comunidade A", Local = "São Paulo", Status = "Ativa", Complemento = "Zona Leste", Descricao = "Comunidade em SP", DescricaoAcessibilidade = "Acessível", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
            new Comunidade { IdComunidade = 2, Nome = "Comunidade B", Local = "Rio de Janeiro", Status = "Ativa", Complemento = "Zona Norte", Descricao = "Comunidade no RJ", DescricaoAcessibilidade = "Parcial", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
            new Comunidade { IdComunidade = 3, Nome = "Comunidade C", Local = "Belo Horizonte", Status = "Inativa", Complemento = "Centro", Descricao = "Comunidade em BH", DescricaoAcessibilidade = "Baixa", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
            new Comunidade { IdComunidade = 4, Nome = "Comunidade D", Local = "Curitiba", Status = "Ativa", Complemento = "Sul", Descricao = "Comunidade no PR", DescricaoAcessibilidade = "Alta", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
            new Comunidade { IdComunidade = 5, Nome = "Comunidade E", Local = "Salvador", Status = "Ativa", Complemento = "Norte", Descricao = "Comunidade na BA", DescricaoAcessibilidade = "Média", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now }
            );


            // ===================== Atores =====================
            modelBuilder.Entity<Ator>().HasData(
            new Ator { IdAtores = 1, Nome = "Lucas Andrade", Genero = "Masculino", Idade = 30, PapelSocial1 = "Líder", PapelSocial2 = "Professor", Telefone = 119111111, Extra = "Ativo", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
            new Ator { IdAtores = 2, Nome = "Fernanda Costa", Genero = "Feminino", Idade = 28, PapelSocial1 = "Médica", PapelSocial2 = "Voluntária", Telefone = 219222222, Extra = "Ativo", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
            new Ator { IdAtores = 3, Nome = "Roberto Alves", Genero = "Masculino", Idade = 35, PapelSocial1 = "Advogado", PapelSocial2 = "Consultor", Telefone = 319333333, Extra = "Ativo", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
            new Ator { IdAtores = 4, Nome = "Juliana Prado", Genero = "Feminino", Idade = 26, PapelSocial1 = "Estudante", PapelSocial2 = "Estagiária", Telefone = 419444444, Extra = "Ativo", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
            new Ator { IdAtores = 5, Nome = "Marcelo Nunes", Genero = "Masculino", Idade = 40, PapelSocial1 = "Empresário", PapelSocial2 = "Mentor", Telefone = 519555555, Extra = "Ativo", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now }
            );

            // ===================== Permissoes =====================
            modelBuilder.Entity<Permissao>().HasData(
                new Permissao { IdPermissoes = 1, PerfilAcessoId = 1, PermissaoNome = "Ler" },
                new Permissao { IdPermissoes = 2, PerfilAcessoId = 2, PermissaoNome = "Escrever" },
                new Permissao { IdPermissoes = 3, PerfilAcessoId = 3, PermissaoNome = "Editar" },
                new Permissao { IdPermissoes = 4, PerfilAcessoId = 4, PermissaoNome = "Excluir" },
                new Permissao { IdPermissoes = 5, PerfilAcessoId = 5, PermissaoNome = "Administrador" }
            );

            // ===================== Comunidade =====================
            modelBuilder.Entity<Comunidade>().HasData(
                new Comunidade { IdComunidade = 1, Nome = "Comunidade A", Local = "São Paulo", Status = "Ativa", Complemento = "Zona Leste", Descricao = "Comunidade em SP", DescricaoAcessibilidade = "Acessível", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new Comunidade { IdComunidade = 2, Nome = "Comunidade B", Local = "Rio de Janeiro", Status = "Ativa", Complemento = "Zona Norte", Descricao = "Comunidade no RJ", DescricaoAcessibilidade = "Parcial", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new Comunidade { IdComunidade = 3, Nome = "Comunidade C", Local = "Belo Horizonte", Status = "Inativa", Complemento = "Centro", Descricao = "Comunidade em BH", DescricaoAcessibilidade = "Baixa", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new Comunidade { IdComunidade = 4, Nome = "Comunidade D", Local = "Curitiba", Status = "Ativa", Complemento = "Sul", Descricao = "Comunidade no PR", DescricaoAcessibilidade = "Alta", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new Comunidade { IdComunidade = 5, Nome = "Comunidade E", Local = "Salvador", Status = "Ativa", Complemento = "Norte", Descricao = "Comunidade na BA", DescricaoAcessibilidade = "Média", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now }
            );

            // ===================== Atores =====================
            modelBuilder.Entity<Ator>().HasData(
                new Ator { IdAtores = 1, Nome = "Lucas Andrade", Genero = "Masculino", Idade = 30, PapelSocial1 = "Líder", PapelSocial2 = "Professor", Telefone = 119111111, Extra = "Ativo", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new Ator { IdAtores = 2, Nome = "Fernanda Costa", Genero = "Feminino", Idade = 28, PapelSocial1 = "Médica", PapelSocial2 = "Voluntária", Telefone = 219222222, Extra = "Ativo", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new Ator { IdAtores = 3, Nome = "Roberto Alves", Genero = "Masculino", Idade = 35, PapelSocial1 = "Advogado", PapelSocial2 = "Consultor", Telefone = 319333333, Extra = "Ativo", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new Ator { IdAtores = 4, Nome = "Juliana Prado", Genero = "Feminino", Idade = 26, PapelSocial1 = "Estudante", PapelSocial2 = "Estagiária", Telefone = 419444444, Extra = "Ativo", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new Ator { IdAtores = 5, Nome = "Marcelo Nunes", Genero = "Masculino", Idade = 40, PapelSocial1 = "Empresário", PapelSocial2 = "Mentor", Telefone = 519555555, Extra = "Ativo", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now }
            );

            // ===================== Rede_Recursos =====================
            modelBuilder.Entity<RedeRecurso>().HasData(
                new RedeRecurso { IdRede = 1, AtorId = 1, ComunidadeId = 1, Tipo = "Internet", Dispositivo = "PC", Servicos = "Acesso remoto", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new RedeRecurso { IdRede = 2, AtorId = 2, ComunidadeId = 2, Tipo = "Telefonia", Dispositivo = "Celular", Servicos = "Chamadas", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new RedeRecurso { IdRede = 3, AtorId = 3, ComunidadeId = 3, Tipo = "Energia", Dispositivo = "Gerador", Servicos = "Suporte elétrico", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new RedeRecurso { IdRede = 4, AtorId = 4, ComunidadeId = 4, Tipo = "Água", Dispositivo = "Reservatório", Servicos = "Abastecimento", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new RedeRecurso { IdRede = 5, AtorId = 5, ComunidadeId = 5, Tipo = "Saúde", Dispositivo = "Clínica", Servicos = "Atendimento médico", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now }
            );

            // ===================== Eixo =====================
            modelBuilder.Entity<Eixo>().HasData(
                new Eixo { IdEixo = 1, Nome = "Educação" },
                new Eixo { IdEixo = 2, Nome = "Saúde" },
                new Eixo { IdEixo = 3, Nome = "Segurança" },
                new Eixo { IdEixo = 4, Nome = "Cultura" },
                new Eixo { IdEixo = 5, Nome = "Infraestrutura" }
            );

            // ===================== Rede_Eixo =====================
            modelBuilder.Entity<RedeEixo>().HasData(
                new RedeEixo { IdRedeEixo = 1, RedeId = 1, EixoId = 1 },
                new RedeEixo { IdRedeEixo = 2, RedeId = 2, EixoId = 2 },
                new RedeEixo { IdRedeEixo = 3, RedeId = 3, EixoId = 3 },
                new RedeEixo { IdRedeEixo = 4, RedeId = 4, EixoId = 4 },
                new RedeEixo { IdRedeEixo = 5, RedeId = 5, EixoId = 5 }
            );

            // ===================== Ator_Comunidade =====================
            modelBuilder.Entity<AtorComunidade>().HasData(
                new AtorComunidade { IdAComunidade = 1, ComunidadeId = 1, AtorId = 1 },
                new AtorComunidade { IdAComunidade = 2, ComunidadeId = 2, AtorId = 2 },
                new AtorComunidade { IdAComunidade = 3, ComunidadeId = 3, AtorId = 3 },
                new AtorComunidade { IdAComunidade = 4, ComunidadeId = 4, AtorId = 4 },
                new AtorComunidade { IdAComunidade = 5, ComunidadeId = 5, AtorId = 5 }
            );

            // ===================== Diario_Campo =====================
            modelBuilder.Entity<DiarioCampo>().HasData(
                new DiarioCampo { IdDCampo = 1, ComunidadeId = 1, Data = DateTime.Now, Descricao = "Reunião comunitária", Localizacao = "Praça central", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new DiarioCampo { IdDCampo = 2, ComunidadeId = 2, Data = DateTime.Now, Descricao = "Atividade esportiva", Localizacao = "Quadra", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new DiarioCampo { IdDCampo = 3, ComunidadeId = 3, Data = DateTime.Now, Descricao = "Feira cultural", Localizacao = "Centro comunitário", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new DiarioCampo { IdDCampo = 4, ComunidadeId = 4, Data = DateTime.Now, Descricao = "Ação social", Localizacao = "Escola local", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new DiarioCampo { IdDCampo = 5, ComunidadeId = 5, Data = DateTime.Now, Descricao = "Encontro de líderes", Localizacao = "Associação", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now }
            );
            
                // ===================== Diario_Acoes =====================
            modelBuilder.Entity<DiarioAcoes>().HasData(
                new DiarioAcoes { IdDAcoes = 1, AcoesId = 1, DiarioId = 1 },
                new DiarioAcoes { IdDAcoes = 2, AcoesId = 2, DiarioId = 2 },
                new DiarioAcoes { IdDAcoes = 3, AcoesId = 3, DiarioId = 3 },
                new DiarioAcoes { IdDAcoes = 4, AcoesId = 4, DiarioId = 4 },
                new DiarioAcoes { IdDAcoes = 5, AcoesId = 5, DiarioId = 5 }
            );

            // ===================== Diario_Eixo =====================
            modelBuilder.Entity<DiarioEixo>().HasData(
                new DiarioEixo { IdDiarioEixo = 1, DiarioId = 1, EixoId = 1 },
                new DiarioEixo { IdDiarioEixo = 2, DiarioId = 2, EixoId = 2 },
                new DiarioEixo { IdDiarioEixo = 3, DiarioId = 3, EixoId = 3 },
                new DiarioEixo { IdDiarioEixo = 4, DiarioId = 4, EixoId = 4 },
                new DiarioEixo { IdDiarioEixo = 5, DiarioId = 5, EixoId = 5 }
            );

            // ===================== Atividades =====================
            modelBuilder.Entity<Atividade>().HasData(
                new Atividade { IdAtividade = 1, Nome = "Oficina de Artes", Descricao = "Atividade de pintura e desenho" },
                new Atividade { IdAtividade = 2, Nome = "Aula de Música", Descricao = "Instrumentos e canto" },
                new Atividade { IdAtividade = 3, Nome = "Esporte Comunitário", Descricao = "Futebol, vôlei, basquete" },
                new Atividade { IdAtividade = 4, Nome = "Curso de Tecnologia", Descricao = "Informática básica" },
                new Atividade { IdAtividade = 5, Nome = "Feira Solidária", Descricao = "Troca de produtos" }
            );

            // ===================== Acoes =====================
            modelBuilder.Entity<Acoes>().HasData(
                new Acoes { IdAcoes = 1, Quantidade = 10, AtividadeId = 1 },
                new Acoes { IdAcoes = 2, Quantidade = 15, AtividadeId = 2 },
                new Acoes { IdAcoes = 3, Quantidade = 20, AtividadeId = 3 },
                new Acoes { IdAcoes = 4, Quantidade = 12, AtividadeId = 4 },
                new Acoes { IdAcoes = 5, Quantidade = 25, AtividadeId = 5 }
            );

            // ===================== Acoes_Atores =====================
            modelBuilder.Entity<AcoesAtores>().HasData(
                new AcoesAtores { IdAAtores = 1, AtoresId = 1, AcoesId = 1 },
                new AcoesAtores { IdAAtores = 2, AtoresId = 2, AcoesId = 2 },
                new AcoesAtores { IdAAtores = 3, AtoresId = 3, AcoesId = 3 },
                new AcoesAtores { IdAAtores = 4, AtoresId = 4, AcoesId = 4 },
                new AcoesAtores { IdAAtores = 5, AtoresId = 5, AcoesId = 5 }
            );

            // ===================== Anexos_Diario =====================
            modelBuilder.Entity<AnexosDiario>().HasData(
                new AnexosDiario { IdAnexos = 1, DiarioId = 1, Caminho = "foto1.png" },
                new AnexosDiario { IdAnexos = 2, DiarioId = 2, Caminho = "foto2.png" },
                new AnexosDiario { IdAnexos = 3, DiarioId = 3, Caminho = "documento1.pdf" },
                new AnexosDiario { IdAnexos = 4, DiarioId = 4, Caminho = "audio1.mp3" },
                new AnexosDiario { IdAnexos = 5, DiarioId = 5, Caminho = "video1.mp4" }
            );

            // ===================== Vulnerabilidades =====================
            modelBuilder.Entity<Vulnerabilidade>().HasData(
                new Vulnerabilidade { IdVulnerabilidade = 1, Nome = "Falta de saneamento", Localizacao = "Zona Norte", Servicos = "Nenhum", ComunidadeId = 1 },
                new Vulnerabilidade { IdVulnerabilidade = 2, Nome = "Violência urbana", Localizacao = "Zona Leste", Servicos = "Polícia comunitária", ComunidadeId = 2 },
                new Vulnerabilidade { IdVulnerabilidade = 3, Nome = "Desemprego", Localizacao = "Centro", Servicos = "Cursos profissionalizantes", ComunidadeId = 3 },
                new Vulnerabilidade { IdVulnerabilidade = 4, Nome = "Drogas", Localizacao = "Zona Oeste", Servicos = "Clínica de reabilitação", ComunidadeId = 4 },
                new Vulnerabilidade { IdVulnerabilidade = 5, Nome = "Falta de escolas", Localizacao = "Zona Sul", Servicos = "Projetos de ensino", ComunidadeId = 5 }
            );

            // ===================== Vulnerabilidades_Eixo =====================
            modelBuilder.Entity<VulnerabilidadesEixo>().HasData(
                new VulnerabilidadesEixo { IdVEixo = 1, EixoId = 1, VulnerabilidadeId = 1 },
                new VulnerabilidadesEixo { IdVEixo = 2, EixoId = 2, VulnerabilidadeId = 2 },
                new VulnerabilidadesEixo { IdVEixo = 3, EixoId = 3, VulnerabilidadeId = 3 },
                new VulnerabilidadesEixo { IdVEixo = 4, EixoId = 4, VulnerabilidadeId = 4 },
                new VulnerabilidadesEixo { IdVEixo = 5, EixoId = 5, VulnerabilidadeId = 5 }
            );

            // ===================== Rede_Primaria =====================
            modelBuilder.Entity<RedePrimaria>().HasData(
                new RedePrimaria { IdRedePrimaria = 1, AtorPrincipalId = 1, AtorRelacionadoId = 2, TipoRelacao = "Amizade" },
                new RedePrimaria { IdRedePrimaria = 2, AtorPrincipalId = 2, AtorRelacionadoId = 3, TipoRelacao = "Parceria" },
                new RedePrimaria { IdRedePrimaria = 3, AtorPrincipalId = 3, AtorRelacionadoId = 4, TipoRelacao = "Trabalho" },
                new RedePrimaria { IdRedePrimaria = 4, AtorPrincipalId = 4, AtorRelacionadoId = 5, TipoRelacao = "Estudo" },
                new RedePrimaria { IdRedePrimaria = 5, AtorPrincipalId = 5, AtorRelacionadoId = 1, TipoRelacao = "Mentoria" }
            );

            // ===================== Avaliacao_Pessoal =====================
            modelBuilder.Entity<AvaliacaoPessoal>().HasData(
                new AvaliacaoPessoal { IdAvaliacao = 1, AtorId = 1, CCrimes = 1, Substancias = 0, Moradia = 1, Prevenção = 1, AssBasica = 2, Educacao = 3, Saude = 2, Ocupacao = 1, Lazer = 2, DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new AvaliacaoPessoal { IdAvaliacao = 2, AtorId = 2, CCrimes = 0, Substancias = 1, Moradia = 2, Prevenção = 1, AssBasica = 2, Educacao = 4, Saude = 3, Ocupacao = 2, Lazer = 3, DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new AvaliacaoPessoal { IdAvaliacao = 3, AtorId = 3, CCrimes = 2, Substancias = 1, Moradia = 3, Prevenção = 2, AssBasica = 3, Educacao = 2, Saude = 2, Ocupacao = 3, Lazer = 1, DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new AvaliacaoPessoal { IdAvaliacao = 4, AtorId = 4, CCrimes = 0, Substancias = 0, Moradia = 1, Prevenção = 2, AssBasica = 4, Educacao = 4, Saude = 3, Ocupacao = 2, Lazer = 4, DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new AvaliacaoPessoal { IdAvaliacao = 5, AtorId = 5, CCrimes = 1, Substancias = 2, Moradia = 2, Prevenção = 3, AssBasica = 2, Educacao = 3, Saude = 2, Ocupacao = 3, Lazer = 2, DtCriacao = DateTime.Now, DtModificacao = DateTime.Now }
            );

            // ===================== FonteInf =====================
            modelBuilder.Entity<FonteInf>().HasData(
                new FonteInf { IdFonte = 1 },
                new FonteInf { IdFonte = 2 },
                new FonteInf { IdFonte = 3 },
                new FonteInf { IdFonte = 4 },
                new FonteInf { IdFonte = 5 }
            );

            // ===================== Ficha_Primeiro_Contato =====================
            modelBuilder.Entity<FichaPrimeiroContato>().HasData(
                new FichaPrimeiroContato { IdFicha = 1, AtorId = 1, Localizacao = "Zona Norte", Data = DateTime.Now, LContato = "Vizinho", FonteDados = "Censo", EstaFamiliar = "Sim", EstruFamiliar = "Completa", NFilhos = 2, NFilhas = 1, AEscola = 1, SLer = "Sim", SCalc = "Sim", SComp = "Não", QReabili = 0, LTrat = "Nenhum", Coment = "Boa integração", CPrimeiroContato = "2023-1", EParceiro = "Sim", FPeloParceirto = "Não", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new FichaPrimeiroContato { IdFicha = 2, AtorId = 2, Localizacao = "Zona Sul", Data = DateTime.Now, LContato = "ONG", FonteDados = "Entrevista", EstaFamiliar = "Não", EstruFamiliar = "Parcial", NFilhos = 1, NFilhas = 0, AEscola = 0, SLer = "Sim", SCalc = "Não", SComp = "Não", QReabili = 1, LTrat = "Clínica", Coment = "Necessita apoio", CPrimeiroContato = "2023-2", EParceiro = "Não", FPeloParceirto = "Não", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new FichaPrimeiroContato { IdFicha = 3, AtorId = 3, Localizacao = "Zona Leste", Data = DateTime.Now, LContato = "Assistente social", FonteDados = "Formulário", EstaFamiliar = "Sim", EstruFamiliar = "Completa", NFilhos = 3, NFilhas = 2, AEscola = 1, SLer = "Sim", SCalc = "Sim", SComp = "Sim", QReabili = 0, LTrat = "Nenhum", Coment = "Boa situação", CPrimeiroContato = "2023-3", EParceiro = "Sim", FPeloParceirto = "Sim", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new FichaPrimeiroContato { IdFicha = 4, AtorId = 4, Localizacao = "Zona Oeste", Data = DateTime.Now, LContato = "Igreja", FonteDados = "Entrevista", EstaFamiliar = "Não", EstruFamiliar = "Incompleta", NFilhos = 0, NFilhas = 1, AEscola = 1, SLer = "Não", SCalc = "Não", SComp = "Não", QReabili = 2, LTrat = "Hospital", Coment = "Vulnerável", CPrimeiroContato = "2023-4", EParceiro = "Não", FPeloParceirto = "Não", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now },
                new FichaPrimeiroContato { IdFicha = 5, AtorId = 5, Localizacao = "Centro", Data = DateTime.Now, LContato = "Escola", FonteDados = "Censo escolar", EstaFamiliar = "Sim", EstruFamiliar = "Completa", NFilhos = 2, NFilhas = 2, AEscola = 1, SLer = "Sim", SCalc = "Sim", SComp = "Sim", QReabili = 0, LTrat = "Nenhum", Coment = "Estável", CPrimeiroContato = "2023-5", EParceiro = "Sim", FPeloParceirto = "Não", DtCriacao = DateTime.Now, DtModificacao = DateTime.Now }
            );

            // ===================== Ficha_Condicoes =====================
            modelBuilder.Entity<FichaCondicoes>().HasData(
                new FichaCondicoes { IdCondicoes = 1, FichaId = 1, Cond = "Boa saúde" },
                new FichaCondicoes { IdCondicoes = 2, FichaId = 2, Cond = "Necessita assistência" },
                new FichaCondicoes { IdCondicoes = 3, FichaId = 3, Cond = "Situação estável" },
                new FichaCondicoes { IdCondicoes = 4, FichaId = 4, Cond = "Risco social" },
                new FichaCondicoes { IdCondicoes = 5, FichaId = 5, Cond = "Bem-estar adequado" }
            );

            // ===================== Ficha_Peticoes =====================
            modelBuilder.Entity<FichaPeticoes>().HasData(
                new FichaPeticoes { IdPeticoes = 1, FichaId = 1, Pet = "Solicitação de escola" },
                new FichaPeticoes { IdPeticoes = 2, FichaId = 2, Pet = "Auxílio financeiro" },
                new FichaPeticoes { IdPeticoes = 3, FichaId = 3, Pet = "Atendimento médico" },
                new FichaPeticoes { IdPeticoes = 4, FichaId = 4, Pet = "Programa social" },
                new FichaPeticoes { IdPeticoes = 5, FichaId = 5, Pet = "Cursos de capacitação" }
            );

            // ===================== Ficha_Resp =====================
            modelBuilder.Entity<FichaResp>().HasData(
                new FichaResp { IdCondicoes = 1, FichaId = 1, Resp = "Atendido" },
                new FichaResp { IdCondicoes = 2, FichaId = 2, Resp = "Pendente" },
                new FichaResp { IdCondicoes = 3, FichaId = 3, Resp = "Atendido" },
                new FichaResp { IdCondicoes = 4, FichaId = 4, Resp = "Em análise" },
                new FichaResp { IdCondicoes = 5, FichaId = 5, Resp = "Negado" }
            );

            // ===================== Ficha_Result =====================
            modelBuilder.Entity<FichaResult>().HasData(
                new FichaResult { IdCondicoes = 1, FichaId = 1, Result = "Positivo" },
                new FichaResult { IdCondicoes = 2, FichaId = 2, Result = "Negativo" },
                new FichaResult { IdCondicoes = 3, FichaId = 3, Result = "Positivo" },
                new FichaResult { IdCondicoes = 4, FichaId = 4, Result = "Neutro" },
                new FichaResult { IdCondicoes = 5, FichaId = 5, Result = "Positivo" }
            );

            // ===================== Atividades_Eixo =====================
            modelBuilder.Entity<AtividadesEixo>().HasData(
                new AtividadesEixo { IdAEixo = 1, EixoId = 1, AtividadeId = 1 },
                new AtividadesEixo { IdAEixo = 2, EixoId = 2, AtividadeId = 2 },
                new AtividadesEixo { IdAEixo = 3, EixoId = 3, AtividadeId = 3 },
                new AtividadesEixo { IdAEixo = 4, EixoId = 4, AtividadeId = 4 },
                new AtividadesEixo { IdAEixo = 5, EixoId = 5, AtividadeId = 5 }
            );

            // dotnet ef migrations add Products
            // dotnet ef database update

        }
    }
}
