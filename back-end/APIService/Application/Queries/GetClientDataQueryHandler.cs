using MediatR;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace APIService.Application.Queries;

public class GetClientDataQueryHandler : IRequestHandler<GetClientDataQuery, GetClientDataResponse>
{
    private static readonly ConcurrentDictionary<string, Task<string>> _tasks = new();
    private static long _requestCounter = 0;
    private readonly IMemoryCache _cache;

    public GetClientDataQueryHandler(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<GetClientDataResponse> Handle(GetClientDataQuery request, CancellationToken cancellationToken)
    {
        var clientId = request.ClientId;

        if (string.IsNullOrEmpty(clientId))
        {
            return new GetClientDataResponse(400, null, "Client ID is required.");
        }

        // Cache hit
        if (_cache.TryGetValue(clientId, out string? cached) && cached != null)
        {
            return new GetClientDataResponse(200, cached);
        }

        // Check ongoing task
        if (_tasks.TryGetValue(clientId, out var existingTask) && existingTask != null)
        {
            if (existingTask.IsCompletedSuccessfully)
            {
                var data = await existingTask.ConfigureAwait(false);
                _cache.Set(clientId, data, TimeSpan.FromMinutes(5));
                _tasks.TryRemove(clientId, out _);
                return new GetClientDataResponse(200, data);
            }

            if (existingTask.IsFaulted)
            {
                _tasks.TryRemove(clientId, out _);
                return new GetClientDataResponse(500, null, existingTask.Exception?.Message ?? "Computation failed.");
            }

            // Still processing
            return new GetClientDataResponse(202);
        }

        // Start new computation
        var count = Interlocked.Increment(ref _requestCounter);
        if (count % 10 == 0)
        {
            return new GetClientDataResponse(500, null, "Simulated error on every 10th request.");
        }

        var computationTask = Task.Run(async () =>
        {
            await Task.Delay(60000, cancellationToken);
            return $"Unique data for client {clientId} generated at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC.";
        }, cancellationToken);

        _tasks[clientId] = computationTask;

        return new GetClientDataResponse(202);
    }
}
