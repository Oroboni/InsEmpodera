using System;
using System.Collections.Generic;
using Empodera.Models;

namespace Empodera.Data
{
    public class UsuarioSeed
    {
        public int Id_Usuario { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string? Foto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Ocupacao { get; set; } = string.Empty;
        public string? Genero { get; set; } = string.Empty;
        public DateTime Dt_Nascimento { get; set; }
        public int Nivel_Permissao { get; set; }
        public DateTime Dt_Criacao { get; set; }
        public DateTime? Dt_Atualizacao { get; set; }
        public string Ativo { get; set; } = string.Empty;
        
        public int FkIdPerfil { get; set; }
        
        public Perfil Perfil { get; set; } = null!;
        public List<ComunidadeSeed>? Comunidades { get; set; }
        public List<AtoresSeed>? Atores { get; set; }
        public List<RedeRecursosSeed>? Redes { get; set; }
        public List<DiarioCampoSeed>? Diarios { get; set; }
        public List<AvaliacaoPessoalSeed>? Avaliacoes { get; set; }
        public List<FichaPrimeiroContatoSeed>? Fichas { get; set; }
        public List<AtividadeSeed>? Atividades { get; set; }
    }

    public class PerfilSeed
    {
        public int Id_Perfil { get; set; }
        public string Nome { get; set; } = string.Empty;

        public List<UsuarioSeed> Usuarios { get; set; } = new();
        public List<PermissoesSeed>? Permissoes { get; set; }
    }

    public class PermissoesSeed
    {
        public int IdPermissoes { get; set; }
        public int FkIdPerfil { get; set; }
        public string Modulo { get; set; } = string.Empty;
        public string PodeListar { get; set; } = "N";
        public string PodeDetalhar { get; set; } = "N";
        public string PodeCriar { get; set; } = "N";
        public string PodeAtualizar { get; set; } = "N";
        public string PodeDeletar { get; set; } = "N";

        public Perfil Perfil { get; set; } = null!;
    }

    public class ComunidadeSeed
    {
        public int Id_Comunidade { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Local { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Complemento { get; set; } = string.Empty;
        public string? Descricao { get; set; } = string.Empty;
        public string? Descricao_Acessibilidade { get; set; } = string.Empty;
        public DateTime Dt_Criacao { get; set; }
        public DateTime Dt_Modificacao { get; set; }
        public int FK_Id_Usuario { get; set; }
        public int? FK_Id_UsuarioM { get; set; }
        public string Ativo { get; set; } = "S";

        public List<UsuarioSeed>? Usuarios {get; set;}
        public List<AtoresComunidadeSeed>? AtoresComunidade { get; set; }
        public List<RedeRecursosSeed>? Redes { get; set; }
        public List<DiarioCampoSeed>? Diarios { get; set; }
        public List<VulnerabilidadeSeed>? Vulnerabilidades { get; set; }
        public List<AtividadeSeed>? Atividades { get; set; }
    }

    public class AtoresSeed
    {
        public int Id_Atores { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Genero { get; set; }
        public DateTime? Dt_Nascimento { get; set; }
        public string? Papel_Social1 { get; set; }
        public string? Papel_Social2 { get; set; }
        public int? Telefone { get; set; }
        public bool DaEquipe { get; set; } = false;
        public bool Rope { get; set; } = false;
        public bool Lopiniao { get; set; } = false;
        public bool Mcomunidade { get; set; } = false;
        public DateTime Dt_Criacao { get; set; }
        public DateTime Dt_Modificacao { get; set; }
        public string Ativo { get; set; } = "S";
        public int FkIdUsuario { get; set; }
        public int? FkIdUsuarioM { get; set; }

        public List<UsuarioSeed>? Usuarios {get; set;}
        public List<RedeRecursosSeed>? Redes { get; set; }
        public List<AtoresComunidadeSeed>? Comunidades { get; set; }
        public List<AcoesAtoresSeed>? Acoes { get; set; }
        public List<AvaliacaoPessoalSeed>? Avaliacoes { get; set; }
        public List<RedePrimariaSeed>? RedesPrimarias { get; set; }
    }

    public class RedeRecursosSeed
    {
        public int Id_Rede { get; set; }
        public int? Fk_Id_Ator { get; set; }
        public int Fk_Id_Comunidade { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string? Nome { get; set; }
        public string? Dispositivo { get; set; }
        public string? Localizacao { get; set; }
        public string? Servicos { get; set; }
        public DateTime Dt_Criacao { get; set; }
        public DateTime Dt_Modificacao { get; set; }
        public int FK_Id_Usuario { get; set; }

        public List<RedeEixoSeed>? Eixos { get; set; }
    }

    public class EixoSeed
    {
        public int Id_Eixo { get; set; }
        public string Nome { get; set; } = string.Empty;
    }

    public class RedeEixoSeed
    {
        public int Id_Rede_Eixo { get; set; }
        public int Fk_Id_Rede { get; set; }
        public int Fk_Id_Eixo { get; set; }
    }

    public class AtoresComunidadeSeed
    {
        public int Id_A_Comunidade { get; set; }
        public int Fk_Id_Comunidade { get; set; }
        public int Fk_Id_Ator { get; set; }
    }

    public class DiarioCampoSeed
    {
        public int Id_D_Campo { get; set; }
        public int Fk_Id_Comunidade { get; set; }
        public DateTime Data { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string Localizacao { get; set; } = string.Empty;
        public DateTime Dt_Criacao { get; set; }
        public DateTime Dt_Modificacao { get; set; }
        public string Foto { get; set; } = string.Empty;
        public int FK_Id_Usuario { get; set; }

        public List<DiarioAcoesSeed>? DiarioAcoes { get; set; }
        public List<DiarioDAcoesSeed>? DetalhesAcoes { get; set; }
        public List<DiarioEixoSeed>? DiarioEixos { get; set; }
        public List<AnexosDiarioSeed>? Anexos { get; set; }
    }

    public class DiarioAcoesSeed
    {
        public int Id_D_Acoes { get; set; }
        public int Fk_Id_Acoes { get; set; }
        public int Fk_Id_Diario { get; set; }
    }

    public class DiarioDAcoesSeed
    {
        public int Id_D_Acoes { get; set; }
        public int Fk_Id_Diario { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string PeovedorEx { get; set; } = string.Empty;
        public int Quantidade { get; set; }

        public List<DAAtoresSeed>? Atores { get; set; }
    }

    public class DetalhesDAcoesSeed
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Fk_Id_D_D_Acoes { get; set; }
    }

    public class DetalhesEixosSeed
    {
        public int Id_Diario_Eixo { get; set; }
        public int Fk_Id_Detalhes { get; set; }
        public int Fk_Id_Eixo { get; set; }
    }

    public class DAAtoresSeed
    {
        public int Id { get; set; }
        public int Fk_Id_D_D_Acoes { get; set; }
        public int Fk_Id_Atores { get; set; }
    }

    public class DiarioEixoSeed
    {
        public int Id_Diario_Eixo { get; set; }
        public int Fk_Id_Diario { get; set; }
        public int Fk_Id_Eixo { get; set; }
    }

    public class AcoesSeed
    {
        public int Id_Acoes { get; set; }
        public int Quantidade { get; set; }
        public int Fk_Id_Atividade { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Provedor { get; set; } = string.Empty;

        public List<AcoesAtoresSeed>? Atores { get; set; }
    }

    public class AcoesAtoresSeed
    {
        public int Id_A_Atores { get; set; }
        public int Fk_Id_Atores { get; set; }
        public int Fk_Id_Acoes { get; set; }
    }

    public class AnexosDiarioSeed
    {
        public int Id_Anexos { get; set; }
        public int Fk_Id_Diario { get; set; }
        public string Caminho { get; set; } = string.Empty;
    }

    public class VulnerabilidadeSeed
    {
        public int Id_Vulnerabilidade { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Localizacao { get; set; } = string.Empty;
        public string Servicos { get; set; } = string.Empty;
        public int Fk_Id_Comunidade { get; set; }

        public List<VulnerabilidadesEixoSeed>? Eixos { get; set; }
    }

    public class VulnerabilidadesEixoSeed
    {
        public int Id_V_Eixo { get; set; }
        public int Fk_Id_Eixo { get; set; }
        public int Fk_Id_Vulnerabilidades { get; set; }
    }

    public class RedePrimariaSeed
    {
        public int Id_Rede_Primaria { get; set; }
        public int Fk_Id_Ator_Principal { get; set; }
        public int Fk_Id_Ator_Relacionados { get; set; }
        public string Tipo_Relacao { get; set; } = string.Empty;
    }

    public class AvaliacaoPessoalSeed
    {
        public int Id_Avaliacao { get; set; }
        public int Fk_Id_Ator { get; set; }
        public int CCrimes { get; set; }
        public int Substancias { get; set; }
        public int Moradia { get; set; }
        public int Prevenção { get; set; }
        public int AssBasica { get; set; }
        public int Educacao { get; set; }
        public int Saude { get; set; }
        public int Ocupacao { get; set; }
        public int Lazer { get; set; }
        public DateTime Dt_Criacao { get; set; }
        public DateTime Dt_Modificacao { get; set; }
        public int FK_Id_Usuario { get; set; }
    }

    public class FichaPrimeiroContatoSeed
    {
        public int IdFicha { get; set; }       
        public int FKidAtores{ get; set; }       
        public string Endereco { get; set; } = null!;
        public string? Complemento { get; set; } = null!; 
        public string? Emprego { get; set; } = null!;
        public string? CEstabeleceu { get; set; } = null!;
        public string? NovoParceiro { get; set; } = null!;
        public string? FornecidoParceiro { get; set; } = null!;
        public string? Telefone { get; set; } = null!;
        public string? LContato { get; set; } = null!;
        public string? FonteDados { get; set; } = null!;
        public string? EstaFamiliar { get; set; } = null!;
        public string? EstruFamiliar { get; set; } = null!;
        public int? NFIlhos { get; set; }        
        public int? NFilhas { get; set; }         
        public int? AEscolar { get; set; }       
        public string? SLer { get; set; } = null!;
        public string? SCalc { get; set; } = null!;
        public string? SComp { get; set; } = null!;
        public int? QReabili { get; set; }
        public string? LTrat { get; set; }
        public string? Coment { get; set; } = null!;
        public DateTime DtContato { get; set; }
        public DateTime HoraContato { get; set; } 
        public DateTime DtCriacao { get; set; }
        public DateTime DtModificacao { get; set; }
        public int FkIdUsuario { get; set; }

        public List<FonteInfSeed>? Fontes { get; set; }
        public ICollection<FichaCondicoes>? FichaCondicoes { get; set; }
        public ICollection<FichaPeticoes>? FichaPeticoes { get; set; }
        public ICollection<FichaResp>? FichaRespostas { get; set; }
        public ICollection<FichaResult>? FichaResultados { get; set; }
    }

    public class FonteInfSeed
    {
        public int Id_Fonte { get; set; }
        public int Fk_Id_Ficha { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public int Idade { get; set; }
        public string Papel_Social1 { get; set; } = string.Empty;
        public string Papel_Social2 { get; set; } = string.Empty;
        public int Telefone { get; set; }
        public string Extra { get; set; } = string.Empty;
        public int? Fk_Id_Ator { get; set; } = null!;
    }

    public class FichaCondicoesSeed
    {
        public int Id_Condicoes { get; set; }
        public int Fk_Id_Ficha { get; set; }
        public string Cond { get; set; } = string.Empty;
    }

    public class FichaPeticoesSeed
    {
        public int Id_Peticoes { get; set; }
        public int Fk_Id_Ficha { get; set; }
        public string Pet { get; set; } = string.Empty;
    }

    public class FichaRespSeed
    {
        public int Id_Condicoes { get; set; }
        public int Fk_Id_Ficha { get; set; }
        public string Resp { get; set; } = string.Empty;
    }

    public class FichaResultSeed
    {
        public int Id_Condicoes { get; set; }
        public int Fk_Id_Ficha { get; set; }
        public string Result { get; set; } = string.Empty;
    }

    public class AtividadeSeed
    {
        public int Id_Atividade { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string? Foto { get; set; } = string.Empty;
        public int FK_Id_Comunidade { get; set; }
        public int FkIdUsuario { get; set; }
        public int? FkIdUsuarioM { get; set; }

        public List<UsuarioSeed>? Usuarios {get; set;}

        public List<AtividadeEixoSeed>? Eixos { get; set; }
        public List<AcoesSeed>? Acoes { get; set; }
    }

    public class AtividadeEixoSeed
    {
        public int Id_A_Eixo { get; set; }
        public int Fk_Id_Eixo { get; set; }
        public int Fk_Id_Atividade { get; set; }
    }
}
