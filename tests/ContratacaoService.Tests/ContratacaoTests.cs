using ContratacaoService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace ContratacaoService.Tests;

public class ContratacaoTests
{
    #region Testes de Criação

    [Fact]
    public void CriarContratacao_DeveDefinirPropriedadesCorretamente()
    {
        var propostaId = Guid.NewGuid();

        var contratacao = new Contratacao(propostaId);

        contratacao.Id.Should().NotBe(Guid.Empty);
        contratacao.PropostaId.Should().Be(propostaId);
        contratacao.DataContratacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CriarContratacao_DeveFalhar_PropostaIdVazio()
    {
        Action act = () => new Contratacao(Guid.Empty);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*PropostaId*")
            .WithParameterName("propostaId");
    }

    [Fact]
    public void CriarContratacao_DeveGerarIdsUnicos()
    {
        var propostaId = Guid.NewGuid();

        var contratacao1 = new Contratacao(propostaId);
        var contratacao2 = new Contratacao(Guid.NewGuid());

        contratacao1.Id.Should().NotBe(contratacao2.Id);
    }

    [Fact]
    public void CriarContratacao_DeveDefinirDataContratacaoUtc()
    {
        var contratacao = new Contratacao(Guid.NewGuid());

        contratacao.DataContratacao.Kind.Should().Be(DateTimeKind.Utc);
    }

    #endregion

    #region Testes de Validação

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void CriarContratacao_DeveFalhar_GuidInvalido(string guidString)
    {
        var guidInvalido = Guid.Parse(guidString);

        Action act = () => new Contratacao(guidInvalido);

        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Testes de Imutabilidade

    [Fact]
    public void PropriedadesContratacao_DevemSerSomenteGet()
    {
        var contratacao = new Contratacao(Guid.NewGuid());

        var idProperty = typeof(Contratacao).GetProperty(nameof(Contratacao.Id));
        var propostaIdProperty = typeof(Contratacao).GetProperty(nameof(Contratacao.PropostaId));
        var dataProperty = typeof(Contratacao).GetProperty(nameof(Contratacao.DataContratacao));

        idProperty.Should().NotBeNull();
        idProperty!.CanWrite.Should().BeFalse("Id não deve ser alterável externamente");
        
        propostaIdProperty.Should().NotBeNull();
        propostaIdProperty!.CanWrite.Should().BeFalse("PropostaId não deve ser alterável externamente");
        
        dataProperty.Should().NotBeNull();
        dataProperty!.CanWrite.Should().BeFalse("DataContratacao não deve ser alterável externamente");
    }

    #endregion

    #region Testes de Comportamento

    [Fact]
    public void CriarMultiplasContratacoes_DevePermitirMesmaPropostaId()
    {
        // Embora não seja um comportamento desejável no negócio,
        // a entidade por si só permite. A validação deve estar no Use Case
        var propostaId = Guid.NewGuid();

        var contratacao1 = new Contratacao(propostaId);
        var contratacao2 = new Contratacao(propostaId);

        contratacao1.PropostaId.Should().Be(propostaId);
        contratacao2.PropostaId.Should().Be(propostaId);
        contratacao1.Id.Should().NotBe(contratacao2.Id);
    }

    #endregion
}
