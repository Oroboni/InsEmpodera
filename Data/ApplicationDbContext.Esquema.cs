using System.Linq.Expressions;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Empodera.Data
{
    /// <summary>
    /// Esquema do banco conforme a revisão técnica do DDL (docs/revisao-ddl-mysql.md):
    /// chaves INT UNSIGNED AUTO_INCREMENT, colunas com o mesmo nome da chave que referenciam
    /// (sem prefixo Fk), VARCHAR com tamanhos justificados (sem TEXT/LONGTEXT), Sim/Não em CHAR(1) ou TINYINT com CHECK,
    /// datas sem frações de segundo, auditoria igual em todas as tabelas e restrições nomeadas
    /// (FK_Filha_Pai, UQ_Tabela_Colunas, CK_Tabela_Coluna).
    /// O script Database/Empodera.schema.sql é gerado a partir deste modelo (scripts/GerarSchema).
    /// </summary>
    public partial class ApplicationDbContext
    {
        /// <summary>Nome das tabelas nas restrições (FK_Acoes_Atividades etc.).</summary>
        internal static readonly IReadOnlyDictionary<string, string> NomesRestricao = new Dictionary<string, string>
        {
            ["acoes"] = "Acoes", ["acoesatores"] = "AcoesAtores", ["anexosdiario"] = "AnexosDiario",
            ["atividades"] = "Atividades", ["atividadeseixo"] = "AtividadesEixo", ["atorcomunidades"] = "AtorComunidades",
            ["atores"] = "Atores", ["avaliacaopessoal"] = "AvaliacaoPessoal", ["comunidades"] = "Comunidades",
            ["daatores"] = "DAAtores", ["detalhesdacoes"] = "DetalhesDAcoes", ["detalheseixos"] = "DetalhesEixos",
            ["diarioacoes"] = "DiarioAcoes", ["diariodacoes"] = "DiarioDAcoes", ["diarioeixos"] = "DiarioEixos",
            ["diarioscampo"] = "DiariosCampo", ["diariosprocessoeixos"] = "DiariosProcessoEixos",
            ["diariosprocessopessoal"] = "DiariosProcessoPessoal", ["eixos"] = "Eixos", ["fichacondicoes"] = "FichaCondicoes",
            ["fichapeticoes"] = "FichaPeticoes", ["ficharespostas"] = "FichaRespostas", ["ficharesultados"] = "FichaResultados",
            ["fichasprimeirocontato"] = "FichasPrimeiroContato", ["fontesinfo"] = "FontesInfo", ["perfis"] = "Perfis",
            ["permissoes"] = "Permissoes", ["recursosatores"] = "RecursosAtores", ["redeeixos"] = "RedeEixos",
            ["rederecursos"] = "RedeRecursos", ["redesprimarias"] = "RedesPrimarias", ["usuarios"] = "Usuarios",
            ["vulnerabilidades"] = "Vulnerabilidades", ["vulnerabilidadeseixo"] = "VulnerabilidadesEixo"
        };

        // Índices só onde há motivo: o MySQL cria sozinho o índice de cada FK (com o nome da restrição),
        // e um UNIQUE que começa pela coluna da FK já serve de índice para ela.
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Conventions.Remove(typeof(ForeignKeyIndexConvention));
        }

        private bool UsaMySql => Database.ProviderName?.Contains("MySql", StringComparison.OrdinalIgnoreCase) == true;

        private void ConfigurarEsquema(ModelBuilder mb)
        {
            // ── usuarios ── Senha guarda o hash do Identity (base64, 84 caracteres): VARCHAR(255) em ASCII binário,
            // como recomenda a revisão (5.4) — hash não precisa de Unicode nem de comparação sem distinção de maiúsculas.
            mb.Entity<Usuario>(e =>
            {
                Chave(e, u => u.IdUsuario, "IdUsuario");
                Texto(e, u => u.Nome, "Nome", 150);
                Texto(e, u => u.Senha, "Senha", 255);
                if (UsaMySql) e.Property(u => u.Senha).HasCharSet("ascii").UseCollation("ascii_bin");
                Texto(e, u => u.Foto, "Foto", 255);
                Texto(e, u => u.Email, "Email", 254);
                Texto(e, u => u.Ocupacao, "Ocupacao", 100);
                Pequeno(e, u => u.Genero, "Genero", "CK_Usuarios_Genero", "Genero BETWEEN 0 AND 3");
                Data(e, u => u.DtNascimento, "DtNascimento");
                Criacao(e, u => u.DtCriacao);
                Modificacao(e, u => u.DtAtualizacao);
                SimNao(e, u => u.Ativo, "Ativo", "CK_Usuarios_Ativo");
                e.Property(u => u.IdiomaPreferido).HasColumnName("IdiomaPreferido").HasDefaultValue(IdiomaPreferido.Default).HasSentinel((IdiomaPreferido)(-1));
                if (UsaMySql) e.Property(u => u.IdiomaPreferido).HasColumnType("tinyint unsigned");
                Check(e, "CK_Usuarios_IdiomaPreferido", "IdiomaPreferido BETWEEN 0 AND 3");
                Referencia(e, u => u.FkIdPerfil, "IdPerfil");
                Texto(e, u => u.UserName, "UserName", 254);
                Texto(e, u => u.NormalizedUserName, "NormalizedUserName", 254);
                Texto(e, u => u.NormalizedEmail, "NormalizedEmail", 254);
                Booleano(e, u => u.EmailConfirmed, "EmailConfirmed", "CK_Usuarios_EmailConfirmed");
                Texto(e, u => u.SecurityStamp, "SecurityStamp", 100);
                Texto(e, u => u.ConcurrencyStamp, "ConcurrencyStamp", 100);
                Texto(e, u => u.PhoneNumber, "PhoneNumber", 60);
                Booleano(e, u => u.PhoneNumberConfirmed, "PhoneNumberConfirmed", "CK_Usuarios_PhoneNumberConfirmed");
                Booleano(e, u => u.TwoFactorEnabled, "TwoFactorEnabled", "CK_Usuarios_TwoFactorEnabled");
                DataHora(e, u => u.LockoutEnd, "LockoutEnd");
                Booleano(e, u => u.LockoutEnabled, "LockoutEnabled", "CK_Usuarios_LockoutEnabled");
                Contador(e, u => u.AccessFailedCount, "AccessFailedCount");
                Unico(e, "UQ_Usuarios_NormalizedEmail", u => u.NormalizedEmail);
                Unico(e, "UQ_Usuarios_NormalizedUserName", u => u.NormalizedUserName);
            });

            // ── perfis ── (IdUsuario = quem criou o perfil; perfis antigos podem não ter essa informação)
            mb.Entity<Perfil>(e =>
            {
                Chave(e, p => p.IdPerfil, "IdPerfil");
                Referencia(e, p => p.FkIdUsuario, "IdUsuario");
                Texto(e, p => p.Nome, "Nome", 100);
                Criacao(e, p => p.DtCriacao);
                Modificacao(e, p => p.DtModificacao);
                e.HasOne<Usuario>().WithMany().HasForeignKey(p => p.FkIdUsuario).OnDelete(DeleteBehavior.SetNull);
            });

            mb.Entity<Permissoes>(e =>
            {
                Chave(e, p => p.IdPermissoes, "IdPermissao");
                Referencia(e, p => p.FkIdPerfil, "IdPerfil");
                Texto(e, p => p.Modulo, "Modulo", 50);
                SimNao(e, p => p.PodeListar, "PodeListar", "CK_Permissoes_PodeListar");
                SimNao(e, p => p.PodeDetalhar, "PodeDetalhar", "CK_Permissoes_PodeDetalhar");
                SimNao(e, p => p.PodeCriar, "PodeCriar", "CK_Permissoes_PodeCriar");
                SimNao(e, p => p.PodeAtualizar, "PodeAtualizar", "CK_Permissoes_PodeAtualizar");
                SimNao(e, p => p.PodeDeletar, "PodeDeletar", "CK_Permissoes_PodeDeletar");
                Auditoria(e);
                Unico(e, "UQ_Permissoes_Perfil_Modulo", p => new { p.FkIdPerfil, p.Modulo });
            });

            mb.Entity<Comunidade>(e =>
            {
                Chave(e, c => c.Id_Comunidade, "IdComunidade");
                Texto(e, c => c.Nome, "Nome", 150);
                Texto(e, c => c.Local, "Local", 500);
                Texto(e, c => c.LocalMapa, "LocalMapa", 500);
                Texto(e, c => c.LocalSecundario, "LocalSecundario", 500);
                Texto(e, c => c.LocalMapaSecundario, "LocalMapaSecundario", 500);
                Texto(e, c => c.Status, "Status", 30);
                Texto(e, c => c.Complemento, "Complemento", 255);
                Texto(e, c => c.Descricao, "Descricao", 5000);            // maior texto real: 3.433
                Texto(e, c => c.Descricao_Acessibilidade, "DescricaoAcessibilidade", 2000);
                Criacao(e, c => c.Dt_Criacao);
                Modificacao(e, c => c.Dt_Modificacao);
                Referencia(e, c => c.FK_Id_Usuario, "IdUsuario");
                Referencia(e, c => c.FK_Id_UsuarioM, "IdUsuarioModificacao");
                SimNao(e, c => c.Ativo, "Ativo", "CK_Comunidades_Ativo");
            });

            mb.Entity<Atores>(e =>
            {
                Chave(e, a => a.IdAtores, "IdAtor");
                Texto(e, a => a.Nome, "Nome", 150);
                Pequeno(e, a => a.Genero, "Genero", "CK_Atores_Genero", "Genero BETWEEN 0 AND 3");
                Pequeno(e, a => a.Idade, "Idade", "CK_Atores_Idade", "Idade <= 130");
                Texto(e, a => a.PapelSocial1, "PapelSocial1", 255);
                Texto(e, a => a.PapelSocial2, "PapelSocial2", 255);
                Texto(e, a => a.Interesse, "Interesse", 255);
                Texto(e, a => a.Telefone, "Telefone", 60);
                Booleano(e, a => a.DaEquipe, "DaEquipe", "CK_Atores_DaEquipe");
                Booleano(e, a => a.Rope, "Rope", "CK_Atores_Rope");
                Booleano(e, a => a.Lopiniao, "Lopiniao", "CK_Atores_Lopiniao");
                Booleano(e, a => a.Mcomunidade, "Mcomunidade", "CK_Atores_Mcomunidade");
                Criacao(e, a => a.DtCriacao);
                Modificacao(e, a => a.DtModificacao);
                SimNao(e, a => a.Ativo, "Ativo", "CK_Atores_Ativo");
                Referencia(e, a => a.FkIdUsuario, "IdUsuario");
                Referencia(e, a => a.FkIdUsuarioM, "IdUsuarioModificacao");
            });

            mb.Entity<RedeRecursos>(e =>
            {
                Chave(e, r => r.Id_Rede, "IdRede");
                Referencia(e, r => r.FK_id_Atores, "IdAtor");
                Referencia(e, r => r.FkIdComunidade, "IdComunidade");
                Texto(e, r => r.Nome, "Nome", 150);
                Texto(e, r => r.Tipo, "Tipo", 50);
                Texto(e, r => r.Dispositivo, "Dispositivo", 150);
                Texto(e, r => r.Servicos, "Servicos", 2000);
                Texto(e, r => r.Localizacao, "Localizacao", 255);
                Criacao(e, r => r.DtCriacao);
                Modificacao(e, r => r.DtModificacao);
                Referencia(e, r => r.FkIdUsuario, "IdUsuario");
            });

            mb.Entity<Eixo>(e =>
            {
                Chave(e, x => x.IdEixo, "IdEixo");
                Texto(e, x => x.Nome, "Nome", 100);
                Auditoria(e);
                Unico(e, "UQ_Eixos_Nome", x => x.Nome);
            });

            Associativa<RedeEixo>(mb, "IdRedeEixo", x => x.IdRedeEixo,
                (x => x.FkIdRede, "IdRede"), (x => x.FkIdEixo, "IdEixo"), "UQ_RedeEixos_Rede_Eixo", x => new { x.FkIdRede, x.FkIdEixo });

            mb.Entity<AtorComunidade>(e =>
            {
                Chave(e, x => x.IdAtorComunidade, "IdAtorComunidade");
                Referencia(e, x => x.FK_id_Atores, "IdAtor");
                Referencia(e, x => x.FkIdComunidade, "IdComunidade");
                Pequeno(e, x => x.TipoRelacionamento, "TipoRelacionamento", "CK_AtorComunidades_TipoRelacionamento", "TipoRelacionamento BETWEEN 1 AND 5");
                Auditoria(e);
            });

            mb.Entity<DiarioCampo>(e =>
            {
                Chave(e, d => d.IdDCampo, "IdDiarioCampo");
                Referencia(e, d => d.FkIdComunidade, "IdComunidade");
                Data(e, d => d.Data, "Data");
                Texto(e, d => d.Descricao, "Descricao", 8000);            // maior texto real: 5.299
                Texto(e, d => d.Localizacao, "Localizacao", 255);
                Texto(e, d => d.Foto, "Foto", 255);
                Criacao(e, d => d.DtCriacao);
                Modificacao(e, d => d.DtModificacao);
                Referencia(e, d => d.FkIdUsuario, "IdUsuario");
            });

            Associativa<DiarioAcoes>(mb, "IdDiarioAcao", x => x.IdDAcoes,
                (x => x.FkIdDiario, "IdDiarioCampo"), (x => x.FkIdAcoes, "IdAcao"), "UQ_DiarioAcoes_Diario_Acao", x => new { x.FkIdDiario, x.FkIdAcoes });

            mb.Entity<DiarioDAcoes>(e =>
            {
                Chave(e, x => x.IdDAcoes, "IdDiarioDAcao");
                Referencia(e, x => x.FkIdDiario, "IdDiarioCampo");
                Texto(e, x => x.Nome, "Nome", 150);
                Texto(e, x => x.PeovedorEx, "ProvedorExterno", 150);
                Quantidade(e, x => x.Quantidade, "CK_DiarioDAcoes_Quantidade");
                Auditoria(e);
            });

            mb.Entity<DetalhesDAcoes>(e =>
            {
                Chave(e, x => x.Id, "IdDetalheDAcao");
                Referencia(e, x => x.FkIdDDacoes, "IdDiarioDAcao");
                Texto(e, x => x.Nome, "Nome", 150);
                Auditoria(e);
            });

            Associativa<DetalhesEixos>(mb, "IdDetalheEixo", x => x.IdDiarioEixo,
                (x => x.FkIdDetalhes, "IdDetalheDAcao"), (x => x.FkIdEixo, "IdEixo"), "UQ_DetalhesEixos_Detalhe_Eixo", x => new { x.FkIdDetalhes, x.FkIdEixo });
            Associativa<DAAtores>(mb, "IdDAAtor", x => x.Id,
                (x => x.FkIdDDacoes, "IdDiarioDAcao"), (x => x.FK_id_Atores, "IdAtor"), "UQ_DAAtores_DiarioDAcao_Ator", x => new { x.FkIdDDacoes, x.FK_id_Atores });
            Associativa<DiarioEixo>(mb, "IdDiarioEixo", x => x.IdDiarioEixo,
                (x => x.FkIdDiario, "IdDiarioCampo"), (x => x.FkIdEixo, "IdEixo"), "UQ_DiarioEixos_Diario_Eixo", x => new { x.FkIdDiario, x.FkIdEixo });

            mb.Entity<Acoes>(e =>
            {
                Chave(e, x => x.IdAcoes, "IdAcao");
                Referencia(e, x => x.FkIdAtividade, "IdAtividade");
                Texto(e, x => x.Nome, "Nome", 150);
                Texto(e, x => x.Provedor, "Provedor", 150);
                Quantidade(e, x => x.Quantidade, "CK_Acoes_Quantidade");
                Auditoria(e);
            });

            Associativa<AcoesAtores>(mb, "IdAcaoAtor", x => x.IdAAtores,
                (x => x.FkIdAcoes, "IdAcao"), (x => x.FK_id_Atores, "IdAtor"), "UQ_AcoesAtores_Acao_Ator", x => new { x.FkIdAcoes, x.FK_id_Atores });

            mb.Entity<AnexosDiario>(e =>
            {
                Chave(e, x => x.IdAnexos, "IdAnexo");
                Referencia(e, x => x.FkIdDiario, "IdDiarioCampo");
                Texto(e, x => x.Caminho, "Caminho", 500);
                Auditoria(e);
            });

            mb.Entity<Vulnerabilidade>(e =>
            {
                Chave(e, x => x.IdVulnerabilidade, "IdVulnerabilidade");
                Referencia(e, x => x.FkIdComunidade, "IdComunidade");
                Texto(e, x => x.Nome, "Nome", 150);
                Texto(e, x => x.Localizacao, "Localizacao", 255);
                Texto(e, x => x.Servicos, "Servicos", 2000);
                Auditoria(e);
            });

            Associativa<VulnerabilidadesEixo>(mb, "IdVulnerabilidadeEixo", x => x.IdVEixo,
                (x => x.FkIdVulnerabilidade, "IdVulnerabilidade"), (x => x.FkIdEixo, "IdEixo"), "UQ_VulnerabilidadesEixo_Vulnerabilidade_Eixo", x => new { x.FkIdVulnerabilidade, x.FkIdEixo });

            mb.Entity<RedePrimaria>(e =>
            {
                Chave(e, x => x.IdRedePrimaria, "IdRedePrimaria");
                Referencia(e, x => x.FkIdAtorPrincipal, "IdAtorPrincipal");
                Referencia(e, x => x.FkIdAtorRelacionados, "IdAtorRelacionado");
                Texto(e, x => x.TipoRelacao, "TipoRelacao", 100);
                e.Property(x => x.TipoRede).HasColumnName("TipoRede");
                Check(e, "CK_RedesPrimarias_TipoRede", "TipoRede IN ('Primaria', 'Subjetiva')");
                Check(e, "CK_RedesPrimarias_AtoresDistintos", "IdAtorPrincipal <> IdAtorRelacionado");
                Auditoria(e);
                Unico(e, "UQ_RedesPrimarias_Principal_Relacionado_Tipo", x => new { x.FkIdAtorPrincipal, x.FkIdAtorRelacionados, x.TipoRede });
            });

            mb.Entity<AvaliacaoPessoal>(e =>
            {
                Chave(e, x => x.IdAvaliacao, "IdAvaliacao");
                Referencia(e, x => x.FK_id_Atores, "IdAtor");
                Nota(e, x => x.CCrimes, "CCrimes");
                Nota(e, x => x.Substancias, "Substancias");
                Nota(e, x => x.Moradia, "Moradia");
                Nota(e, x => x.Prevencao, "Prevencao");
                Nota(e, x => x.AssBasica, "AssBasica");
                Nota(e, x => x.Educacao, "Educacao");
                Nota(e, x => x.Saude, "Saude");
                Nota(e, x => x.Ocupacao, "Ocupacao");
                Nota(e, x => x.Lazer, "Lazer");
                Criacao(e, x => x.DtCriacao);
                Modificacao(e, x => x.DtModificacao);
                Referencia(e, x => x.FkIdUsuario, "IdUsuario");
            });

            mb.Entity<FichaPrimeiroContato>(e =>
            {
                Chave(e, f => f.IdFicha, "IdFicha");
                Referencia(e, f => f.FK_id_Atores, "IdAtor");
                Texto(e, f => f.Endereco, "Endereco", 255);
                Texto(e, f => f.Complemento, "Complemento", 255);
                Texto(e, f => f.Emprego, "Emprego", 150);
                Texto(e, f => f.CEstabeleceu, "CEstabeleceu", 100);
                SimNaoTexto(e, f => f.NovoParceiro, "NovoParceiro", "CK_FichasPrimeiroContato_NovoParceiro");
                SimNaoTexto(e, f => f.FornecidoParceiro, "FornecidoParceiro", "CK_FichasPrimeiroContato_FornecidoParceiro");
                Texto(e, f => f.Telefone, "Telefone", 60);
                Texto(e, f => f.LContato, "LContato", 150);
                Texto(e, f => f.FonteDados, "FonteDados", 150);
                Texto(e, f => f.EstaFamiliar, "EstaFamiliar", 100);
                Texto(e, f => f.EstruFamiliar, "EstruFamiliar", 255);
                Pequeno(e, f => f.NFIlhos, "NFilhos", "CK_FichasPrimeiroContato_NFilhos", "NFilhos <= 30");
                Pequeno(e, f => f.NFilhas, "NFilhas", "CK_FichasPrimeiroContato_NFilhas", "NFilhas <= 30");
                Pequeno(e, f => f.AEscolar, "AEscolar", "CK_FichasPrimeiroContato_AEscolar", "AEscolar <= 30");
                Texto(e, f => f.Status, "Status", 20);
                Check(e, "CK_FichasPrimeiroContato_Status", "Status IN ('EmProgresso', 'Concluida', 'Abandonada')");
                SimNaoTexto(e, f => f.SLer, "SLer", "CK_FichasPrimeiroContato_SLer");
                SimNaoTexto(e, f => f.SCalc, "SCalc", "CK_FichasPrimeiroContato_SCalc");
                SimNaoTexto(e, f => f.SComp, "SComp", "CK_FichasPrimeiroContato_SComp");
                Pequeno(e, f => f.QReabili, "QReabili", "CK_FichasPrimeiroContato_QReabili", "QReabili <= 100");
                Texto(e, f => f.LTrat, "LTrat", 150);
                Texto(e, f => f.Coment, "Coment", 2000);
                Data(e, f => f.DtContato, "DtContato");
                e.Property(f => f.HoraContato).HasColumnName("HoraContato");
                if (UsaMySql) e.Property(f => f.HoraContato).HasColumnType("time");
                Criacao(e, f => f.DtCriacao);
                Modificacao(e, f => f.DtModificacao);
                Referencia(e, f => f.FkIdComunidade, "IdComunidade");
                Referencia(e, f => f.FkIdUsuario, "IdUsuario");
            });

            mb.Entity<FonteInf>(e =>
            {
                Chave(e, x => x.IdFonte, "IdFonte");
                Referencia(e, x => x.FkIdFicha, "IdFicha");
                Texto(e, x => x.Nome, "Nome", 150);
                Pequeno(e, x => x.Genero, "Genero", "CK_FontesInfo_Genero", "Genero BETWEEN 0 AND 3");
                Pequeno(e, x => x.Idade, "Idade", "CK_FontesInfo_Idade", "Idade <= 130");
                Texto(e, x => x.PapelSocial1, "PapelSocial1", 255);
                Texto(e, x => x.PapelSocial2, "PapelSocial2", 255);
                Texto(e, x => x.Interesse, "Interesse", 255);
                Texto(e, x => x.Telefone, "Telefone", 60);
                Texto(e, x => x.Extra, "Extra", 255);
                Referencia(e, x => x.Fk_Id_Ator, "IdAtor");
                e.HasOne(x => x.Ator).WithMany(a => a.FonteInfos).HasForeignKey(x => x.Fk_Id_Ator).OnDelete(DeleteBehavior.SetNull);
                Auditoria(e);
            });

            FilhaDaFicha<FichaCondicoes>(mb, "IdFichaCondicao", x => x.IdCondicoes, x => x.FkIdFicha, x => x.Cond, "Cond");
            FilhaDaFicha<FichaPeticoes>(mb, "IdFichaPeticao", x => x.IdPeticoes, x => x.FkIdFicha, x => x.Pet, "Pet");
            FilhaDaFicha<FichaResp>(mb, "IdFichaResposta", x => x.IdCondicoes, x => x.FkIdFicha, x => x.Resp, "Resp");
            FilhaDaFicha<FichaResult>(mb, "IdFichaResultado", x => x.IdCondicoes, x => x.FkIdFicha, x => x.Result, "Result");

            mb.Entity<Atividades>(e =>
            {
                Chave(e, x => x.IdAtividade, "IdAtividade");
                Texto(e, x => x.Nome, "Nome", 150);
                Texto(e, x => x.Descricao, "Descricao", 2000);
                Texto(e, x => x.Foto, "Foto", 255);
                Referencia(e, x => x.FkIdComunidade, "IdComunidade");
                Criacao(e, x => x.DtCriacao);
                Modificacao(e, x => x.DtModificacao);
                Referencia(e, x => x.FkIdUsuario, "IdUsuario");
                Referencia(e, x => x.FkIdUsuarioM, "IdUsuarioModificacao");
            });

            Associativa<AtividadesEixo>(mb, "IdAtividadeEixo", x => x.IdAEixo,
                (x => x.FkIdAtividade, "IdAtividade"), (x => x.FkIdEixo, "IdEixo"), "UQ_AtividadesEixo_Atividade_Eixo", x => new { x.FkIdAtividade, x.FkIdEixo });

            mb.Entity<RecursosAtores>(e =>
            {
                Chave(e, x => x.Id_Recursos_Atores, "IdRecursoAtor");
                Referencia(e, x => x.FK_id_Atores, "IdAtor");
                Texto(e, x => x.Nome, "Nome", 150);
                Texto(e, x => x.Tipo, "Tipo", 50);
                SimNao(e, x => x.Pode, "Pode", "CK_RecursosAtores_Pode");
                Auditoria(e);
            });

            mb.Entity<DiarioProcessoPessoal>(e =>
            {
                Chave(e, d => d.IdDiarioProcesso, "IdDiarioProcesso");
                Referencia(e, d => d.FK_id_Atores, "IdAtor");
                Data(e, d => d.Data, "Data");
                e.Property(d => d.Descricao).HasColumnName("Descricao").HasMaxLength(4000);
                Criacao(e, d => d.DtCriacao);
                Modificacao(e, d => d.DtModificacao);
                Referencia(e, d => d.FkIdUsuario, "IdUsuario");
                Referencia(e, d => d.FkIdUsuarioM, "IdUsuarioModificacao");
            });

            Associativa<DiarioProcessoEixo>(mb, "IdDiarioProcessoEixo", x => x.IdDiarioProcessoEixo,
                (x => x.FkIdDiarioProcesso, "IdDiarioProcesso"), (x => x.FkIdEixo, "IdEixo"), "UQ_DiariosProcessoEixos_Diario_Eixo", x => new { x.FkIdDiarioProcesso, x.FkIdEixo });

            NomearChavesEstrangeiras(mb);
        }

        // FK_<Filha>_<Pai>; quando a filha aponta duas vezes para o mesmo pai, o papel vai no fim
        // (FK_Atores_Usuarios_Modificacao, FK_RedesPrimarias_Atores_Relacionado).
        private static void NomearChavesEstrangeiras(ModelBuilder mb)
        {
            foreach (var entidade in mb.Model.GetEntityTypes())
            {
                var filha = entidade.GetTableName()!;
                var porPai = entidade.GetForeignKeys().GroupBy(fk => fk.PrincipalEntityType.GetTableName()!);
                foreach (var grupo in porPai)
                {
                    foreach (var fk in grupo)
                    {
                        var nome = $"FK_{NomesRestricao[filha]}_{NomesRestricao[grupo.Key]}";
                        var coluna = fk.Properties[0].GetColumnName(StoreObjectIdentifier.Table(filha, null))!;
                        var colunaPai = fk.PrincipalKey.Properties[0].GetColumnName(StoreObjectIdentifier.Table(grupo.Key, null))!;
                        if (grupo.Count() > 1 && coluna != colunaPai)
                            nome += "_" + coluna[colunaPai.Length..].TrimStart('_');
                        fk.SetConstraintName(nome);
                    }
                }
            }
        }

        // ── Auxiliares ──────────────────────────────────────────────────────

        private void Chave<T>(EntityTypeBuilder<T> e, Expression<Func<T, int>> prop, string coluna) where T : class
        {
            var p = e.Property(prop).HasColumnName(coluna).ValueGeneratedOnAdd();
            if (UsaMySql) p.HasColumnType("int unsigned");
        }

        private void Referencia<T>(EntityTypeBuilder<T> e, Expression<Func<T, int>> prop, string coluna) where T : class
        {
            var p = e.Property(prop).HasColumnName(coluna);
            if (UsaMySql) p.HasColumnType("int unsigned");
        }

        private void Referencia<T>(EntityTypeBuilder<T> e, Expression<Func<T, int?>> prop, string coluna) where T : class
        {
            var p = e.Property(prop).HasColumnName(coluna);
            if (UsaMySql) p.HasColumnType("int unsigned");
        }

        private static void Texto<T>(EntityTypeBuilder<T> e, Expression<Func<T, string?>> prop, string coluna, int tamanho) where T : class =>
            e.Property(prop).HasColumnName(coluna).HasMaxLength(tamanho);

        // 'S' ou 'N' (já é o que a aplicação grava). No MySQL a comparação do CHECK é binária: com o
        // collation utf8mb4_0900_ai_ci, 's' e 'n' minúsculos também passariam.
        private void SimNao<T>(EntityTypeBuilder<T> e, Expression<Func<T, string?>> prop, string coluna, string check) where T : class
        {
            e.Property(prop).HasColumnName(coluna).HasMaxLength(1).IsFixedLength();
            Check(e, check, RegraSimNao(coluna));
        }

        private string RegraSimNao(string coluna) =>
            UsaMySql ? $"{coluna} COLLATE utf8mb4_bin IN ('S', 'N')" : $"{coluna} IN ('S', 'N')";

        // A aplicação usa "Sim"/"Não"; o banco guarda 'S'/'N'.
        private static readonly ValueConverter<string?, string?> SimNaoConversor = new(
            v => v == "Sim" ? "S" : v == "Não" ? "N" : string.IsNullOrEmpty(v) ? null : v,
            v => v == "S" ? "Sim" : v == "N" ? "Não" : v);

        private void SimNaoTexto<T>(EntityTypeBuilder<T> e, Expression<Func<T, string?>> prop, string coluna, string check) where T : class
        {
            e.Property(prop).HasColumnName(coluna).HasMaxLength(1).IsFixedLength().HasConversion(SimNaoConversor);
            Check(e, check, RegraSimNao(coluna));
        }

        private void Booleano<T>(EntityTypeBuilder<T> e, Expression<Func<T, bool>> prop, string coluna, string check) where T : class
        {
            var p = e.Property(prop).HasColumnName(coluna);
            if (UsaMySql) p.HasColumnType("tinyint unsigned").HasConversion(new BoolToZeroOneConverter<byte>());
            Check(e, check, $"{coluna} IN (0, 1)");
        }

        private void Pequeno<T>(EntityTypeBuilder<T> e, Expression<Func<T, int?>> prop, string coluna, string check, string regra) where T : class
        {
            var p = e.Property(prop).HasColumnName(coluna);
            if (UsaMySql) p.HasColumnType("tinyint unsigned");
            Check(e, check, regra);
        }

        private void Nota<T>(EntityTypeBuilder<T> e, Expression<Func<T, int>> prop, string coluna) where T : class
        {
            var p = e.Property(prop).HasColumnName(coluna);
            if (UsaMySql) p.HasColumnType("tinyint unsigned");
            Check(e, $"CK_{NomesRestricao[e.Metadata.GetTableName()!]}_{coluna}", $"{coluna} BETWEEN 1 AND 5");
        }

        private static void Quantidade<T>(EntityTypeBuilder<T> e, Expression<Func<T, int>> prop, string check) where T : class
        {
            e.Property(prop).HasColumnName("Quantidade");
            Check(e, check, "Quantidade >= 0");
        }

        private void Contador<T>(EntityTypeBuilder<T> e, Expression<Func<T, int>> prop, string coluna) where T : class
        {
            var p = e.Property(prop).HasColumnName(coluna);
            if (UsaMySql) p.HasColumnType("int unsigned");
        }

        private void Data<T>(EntityTypeBuilder<T> e, Expression<Func<T, DateTime>> prop, string coluna) where T : class
        {
            var p = e.Property(prop).HasColumnName(coluna);
            if (UsaMySql) p.HasColumnType("date");
        }

        private void DataHora<T>(EntityTypeBuilder<T> e, Expression<Func<T, DateTimeOffset?>> prop, string coluna) where T : class
        {
            var p = e.Property(prop).HasColumnName(coluna);
            if (UsaMySql) p.HasColumnType("datetime");
        }

        // Auditoria igual nas 34 tabelas: DtCriacao NOT NULL DEFAULT CURRENT_TIMESTAMP e
        // DtModificacao NULL ON UPDATE CURRENT_TIMESTAMP (o ON UPDATE é escrito pelo gerador do script).
        private void Criacao<T>(EntityTypeBuilder<T> e, Expression<Func<T, DateTime>> prop) where T : class
        {
            var p = e.Property(prop).HasColumnName("DtCriacao").HasDefaultValueSql("CURRENT_TIMESTAMP");
            if (UsaMySql) p.HasColumnType("datetime");
        }

        private void Modificacao<T>(EntityTypeBuilder<T> e, Expression<Func<T, DateTime?>> prop) where T : class
        {
            var p = e.Property(prop).HasColumnName("DtModificacao");
            if (UsaMySql) p.HasColumnType("datetime");
        }

        // Tabelas cujas classes não têm as datas: colunas só no banco (preenchidas por DEFAULT/ON UPDATE).
        private void Auditoria<T>(EntityTypeBuilder<T> e) where T : class
        {
            var criacao = e.Property<DateTime>("DtCriacao").HasColumnName("DtCriacao")
                .HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
            var modificacao = e.Property<DateTime?>("DtModificacao").HasColumnName("DtModificacao");
            if (UsaMySql)
            {
                criacao.HasColumnType("datetime");
                modificacao.HasColumnType("datetime");
            }
        }

        private static void Check<T>(EntityTypeBuilder<T> e, string nome, string regra) where T : class =>
            e.ToTable(t => t.HasCheckConstraint(nome, regra));

        private static void Unico<T>(EntityTypeBuilder<T> e, string nome, Expression<Func<T, object?>> colunas) where T : class =>
            e.HasIndex(colunas).IsUnique().HasDatabaseName(nome);

        private void Associativa<T>(
            ModelBuilder mb, string colunaChave, Expression<Func<T, int>> chave,
            (Expression<Func<T, int>> Prop, string Coluna) primeira,
            (Expression<Func<T, int>> Prop, string Coluna) segunda,
            string unico, Expression<Func<T, object?>> colunasUnicas) where T : class
        {
            mb.Entity<T>(e =>
            {
                Chave(e, chave, colunaChave);
                Referencia(e, primeira.Prop, primeira.Coluna);
                Referencia(e, segunda.Prop, segunda.Coluna);
                Auditoria(e);
                Unico(e, unico, colunasUnicas);
            });
        }

        private void FilhaDaFicha<T>(
            ModelBuilder mb, string colunaChave, Expression<Func<T, int>> chave,
            Expression<Func<T, int>> ficha, Expression<Func<T, string>> texto, string colunaTexto) where T : class
        {
            mb.Entity<T>(e =>
            {
                Chave(e, chave, colunaChave);
                Referencia(e, ficha, "IdFicha");
                Texto(e, texto!, colunaTexto, 150);
                Auditoria(e);
            });
        }
    }
}
