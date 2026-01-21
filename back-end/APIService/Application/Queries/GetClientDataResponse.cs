namespace APIService.Application.Queries;

public record GetClientDataResponse(
    int StatusCode,
    string? Data = null,
    string? Error = null);