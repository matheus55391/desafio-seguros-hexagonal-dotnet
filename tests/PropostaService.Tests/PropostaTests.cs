using PropostaService.Domain.Entities;
using Xunit;

namespace PropostaService.Tests;

public class PropostaTests
{
    private static Proposta CriarPropostaPadrao()
    {
        return new Proposta(
            "Maria Silva",
            "12345678901",
            "Residencial",
            100000m,
            250m
        );
    }

    [Fact]
    public void CriarProposta_DeveIniciarEmAnalise()
    {
        var proposta = CriarPropostaPadrao();

        Assert.Equal(StatusProposta.EmAnalise, proposta.Status);
        Assert.NotEqual(Guid.Empty, proposta.Id);
    }

    [Fact]
    public void AlterarStatus_DevePermitirAprovada()
    {
        var proposta = CriarPropostaPadrao();

        proposta.AlterarStatus(StatusProposta.Aprovada);

        Assert.Equal(StatusProposta.Aprovada, proposta.Status);
    }

    [Fact]
    public void AlterarStatus_DeveFalharParaContratadaDireto()
    {
        var proposta = CriarPropostaPadrao();

        Assert.Throws<InvalidOperationException>(() => proposta.AlterarStatus(StatusProposta.Contratada));
    }

    [Fact]
    public void MarcarComoContratada_DeveFuncionarQuandoAprovada()
    {
        var proposta = CriarPropostaPadrao();

        proposta.AlterarStatus(StatusProposta.Aprovada);
        proposta.MarcarComoContratada();

        Assert.Equal(StatusProposta.Contratada, proposta.Status);
    }
}
