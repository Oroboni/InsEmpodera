using ClosedXML.Excel;
using Empodera.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Empodera.Services;

public sealed class SpreadsheetExportService(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<byte[]> ExportCommunityAsync(int communityId, CancellationToken cancellationToken)
    {
        var community = await _context.Comunidades.AsNoTracking()
            .Where(item => item.Id_Comunidade == communityId)
            .ToListAsync(cancellationToken);
        if (community.Count == 0)
            throw new KeyNotFoundException("Comunidade não encontrada.");

        var communityActorIds = await _context.AtorComunidades.AsNoTracking()
            .Where(item => item.FkIdComunidade == communityId)
            .Select(item => item.FK_id_Atores).Distinct().ToArrayAsync(cancellationToken);
        var activityIds = await _context.Atividades.AsNoTracking()
            .Where(item => item.FkIdComunidade == communityId)
            .Select(item => item.IdAtividade).ToArrayAsync(cancellationToken);
        var resourceIds = await _context.RedeRecursos.AsNoTracking()
            .Where(item => item.FkIdComunidade == communityId)
            .Select(item => item.Id_Rede).ToArrayAsync(cancellationToken);
        var vulnerabilityIds = await _context.Vulnerabilidades.AsNoTracking()
            .Where(item => item.FkIdComunidade == communityId)
            .Select(item => item.IdVulnerabilidade).ToArrayAsync(cancellationToken);
        var diaryIds = await _context.DiariosCampo.AsNoTracking()
            .Where(item => item.FkIdComunidade == communityId)
            .Select(item => item.IdDCampo).ToArrayAsync(cancellationToken);
        var formIds = await _context.FichasPrimeiroContato.AsNoTracking()
            .Where(item => item.FkIdComunidade == communityId)
            .Select(item => item.IdFicha).ToArrayAsync(cancellationToken);
        var institutionalActionIds = await _context.DiarioDAcoes.AsNoTracking()
            .Where(item => diaryIds.Contains(item.FkIdDiario))
            .Select(item => item.IdDAcoes).ToArrayAsync(cancellationToken);
        var actionDetailIds = await _context.DetalhesDAcoes.AsNoTracking()
            .Where(item => institutionalActionIds.Contains(item.FkIdDDacoes))
            .Select(item => item.Id).ToArrayAsync(cancellationToken);
        var actionIds = await _context.Acoes.AsNoTracking()
            .Where(item => activityIds.Contains(item.FkIdAtividade))
            .Select(item => item.IdAcoes).ToArrayAsync(cancellationToken);
        var referencedActorIds = communityActorIds
            .Concat(await _context.RedeRecursos.AsNoTracking().Where(item => resourceIds.Contains(item.Id_Rede) && item.FK_id_Atores != null).Select(item => item.FK_id_Atores!.Value).ToArrayAsync(cancellationToken))
            .Concat(await _context.AcoesAtores.AsNoTracking().Where(item => actionIds.Contains(item.FkIdAcoes)).Select(item => item.FK_id_Atores).ToArrayAsync(cancellationToken))
            .Concat(await _context.DAAtores.AsNoTracking().Where(item => institutionalActionIds.Contains(item.FkIdDDacoes)).Select(item => item.FK_id_Atores).ToArrayAsync(cancellationToken))
            .Concat(await _context.FichasPrimeiroContato.AsNoTracking().Where(item => formIds.Contains(item.IdFicha)).Select(item => item.FK_id_Atores).ToArrayAsync(cancellationToken))
            .Distinct().ToArray();
        var primaryNetworks = await _context.RedesPrimarias.AsNoTracking()
            .Where(item => referencedActorIds.Contains(item.FkIdAtorPrincipal) || referencedActorIds.Contains(item.FkIdAtorRelacionados))
            .ToListAsync(cancellationToken);
        var actorIds = referencedActorIds
            .Concat(primaryNetworks.Select(item => item.FkIdAtorPrincipal))
            .Concat(primaryNetworks.Select(item => item.FkIdAtorRelacionados))
            .Distinct().ToArray();

        using var workbook = NewWorkbook("Comunidade e dependências");
        AddSheet(workbook, "Comunidades", community);
        AddSheet(workbook, "Atores", await _context.Atores.AsNoTracking().Where(item => actorIds.Contains(item.IdAtores)).ToListAsync(cancellationToken));
        AddSheet(workbook, "AtorComunidades", await _context.AtorComunidades.AsNoTracking().Where(item => item.FkIdComunidade == communityId).ToListAsync(cancellationToken));
        AddSheet(workbook, "RecursosAtores", await _context.RecursosAtores.AsNoTracking().Where(item => actorIds.Contains(item.FK_id_Atores)).ToListAsync(cancellationToken));
        AddSheet(workbook, "AvaliacoesPessoais", await _context.AvaliacaoPessoal.AsNoTracking().Where(item => actorIds.Contains(item.FK_id_Atores)).ToListAsync(cancellationToken));
        AddSheet(workbook, "RedesPrimarias", primaryNetworks);
        AddSheet(workbook, "Atividades", await _context.Atividades.AsNoTracking().Where(item => activityIds.Contains(item.IdAtividade)).ToListAsync(cancellationToken));
        AddSheet(workbook, "AtividadesEixos", await _context.AtividadesEixo.AsNoTracking().Where(item => activityIds.Contains(item.FkIdAtividade)).ToListAsync(cancellationToken));
        AddSheet(workbook, "Acoes", await _context.Acoes.AsNoTracking().Where(item => actionIds.Contains(item.IdAcoes)).ToListAsync(cancellationToken));
        AddSheet(workbook, "AcoesAtores", await _context.AcoesAtores.AsNoTracking().Where(item => actionIds.Contains(item.FkIdAcoes)).ToListAsync(cancellationToken));
        AddSheet(workbook, "Recursos", await _context.RedeRecursos.AsNoTracking().Where(item => resourceIds.Contains(item.Id_Rede)).ToListAsync(cancellationToken));
        AddSheet(workbook, "RecursosEixos", await _context.RedeEixos.AsNoTracking().Where(item => resourceIds.Contains(item.FkIdRede)).ToListAsync(cancellationToken));
        AddSheet(workbook, "Vulnerabilidades", await _context.Vulnerabilidades.AsNoTracking().Where(item => vulnerabilityIds.Contains(item.IdVulnerabilidade)).ToListAsync(cancellationToken));
        AddSheet(workbook, "VulnerabilidadesEixos", await _context.VulnerabilidadesEixo.AsNoTracking().Where(item => vulnerabilityIds.Contains(item.FkIdVulnerabilidade)).ToListAsync(cancellationToken));
        await AddDiarySheetsAsync(workbook, diaryIds, institutionalActionIds, actionDetailIds, cancellationToken);
        await AddFirstContactSheetsAsync(workbook, formIds, cancellationToken);
        AddSheet(workbook, "Eixos", await _context.Eixos.AsNoTracking().ToListAsync(cancellationToken));
        return Save(workbook);
    }

    public async Task<byte[]> ExportCommunityActorsAsync(int communityId, CancellationToken cancellationToken)
    {
        var links = await _context.AtorComunidades.AsNoTracking()
            .Where(item => item.FkIdComunidade == communityId).ToListAsync(cancellationToken);
        var actorIds = links.Select(item => item.FK_id_Atores).Distinct().ToArray();
        var networks = await _context.RedesPrimarias.AsNoTracking()
            .Where(item => actorIds.Contains(item.FkIdAtorPrincipal) || actorIds.Contains(item.FkIdAtorRelacionados))
            .ToListAsync(cancellationToken);
        var relatedActorIds = networks.SelectMany(item => new[] { item.FkIdAtorPrincipal, item.FkIdAtorRelacionados })
            .Except(actorIds).Distinct().ToArray();
        using var workbook = NewWorkbook("Atores da comunidade");
        AddSheet(workbook, "Atores", await _context.Atores.AsNoTracking().Where(item => actorIds.Contains(item.IdAtores)).ToListAsync(cancellationToken));
        AddSheet(workbook, "AtoresRelacionados", await _context.Atores.AsNoTracking().Where(item => relatedActorIds.Contains(item.IdAtores)).ToListAsync(cancellationToken));
        AddSheet(workbook, "VinculosComunidade", links);
        AddSheet(workbook, "RecursosAtores", await _context.RecursosAtores.AsNoTracking().Where(item => actorIds.Contains(item.FK_id_Atores)).ToListAsync(cancellationToken));
        AddSheet(workbook, "AvaliacoesPessoais", await _context.AvaliacaoPessoal.AsNoTracking().Where(item => actorIds.Contains(item.FK_id_Atores)).ToListAsync(cancellationToken));
        AddSheet(workbook, "RedesPrimarias", networks);
        return Save(workbook);
    }

    public async Task<byte[]> ExportCommunityActivitiesAsync(int communityId, CancellationToken cancellationToken)
    {
        var activities = await _context.Atividades.AsNoTracking()
            .Where(item => item.FkIdComunidade == communityId)
            .ToListAsync(cancellationToken);
        var activityIds = activities.Select(item => item.IdAtividade).ToArray();

        using var workbook = NewWorkbook("Atividades da comunidade");
        AddSheet(workbook, "Atividades", activities);
        AddSheet(workbook, "AtividadesEixos", await _context.AtividadesEixo.AsNoTracking()
            .Where(item => activityIds.Contains(item.FkIdAtividade))
            .ToListAsync(cancellationToken));
        return Save(workbook);
    }

    public async Task<byte[]> ExportActorsAsync(CancellationToken cancellationToken)
    {
        using var workbook = NewWorkbook("Atores");
        AddSheet(workbook, "Atores", await _context.Atores.AsNoTracking().ToListAsync(cancellationToken));
        AddSheet(workbook, "ComunidadesDosAtores", await _context.AtorComunidades.AsNoTracking().ToListAsync(cancellationToken));
        AddSheet(workbook, "RecursosAtores", await _context.RecursosAtores.AsNoTracking().ToListAsync(cancellationToken));
        AddSheet(workbook, "AvaliacoesPessoais", await _context.AvaliacaoPessoal.AsNoTracking().ToListAsync(cancellationToken));
        AddSheet(workbook, "RedesPrimarias", await _context.RedesPrimarias.AsNoTracking().ToListAsync(cancellationToken));
        return Save(workbook);
    }

    public async Task<byte[]> ExportFieldDiariesAsync(CancellationToken cancellationToken)
    {
        var diaryIds = await _context.DiariosCampo.AsNoTracking().Select(item => item.IdDCampo).ToArrayAsync(cancellationToken);
        var actionIds = await _context.DiarioDAcoes.AsNoTracking().Select(item => item.IdDAcoes).ToArrayAsync(cancellationToken);
        var detailIds = await _context.DetalhesDAcoes.AsNoTracking().Select(item => item.Id).ToArrayAsync(cancellationToken);
        using var workbook = NewWorkbook("Diários de campo");
        await AddDiarySheetsAsync(workbook, diaryIds, actionIds, detailIds, cancellationToken);
        return Save(workbook);
    }

    public async Task<byte[]> ExportFirstContactsAsync(CancellationToken cancellationToken)
    {
        var ids = await _context.FichasPrimeiroContato.AsNoTracking().Select(item => item.IdFicha).ToArrayAsync(cancellationToken);
        using var workbook = NewWorkbook("Fichas de primeiro contato");
        await AddFirstContactSheetsAsync(workbook, ids, cancellationToken);
        return Save(workbook);
    }

    private async Task AddDiarySheetsAsync(XLWorkbook workbook, int[] diaryIds, int[] institutionalActionIds, int[] detailIds, CancellationToken token)
    {
        AddSheet(workbook, "DiariosCampo", await _context.DiariosCampo.AsNoTracking().Where(item => diaryIds.Contains(item.IdDCampo)).ToListAsync(token));
        AddSheet(workbook, "DiariosEixos", await _context.DiarioEixos.AsNoTracking().Where(item => diaryIds.Contains(item.FkIdDiario)).ToListAsync(token));
        AddSheet(workbook, "AcoesInstitucionais", await _context.DiarioDAcoes.AsNoTracking().Where(item => diaryIds.Contains(item.FkIdDiario)).ToListAsync(token));
        AddSheet(workbook, "DetalhesAcoes", await _context.DetalhesDAcoes.AsNoTracking().Where(item => institutionalActionIds.Contains(item.FkIdDDacoes)).ToListAsync(token));
        AddSheet(workbook, "EixosDasAcoes", await _context.DetalhesEixos.AsNoTracking().Where(item => detailIds.Contains(item.FkIdDetalhes)).ToListAsync(token));
        AddSheet(workbook, "AtoresDasAcoes", await _context.DAAtores.AsNoTracking().Where(item => institutionalActionIds.Contains(item.FkIdDDacoes)).ToListAsync(token));
        AddSheet(workbook, "AcoesDaEquipe", await _context.DiarioAcoes.AsNoTracking().Where(item => diaryIds.Contains(item.FkIdDiario)).ToListAsync(token));
        AddSheet(workbook, "AnexosDiario", await _context.AnexosDiario.AsNoTracking().Where(item => diaryIds.Contains(item.FkIdDiario)).ToListAsync(token));
    }

    private async Task AddFirstContactSheetsAsync(XLWorkbook workbook, int[] formIds, CancellationToken token)
    {
        AddSheet(workbook, "FichasPrimeiroContato", await _context.FichasPrimeiroContato.AsNoTracking().Where(item => formIds.Contains(item.IdFicha)).ToListAsync(token));
        AddSheet(workbook, "FontesInformacao", await _context.FontesInfo.AsNoTracking().Where(item => formIds.Contains(item.FkIdFicha)).ToListAsync(token));
        AddSheet(workbook, "Condicoes", await _context.FichaCondicoes.AsNoTracking().Where(item => formIds.Contains(item.FkIdFicha)).ToListAsync(token));
        AddSheet(workbook, "Peticoes", await _context.FichaPeticoes.AsNoTracking().Where(item => formIds.Contains(item.FkIdFicha)).ToListAsync(token));
        AddSheet(workbook, "Respostas", await _context.FichaRespostas.AsNoTracking().Where(item => formIds.Contains(item.FkIdFicha)).ToListAsync(token));
        AddSheet(workbook, "Resultados", await _context.FichaResultados.AsNoTracking().Where(item => formIds.Contains(item.FkIdFicha)).ToListAsync(token));
    }

    private void AddSheet<TEntity>(XLWorkbook workbook, string name, IReadOnlyList<TEntity> rows) where TEntity : class
    {
        var worksheet = workbook.Worksheets.Add(name.Length <= 31 ? name : name[..31]);
        var entityType = _context.Model.FindEntityType(typeof(TEntity))
            ?? throw new InvalidOperationException($"Entidade {typeof(TEntity).Name} não mapeada.");
        var properties = entityType.GetProperties()
            .Where(property => property.PropertyInfo != null)
            .OrderByDescending(property => property.IsPrimaryKey())
            .ThenBy(property => property.Name)
            .ToArray();

        for (var column = 0; column < properties.Length; column++)
            worksheet.Cell(1, column + 1).Value = properties[column].Name;

        for (var row = 0; row < rows.Count; row++)
        for (var column = 0; column < properties.Length; column++)
            SetCellValue(worksheet.Cell(row + 2, column + 1), properties[column].PropertyInfo!.GetValue(rows[row]));

        var header = worksheet.Range(1, 1, 1, Math.Max(1, properties.Length));
        header.Style.Font.Bold = true;
        header.Style.Font.FontColor = XLColor.White;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#722B7C");
        header.SetAutoFilter();
        worksheet.SheetView.FreezeRows(1);
        worksheet.Columns().AdjustToContents(8, 45);
    }

    private static XLWorkbook NewWorkbook(string title)
    {
        var workbook = new XLWorkbook();
        workbook.Properties.Title = title;
        workbook.Properties.Subject = "Exportação InsEmpodera";
        workbook.Properties.Created = DateTime.UtcNow;
        return workbook;
    }

    private static void SetCellValue(IXLCell cell, object? value)
    {
        if (value == null) return;
        cell.Value = value switch
        {
            DateTime date => date,
            bool boolean => boolean,
            int number => number,
            long number => number,
            decimal number => number,
            double number => number,
            float number => number,
            _ => value.ToString() ?? string.Empty
        };
    }

    private static byte[] Save(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
