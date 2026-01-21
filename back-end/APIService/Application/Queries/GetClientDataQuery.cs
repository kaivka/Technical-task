using MediatR;

namespace APIService.Application.Queries;

public record GetClientDataQuery(string ClientId) : IRequest<GetClientDataResponse>;