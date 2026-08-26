using System.Text.Json;

namespace Empodera.Models;

public sealed class SystemBackupDocument
{
    public string Product { get; set; } = "InsEmpodera";
    public int FormatVersion { get; set; } = 1;
    public string SchemaFingerprint { get; set; } = string.Empty;
    public DateTime ExportedAtUtc { get; set; }
    public List<SystemBackupTable> Tables { get; set; } = [];
}

public sealed class SystemBackupTable
{
    public string Entity { get; set; } = string.Empty;
    public string Table { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = [];
    public List<List<JsonElement>> Rows { get; set; } = [];
}
