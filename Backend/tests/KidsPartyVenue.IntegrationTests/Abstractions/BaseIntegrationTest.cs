using Bogus;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KidsPartyVenue.IntegrationTests.Abstractions;

[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest : IDisposable
{
    protected static readonly Faker Faker = new();
    private readonly IServiceScope _scope;
    protected readonly KidsPartyVenueDbContext DbContext;
    protected readonly HttpClient HttpClient;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<KidsPartyVenueDbContext>();
        // The refresh cookie is Secure and the test server is http, so the cookie container would never send it back.
        // Tests read Set-Cookie and send the Cookie header themselves.
        HttpClient = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });
    }

    protected async Task<Result<TResult>> SendCommand<TCommand, TResult>(TCommand command)
        where TCommand : ICommand<TResult>
    {
        ICommandHandler<TCommand, TResult> handler = _scope.ServiceProvider
            .GetRequiredService<ICommandHandler<TCommand, TResult>>();

        return await handler.Handle(command, CancellationToken.None);
    }

    public async Task<Result> SendCommand<TCommand>(TCommand command)
        where TCommand : ICommand
    {
        ICommandHandler<TCommand> handler = _scope.ServiceProvider
            .GetRequiredService<ICommandHandler<TCommand>>();

        return await handler.Handle(command, CancellationToken.None);
    }

    protected async Task<Result<TResult>> SendQuery<TQuery, TResult>(TQuery query)
        where TQuery : IQuery<TResult>
    {
        IQueryHandler<TQuery, TResult> handler = _scope.ServiceProvider
            .GetRequiredService<IQueryHandler<TQuery, TResult>>();

        return await handler.Handle(query, CancellationToken.None);
    }

    protected async Task CleanDatabaseAsync()
    {
        await DbContext.Database.ExecuteSqlRawAsync(
            """
            
            """);
    }

    public void Dispose()
    {
        HttpClient.Dispose();
        _scope.Dispose();
    }
}
