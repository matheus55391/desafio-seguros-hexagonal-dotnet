using ContratacaoService.Application.DTOs;
using ContratacaoService.Application.UseCases;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Tests.Fakes;
using FluentAssertions;
using Xunit;

namespace ContratacaoService.Tests;

public class ContratarPropostaUseCaseConcurrencyTests
{
    [Fact]
    public async Task ExecuteAsync_DuasContratacoesSimultaneas_DevePermitirApenasPrimeira()
    {
        var propostaId = Guid.NewGuid();
        var repository = new FakeContratacaoRepository();
        var client = new FakePropostaServiceClient(StatusProposta.Aprovada);
        var publisher = new FakeMessagePublisher();
        var useCase = new ContratarPropostaUseCase(repository, client, publisher);

        var request = new ContratarPropostaRequest(propostaId);

        // Executar duas contratações simultâneas
        var task1 = Task.Run(() => useCase.ExecuteAsync(request));
        var task2 = Task.Run(() => useCase.ExecuteAsync(request));

        var results = await Task.WhenAll(
            task1.ContinueWith(t => new { Success = t.IsCompletedSuccessfully, Exception = t.Exception }),
            task2.ContinueWith(t => new { Success = t.IsCompletedSuccessfully, Exception = t.Exception })
        );

        // Uma deve ter sucesso, outra deve falhar
        var successCount = results.Count(r => r.Success);
        var failureCount = results.Count(r => !r.Success);

        successCount.Should().Be(1, "apenas uma contratação deve ter sucesso");
        failureCount.Should().Be(1, "uma contratação deve falhar");

        // Verificar que apenas uma contratação foi criada
        repository.Itens.Should().HaveCount(1);
    }

    [Fact]
    public async Task ExecuteAsync_MultiplasContratacoesSimultaneasPropostasDiferentes_DeveTerSucesso()
    {
        var repository = new FakeContratacaoRepository();
        var client = new FakePropostaServiceClient(StatusProposta.Aprovada);
        var publisher = new FakeMessagePublisher();
        var useCase = new ContratarPropostaUseCase(repository, client, publisher);

        var proposta1 = Guid.NewGuid();
        var proposta2 = Guid.NewGuid();
        var proposta3 = Guid.NewGuid();
        var proposta4 = Guid.NewGuid();
        var proposta5 = Guid.NewGuid();

        // Executar 5 contratações simultâneas de propostas diferentes
        var tasks = new[]
        {
            Task.Run(() => useCase.ExecuteAsync(new ContratarPropostaRequest(proposta1))),
            Task.Run(() => useCase.ExecuteAsync(new ContratarPropostaRequest(proposta2))),
            Task.Run(() => useCase.ExecuteAsync(new ContratarPropostaRequest(proposta3))),
            Task.Run(() => useCase.ExecuteAsync(new ContratarPropostaRequest(proposta4))),
            Task.Run(() => useCase.ExecuteAsync(new ContratarPropostaRequest(proposta5)))
        };

        var results = await Task.WhenAll(tasks);

        // Todas devem ter sucesso
        results.Should().HaveCount(5);
        repository.Itens.Should().HaveCount(5);

        // Verificar que todas as propostas foram contratadas
        repository.Itens.Select(c => c.PropostaId).Should().BeEquivalentTo(new[]
        {
            proposta1, proposta2, proposta3, proposta4, proposta5
        });
    }

    [Fact]
    public async Task ExecuteAsync_ConcorrenciaAltaComMesmaPropostaId_DevePermitirApenasUma()
    {
        var propostaId = Guid.NewGuid();
        var repository = new FakeContratacaoRepository();
        var client = new FakePropostaServiceClient(StatusProposta.Aprovada);
        var publisher = new FakeMessagePublisher();
        var useCase = new ContratarPropostaUseCase(repository, client, publisher);

        var request = new ContratarPropostaRequest(propostaId);

        // Simular 10 requisições simultâneas
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(async () =>
            {
                try
                {
                    await useCase.ExecuteAsync(request);
                    return true;
                }
                catch
                {
                    return false;
                }
            }))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Apenas uma deve ter sucesso
        results.Count(r => r).Should().Be(1);
        repository.Itens.Should().HaveCount(1);
    }

    [Fact]
    public async Task ExecuteAsync_ConcorrenciaComDelays_DeveManterIntegridade()
    {
        var propostaId = Guid.NewGuid();
        var repository = new FakeContratacaoRepository();
        var client = new FakePropostaServiceClient(StatusProposta.Aprovada);
        var publisher = new FakeMessagePublisher();
        var useCase = new ContratarPropostaUseCase(repository, client, publisher);

        var request = new ContratarPropostaRequest(propostaId);

        // Executar com pequenos delays entre as chamadas
        var tasks = new List<Task<(bool Success, Exception? Error)>>();
        
        for (int i = 0; i < 5; i++)
        {
            var task = Task.Run(async () =>
            {
                await Task.Delay(Random.Shared.Next(0, 50));
                try
                {
                    await useCase.ExecuteAsync(request);
                    return (true, null);
                }
                catch (Exception ex)
                {
                    return (false, ex);
                }
            });
            tasks.Add(task);
        }

        var results = await Task.WhenAll(tasks);

        // Deve ter exatamente um sucesso
        results.Count(r => r.Success).Should().Be(1);
        
        // Outros devem ter falhado com InvalidOperationException
        results.Where(r => !r.Success)
            .Select(r => r.Error)
            .Should().AllSatisfy(ex => 
                ex.Should().BeOfType<InvalidOperationException>());

        repository.Itens.Should().HaveCount(1);
    }
}
