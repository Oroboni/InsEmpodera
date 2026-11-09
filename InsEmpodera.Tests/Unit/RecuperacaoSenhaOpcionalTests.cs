using Empodera.Services.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace InsEmpodera.Tests.Unit;

// Enquanto a conta do Gmail não existe, o sistema precisa subir em produção com a recuperação por e-mail desligada.
public sealed class RecuperacaoSenhaOpcionalTests
{
    [Fact(DisplayName = "E-mail — sem senha de aplicativo a recuperação fica desligada e nada é enfileirado")]
    public void SemSenha_RecuperacaoDesligada()
    {
        using var remetente = Remetente(senha: "");
        Assert.False(remetente.Disponivel);
        Assert.False(remetente.TryQueue("pessoa@exemplo.org", "https://ecoredes.exemplo.org/Account/ResetPassword"));
    }

    [Fact(DisplayName = "E-mail — com senha de aplicativo a recuperação fica ligada")]
    public void ComSenha_RecuperacaoLigada()
    {
        using var remetente = Remetente(senha: "abcd efgh ijkl mnop");
        Assert.True(remetente.Disponivel);
        Assert.True(remetente.TryQueue("pessoa@exemplo.org", "https://ecoredes.exemplo.org/Account/ResetPassword"));
    }

    [Fact(DisplayName = "E-mail — produção inicia sem senha, mas recusa senha preenchida e inválida")]
    public void Producao_ValidaSoQuandoConfigurado()
    {
        Assert.False(OpcoesEmProducao(senha: null).Configurado);
        Assert.True(OpcoesEmProducao(senha: "abcdefghijklmnop", urlPublica: "https://ecoredes.exemplo.org").Configurado);
        Assert.Throws<OptionsValidationException>(() => OpcoesEmProducao(senha: "curta"));
        Assert.Throws<OptionsValidationException>(() => OpcoesEmProducao(senha: "abcdefghijklmnop", urlPublica: "http://sem-https"));
    }

    private static GmailSmtpPasswordResetEmailSender Remetente(string senha) =>
        new(new MonitorFixo(new GmailSmtpOptions { Password = senha }), NullLogger<GmailSmtpPasswordResetEmailSender>.Instance);

    private static GmailSmtpOptions OpcoesEmProducao(string? senha, string? urlPublica = null)
    {
        var valores = new Dictionary<string, string?> { ["Email:Password"] = senha, ["Email:PublicBaseUrl"] = urlPublica };
        var configuracao = new ConfigurationBuilder().AddInMemoryCollection(valores).Build();
        var servicos = new ServiceCollection().AddLogging();
        servicos.AddGmailPasswordRecovery(configuracao, new AmbienteDeProducao());
        using var provedor = servicos.BuildServiceProvider();
        return provedor.GetRequiredService<IOptions<GmailSmtpOptions>>().Value;
    }

    private sealed class MonitorFixo(GmailSmtpOptions valor) : IOptionsMonitor<GmailSmtpOptions>
    {
        public GmailSmtpOptions CurrentValue => valor;
        public GmailSmtpOptions Get(string? name) => valor;
        public IDisposable? OnChange(Action<GmailSmtpOptions, string?> listener) => null;
    }

    private sealed class AmbienteDeProducao : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "InsEmpodera";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
