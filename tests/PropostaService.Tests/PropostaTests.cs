using FluentAssertions;
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

    #region Testes de Criação

    [Fact]
    public void CriarProposta_DeveIniciarEmAnalise()
    {
        var proposta = CriarPropostaPadrao();

        proposta.Status.Should().Be(StatusProposta.EmAnalise);
        proposta.Id.Should().NotBe(Guid.Empty);
        proposta.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CriarProposta_DeveDefinirPropriedadesCorretamente()
    {
        var proposta = new Proposta(
            "João Silva",
            "98765432101",
            "Auto",
            50000m,
            150m
        );

        proposta.NomeCliente.Should().Be("João Silva");
        proposta.CpfCliente.Should().Be("98765432101");
        proposta.TipoSeguro.Should().Be("Auto");
        proposta.ValorCobertura.Should().Be(50000m);
        proposta.ValorPremio.Should().Be(150m);
        proposta.DataAtualizacao.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CriarProposta_DeveFalhar_NomeClienteInvalido(string nomeInvalido)
    {
        Action act = () => new Proposta(nomeInvalido, "12345678901", "Auto", 100000m, 500m);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("nomeCliente");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CriarProposta_DeveFalhar_CpfClienteInvalido(string cpfInvalido)
    {
        Action act = () => new Proposta("João Silva", cpfInvalido, "Auto", 100000m, 500m);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("cpfCliente");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CriarProposta_DeveFalhar_TipoSeguroInvalido(string tipoInvalido)
    {
        Action act = () => new Proposta("João Silva", "12345678901", tipoInvalido, 100000m, 500m);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("tipoSeguro");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void CriarProposta_DeveFalhar_ValorCoberturaInvalido(decimal valorInvalido)
    {
        Action act = () => new Proposta("João Silva", "12345678901", "Auto", valorInvalido, 500m);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cobertura*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void CriarProposta_DeveFalhar_ValorPremioInvalido(decimal valorInvalido)
    {
        Action act = () => new Proposta("João Silva", "12345678901", "Auto", 100000m, valorInvalido);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*premio*");
    }

    #endregion

    #region Testes de Alteração de Status

    [Fact]
    public void AlterarStatus_DevePermitirAprovada()
    {
        var proposta = CriarPropostaPadrao();

        proposta.AlterarStatus(StatusProposta.Aprovada);

        proposta.Status.Should().Be(StatusProposta.Aprovada);
        proposta.DataAtualizacao.Should().NotBeNull();
        proposta.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AlterarStatus_DevePermitirRejeitada()
    {
        var proposta = CriarPropostaPadrao();

        proposta.AlterarStatus(StatusProposta.Rejeitada);

        proposta.Status.Should().Be(StatusProposta.Rejeitada);
        proposta.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void AlterarStatus_DeveFalharParaContratadaDireto()
    {
        var proposta = CriarPropostaPadrao();

        Action act = () => proposta.AlterarStatus(StatusProposta.Contratada);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*EmAnalise*Contratada*");
    }

    [Fact]
    public void AlterarStatus_NaoDevePermitirAlterarRejeitada()
    {
        var proposta = CriarPropostaPadrao();
        proposta.AlterarStatus(StatusProposta.Rejeitada);

        Action act = () => proposta.AlterarStatus(StatusProposta.Aprovada);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Rejeitada*");
    }

    [Fact]
    public void AlterarStatus_NaoDevePermitirAlterarContratada()
    {
        var proposta = CriarPropostaPadrao();
        proposta.AlterarStatus(StatusProposta.Aprovada);
        proposta.MarcarComoContratada();

        Action act = () => proposta.AlterarStatus(StatusProposta.EmAnalise);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Contratada*");
    }

    [Theory]
    [InlineData(StatusProposta.EmAnalise, StatusProposta.Aprovada, true)]
    [InlineData(StatusProposta.EmAnalise, StatusProposta.Rejeitada, true)]
    [InlineData(StatusProposta.EmAnalise, StatusProposta.Contratada, false)]
    [InlineData(StatusProposta.Aprovada, StatusProposta.Contratada, true)]
    [InlineData(StatusProposta.Aprovada, StatusProposta.EmAnalise, false)]
    [InlineData(StatusProposta.Aprovada, StatusProposta.Rejeitada, false)]
    [InlineData(StatusProposta.Rejeitada, StatusProposta.Aprovada, false)]
    [InlineData(StatusProposta.Rejeitada, StatusProposta.EmAnalise, false)]
    [InlineData(StatusProposta.Contratada, StatusProposta.Aprovada, false)]
    [InlineData(StatusProposta.Contratada, StatusProposta.EmAnalise, false)]
    public void AlterarStatus_DeveValidarTransicoesDeStatus(
        StatusProposta statusOrigem,
        StatusProposta statusDestino,
        bool devePermitir)
    {
        var proposta = CriarPropostaPadrao();

        // Configurar estado inicial
        if (statusOrigem == StatusProposta.Aprovada)
        {
            proposta.AlterarStatus(StatusProposta.Aprovada);
        }
        else if (statusOrigem == StatusProposta.Rejeitada)
        {
            proposta.AlterarStatus(StatusProposta.Rejeitada);
        }
        else if (statusOrigem == StatusProposta.Contratada)
        {
            proposta.AlterarStatus(StatusProposta.Aprovada);
            proposta.MarcarComoContratada();
        }

        // Ação
        Action act = () => proposta.AlterarStatus(statusDestino);

        // Validação
        if (devePermitir)
        {
            act.Should().NotThrow();
            proposta.Status.Should().Be(statusDestino);
        }
        else
        {
            act.Should().Throw<InvalidOperationException>();
        }
    }

    #endregion

    #region Testes de Contratação

    [Fact]
    public void MarcarComoContratada_DeveFuncionarQuandoAprovada()
    {
        var proposta = CriarPropostaPadrao();
        proposta.AlterarStatus(StatusProposta.Aprovada);

        proposta.MarcarComoContratada();

        proposta.Status.Should().Be(StatusProposta.Contratada);
        proposta.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void MarcarComoContratada_DeveFalharQuandoEmAnalise()
    {
        var proposta = CriarPropostaPadrao();

        Action act = () => proposta.MarcarComoContratada();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*aprovadas*contratadas*");
    }

    [Fact]
    public void MarcarComoContratada_DeveFalharQuandoRejeitada()
    {
        var proposta = CriarPropostaPadrao();
        proposta.AlterarStatus(StatusProposta.Rejeitada);

        Action act = () => proposta.MarcarComoContratada();

        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Testes de Edge Cases

    [Fact]
    public void CriarProposta_DeveAceitarValoresMinimosMaiorQueZero()
    {
        var proposta = new Proposta(
            "A",
            "1",
            "X",
            0.01m,
            0.01m
        );

        proposta.ValorCobertura.Should().Be(0.01m);
        proposta.ValorPremio.Should().Be(0.01m);
    }

    [Fact]
    public void CriarProposta_DeveAceitarValoresAltos()
    {
        var proposta = new Proposta(
            "Cliente Empresarial",
            "12345678901234",
            "Empresarial",
            999_999_999_999.99m,
            99_999_999.99m
        );

        proposta.ValorCobertura.Should().Be(999_999_999_999.99m);
        proposta.ValorPremio.Should().Be(99_999_999.99m);
    }

    [Fact]
    public void AlterarStatus_DeveManterStatusAnteriorEmCasoDeErro()
    {
        var proposta = CriarPropostaPadrao();
        var statusInicial = proposta.Status;

        try
        {
            proposta.AlterarStatus(StatusProposta.Contratada);
        }
        catch
        {
            // Ignorar erro esperado
        }

        proposta.Status.Should().Be(statusInicial);
    }

    #endregion
}
