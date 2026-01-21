using APIService.API.Controllers;
using APIService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace APIService.Tests;

public class ClientDataProviderControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ClientDataProviderController _controller;

    public ClientDataProviderControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ClientDataProviderController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetClientData_With200StatusCode_ReturnsOkResult()
    {
        // Arrange
        var clientId = "test-client";
        var testData = "Test Data";
        var response = new GetClientDataResponse(200, testData, null);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetClientDataQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetClientData(clientId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(testData, okResult.Value);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetClientData_With202StatusCode_ReturnsAcceptedResult()
    {
        // Arrange
        var clientId = "test-client";
        var response = new GetClientDataResponse(202, null, null);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetClientDataQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetClientData(clientId);

        // Assert
        var acceptedResult = Assert.IsType<AcceptedResult>(result);
        Assert.Equal(202, acceptedResult.StatusCode);
        Assert.Contains(clientId, acceptedResult.Location);
    }

    [Fact]
    public async Task GetClientData_With400StatusCode_ReturnsBadRequestResult()
    {
        // Arrange
        var clientId = "";
        var errorMessage = "Client ID is required.";
        var response = new GetClientDataResponse(400, null, errorMessage);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetClientDataQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetClientData(clientId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(errorMessage, badRequestResult.Value);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetClientData_With500StatusCode_ReturnsInternalServerError()
    {
        // Arrange
        var clientId = "test-client";
        var errorMessage = "Simulated error on every 10th request.";
        var response = new GetClientDataResponse(500, null, errorMessage);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetClientDataQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetClientData(clientId);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal(errorMessage, statusCodeResult.Value);
    }

    [Fact]
    public async Task GetClientData_WithUnexpectedStatusCode_ReturnsInternalServerError()
    {
        // Arrange
        var clientId = "test-client";
        var response = new GetClientDataResponse(999, null, null);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetClientDataQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetClientData(clientId);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Unexpected status", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetClientData_SendsCorrectQueryToMediator()
    {
        // Arrange
        var clientId = "test-client";
        var response = new GetClientDataResponse(200, "data", null);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetClientDataQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _controller.GetClientData(clientId);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<GetClientDataQuery>(q => q.ClientId == clientId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
