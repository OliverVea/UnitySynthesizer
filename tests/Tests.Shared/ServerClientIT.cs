using Microsoft.Extensions.DependencyInjection;
using Synthesizer.Shared.Extensions;
using Synthesizer.Shared.Infrastructure;

namespace Tests.Shared;

public abstract class ServerClientIT
{
    private IServiceProvider? _serverServiceProvider;
    protected IServiceProvider ServerServiceProvider => _serverServiceProvider ?? throw new NullReferenceException();
    
    private IServiceProvider? _clientServiceProvider;
    protected IServiceProvider ClientServiceProvider => _clientServiceProvider ?? throw new NullReferenceException();
    
    private IServiceProvider BuildServerServiceProvider()
    {
        return new ServiceCollection()
            .AddServerServices()
            .BuildServiceProvider();
    }
    
    private IServiceProvider BuildClientServiceProvider()
    {
        return new ServiceCollection()
            .AddClientServices()
            .BuildServiceProvider();
    }

    [SetUp]
    public void Setup()
    {
        _serverServiceProvider = BuildServerServiceProvider();
        _clientServiceProvider = BuildClientServiceProvider();
        
        var clientNamedPipeStream = ClientServiceProvider.GetRequiredService<INamedPipeStream>();
        var serverNamedPipeStream = ServerServiceProvider.GetRequiredService<INamedPipeStream>();
        
        var clientTask = clientNamedPipeStream.ConnectAsync(CancellationToken.None);
        var serverTask = serverNamedPipeStream.ConnectAsync(CancellationToken.None);
        
        Task.WhenAll(clientTask, serverTask).Wait();
    }
    
    [TearDown]
    public void TearDown()
    {
        var clientNamedPipeStream = ClientServiceProvider.GetRequiredService<INamedPipeStream>();
        var serverNamedPipeStream = ServerServiceProvider.GetRequiredService<INamedPipeStream>();
        
        var clientTask = clientNamedPipeStream.DisposeAsync().AsTask();
        var serverTask = serverNamedPipeStream.DisposeAsync().AsTask();
        
        Task.WhenAll(clientTask, serverTask).Wait();
    }
}