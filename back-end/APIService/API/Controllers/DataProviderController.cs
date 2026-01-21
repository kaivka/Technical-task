using APIService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace APIService.API.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientDataProviderController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientDataProviderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetClientData([FromQuery] string clientId)
    {
        var query = new GetClientDataQuery(clientId);
        var response = await _mediator.Send(query);

        return response.StatusCode switch
        {
            200 => Ok(response.Data),
            202 => Accepted($"/api/clients?clientId={clientId}"),
            400 => BadRequest(response.Error),
            500 => StatusCode(500, response.Error),
            _ => StatusCode(500, "Unexpected status")
        };
    }
}