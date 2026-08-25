using Empodera.Controllers;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace InsEmpodera.Tests.Security;

public sealed class ReportUploadSecurityTests : ControllerTestBase
{
    [Fact]
    public async Task SpreadsheetWithTraversalFilename_NeverEscapesUploadAreaAndTemporaryFileIsCleaned()
    {
        var generatedName = $"owned-{Guid.NewGuid():N}.xlsx";
        var maliciousName = $@"..\..\{generatedName}";
        var legacyUploadRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        var escapedPath = Path.GetFullPath(Path.Combine(legacyUploadRoot, maliciousName));
        Assert.False(System.IO.File.Exists(escapedPath));

        var existingTemporaryFiles = Directory
            .GetFiles(Path.GetTempPath(), "empodera-import-*")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        await using var content = new MemoryStream(new byte[] { 0x00, 0x01, 0x02, 0x03 });
        IFormFile upload = new FormFile(content, 0, content.Length, "files", maliciousName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };
        var controller = Attach(new ReportController(Db));

        try
        {
            await Assert.ThrowsAnyAsync<Exception>(
                () => controller.RelatorioComunidade(new List<IFormFile> { upload }));

            Assert.False(System.IO.File.Exists(escapedPath));
            var remainingTemporaryFiles = Directory
                .GetFiles(Path.GetTempPath(), "empodera-import-*")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            Assert.True(
                existingTemporaryFiles.SetEquals(remainingTemporaryFiles),
                "O processamento deixou um arquivo temporário empodera-import-* sem limpeza.");
        }
        finally
        {
            if (System.IO.File.Exists(escapedPath))
                System.IO.File.Delete(escapedPath);
        }
    }

    [Fact]
    public async Task SpreadsheetUpload_RejectsUnsupportedExtensionBeforeWritingToDisk()
    {
        await using var content = new MemoryStream(new byte[] { 0x01 });
        IFormFile upload = new FormFile(content, 0, content.Length, "files", "dados.exe");
        var controller = Attach(new ReportController(Db));

        var result = await controller.RelatorioComunidade(new List<IFormFile> { upload });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains(".xls", badRequest.Value?.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}