using System;
using System.Collections.Generic;

namespace Projeto.Data
{
    public class UsuarioSeed
    {
        public int IdUsuario { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string Foto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Ocupacao { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public DateTime DtNascimento { get; set; }
        public int NivelPermissao { get; set; }
        public DateTime DtCriacao { get; set; }

        public List<UsuarioPerfilSeed>? Perfis { get; set; }
    }

    public class UsuarioPerfilSeed
    {
        public int IdPUsuario { get; set; }
        public int UsuarioId { get; set; }
        public int PerfilAcessoId { get; set; }

        public PerfilAcessoSeed? Perfil { get; set; }
    }

    public class PerfilAcessoSeed
    {
        public int IdPAcesso { get; set; }
        public List<PermissaoSeed>? Permissoes { get; set; }
    }

    public class PermissaoSeed
    {
        public int IdPermissao { get; set; }
        public int PerfilAcessoId { get; set; }
        public string Permissao { get; set; } = string.Empty;
    }

    public class ComunidadeSeed
    {
        public int IdComunidade { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Local { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Complemento { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string DescricaoAcessibilidade { get; set; } = string.Empty;
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }

        public List<RedeRecursoSeed>? Redes { get; set; }
        public List<AtorComunidadeSeed>? Atores { get; set; }
        public List<DiarioCampoSeed>? Diarios { get; set; }
    }

    public class AtorSeed
    {
        public int IdAtor { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public int Idade { get; set; }
        public string PapelSocial1 { get; set; } = string.Empty;
        public string PapelSocial2 { get; set; } = string.Empty;
        public int Telefone { get; set; }
        public string Extra { get; set; } = string.Empty;
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }

        public List<RedeRecursoSeed>? Redes { get; set; }
        public List<AtorComunidadeSeed>? Comunidades { get; set; }
        public List<AcoesAtorSeed>? Acoes { get; set; }
    }

    public class RedeRecursoSeed
    {
        public int IdRede { get; set; }
        public int AtorId { get; set; }
        public int ComunidadeId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Dispositivo { get; set; } = string.Empty;
        public string Servicos { get; set; } = string.Empty;
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }

        public List<RedeEixoSeed>? Eixos { get; set; }
    }

    public class EixoSeed
    {
        public int IdEixo { get; set; }
        public string Nome { get; set; } = string.Empty;
    }

    public class RedeEixoSeed
    {
        public int IdRedeEixo { get; set; }
        public int RedeId { get; set; }
        public int EixoId { get; set; }
    }

    public class AtorComunidadeSeed
    {
        public int IdAComunidade { get; set; }
        public int ComunidadeId { get; set; }
        public int AtorId { get; set; }
    }

    public class DiarioCampoSeed
    {
        public int IdDCampo { get; set; }
        public int ComunidadeId { get; set; }
        public DateTime Data { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string Localizacao { get; set; } = string.Empty;
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }

        public List<DiarioAcaoSeed>? Acoes { get; set; }
        public List<DiarioEixoSeed>? Eixos { get; set; }
        public List<AnexoDiarioSeed>? Anexos { get; set; }
    }

    public class DiarioAcaoSeed
    {
        public int IdDAcoes { get; set; }
        public int AcoesId { get; set; }
        public int DiarioId { get; set; }
    }

    public class DiarioEixoSeed
    {
        public int IdDiarioEixo { get; set; }
        public int DiarioId { get; set; }
        public int EixoId { get; set; }
    }

    public class AcaoSeed
    {
        public int IdAcoes { get; set; }
        public int Quantidade { get; set; }
        public int AtividadeId { get; set; }

        public List<AcoesAtorSeed>? Atores { get; set; }
    }

    public class AcoesAtorSeed
    {
        public int IdAAtores { get; set; }
        public int AtorId { get; set; }
        public int AcoesId { get; set; }
    }

    public class AnexoDiarioSeed
    {
        public int IdAnexos { get; set; }
        public int DiarioId { get; set; }
        public string Caminho { get; set; } = string.Empty;
    }

    public class VulnerabilidadeSeed
    {
        public int IdVulnerabilidade { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Localizacao { get; set; } = string.Empty;
        public string Servicos { get; set; } = string.Empty;
        public int ComunidadeId { get; set; }

        public List<VulnerabilidadeEixoSeed>? Eixos { get; set; }
    }

    public class VulnerabilidadeEixoSeed
    {
        public int IdVEixo { get; set; }
        public int EixoId { get; set; }
        public int VulnerabilidadeId { get; set; }
    }

    public class RedePrimariaSeed
    {
        public int IdRedePrimaria { get; set; }
        public int AtorPrincipalId { get; set; }
        public int AtorRelacionadoId { get; set; }
        public string TipoRelacao { get; set; } = string.Empty;
    }

    public class AvaliacaoPessoalSeed
    {
        public int IdAvaliacao { get; set; }
        public int AtorId { get; set; }
        public DateTime Data { get; set; }
        public string Indicador { get; set; } = string.Empty;
        public int Nota { get; set; }
    }

    public class FichaPrimeiroContatoSeed
    {
        public int IdFicha { get; set; }
        public int AtorId { get; set; }
        public string Localizacao { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public string LContato { get; set; } = string.Empty;
        public string FonteDados { get; set; } = string.Empty;
        public string EstaFamiliar { get; set; } = string.Empty;
        public string EstruFamiliar { get; set; } = string.Empty;
        public int NFilhos { get; set; }
        public int NFilhas { get; set; }
        public int AEscola { get; set; }
        public string SLer { get; set; } = string.Empty;
        public string SCalc { get; set; } = string.Empty;
        public string SComp { get; set; } = string.Empty;
        public int QReabili { get; set; }
        public string LTrat { get; set; } = string.Empty;
        public string Coment { get; set; } = string.Empty;
        public string CPrimeiroContato { get; set; } = string.Empty;
        public string EParceiro { get; set; } = string.Empty;
        public string FPeloParceirto { get; set; } = string.Empty;
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }

        public List<FichaCondicaoSeed>? Condicoes { get; set; }
        public List<FichaPeticaoSeed>? Peticoes { get; set; }
        public List<FichaRespSeed>? Respostas { get; set; }
        public List<FichaResultSeed>? Resultados { get; set; }
    }

    public class FichaCondicaoSeed
    {
        public int IdCondicoes { get; set; }
        public int FichaId { get; set; }
        public string Cond { get; set; } = string.Empty;
    }

    public class FichaPeticaoSeed
    {
        public int IdPeticoes { get; set; }
        public int FichaId { get; set; }
        public string Pet { get; set; } = string.Empty;
    }

    public class FichaRespSeed
    {
        public int IdCondicoes { get; set; }
        public int FichaId { get; set; }
        public string Resp { get; set; } = string.Empty;
    }

    public class FichaResultSeed
    {
        public int IdCondicoes { get; set; }
        public int FichaId { get; set; }
        public string Result { get; set; } = string.Empty;
    }

    public class AtividadeSeed
    {
        public int IdAtividade { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;

        public List<AtividadeEixoSeed>? Eixos { get; set; }
    }

    public class AtividadeEixoSeed
    {
        public int IdAEixo { get; set; }
        public int EixoId { get; set; }
        public int AtividadeId { get; set; }
    }
}