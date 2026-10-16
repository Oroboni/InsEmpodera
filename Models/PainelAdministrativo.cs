namespace Empodera.Models;

// Indicador exibido no inventário de dados da tela Dados e Backup.
public sealed record IndicadorInventario(string Rotulo, string Icone, int Total);

// Dados da conta exibidos em Configurações.
public sealed record ContaUsuarioResumo(string Nome, string Email, string Perfil, string? Ocupacao, bool PodeVerPerfis, bool IsAdmin);

// Números e atividade recente exibidos na página inicial.
public sealed class PainelInicial
{
    public int Comunidades { get; init; }
    public int ComunidadesEmProcesso { get; init; }
    public int Atores { get; init; }
    public int AtoresUltimos30Dias { get; init; }
    public int Fichas { get; init; }
    public int FichasEmProgresso { get; init; }
    public int DiariosCampo { get; init; }
    public int DiariosUltimos30Dias { get; init; }
    public int UsuariosAtivos { get; init; }
    public IReadOnlyList<AtorRecente> AtoresRecentes { get; init; } = [];
}

public sealed record AtorRecente(Guid Id, string Nome, string? Comunidade, DateTime CriadoEm);
