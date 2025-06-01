
using System.Net;
using Grpc.Core;
using Presentation;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Presentation.Models;
using Presentation.Services;
using Xunit;
using static Presentation.AccountGrpcService;

public class AuthServiceTests
{
    private static AsyncUnaryCall<T> CreateAsyncUnaryCall<T>(T response) where T : class
    {
        return new AsyncUnaryCall<T>(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });
    }

    [Fact]
    public async Task SignInAsync_ShouldReturnSuccess_WhenCredentialsValidAndTokenCreated()
    {
        // Arrange
        var grpcMock = new Mock<AccountGrpcServiceClient>();
        grpcMock.Setup(x => x.ValidateCredentialsAsync(It.IsAny<ValidateCredentialsRequest>(), null, null, It.IsAny<CancellationToken>()))
                .Returns(CreateAsyncUnaryCall(new ValidateCredentialsReply { Succeeded = true, UserId = "123", Message = "OK" }));

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new TokenResponse { Succeeded = true, AccessToken = "token123" }))
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Providers:TokenServiceProvider"]).Returns("http://mockurl/");

        var authService = new AuthService(grpcMock.Object, httpClient, configMock.Object);

        var formData = new SignInFormData { Email = "test@example.com", Password = "pwd123" };

        // Act
        var result = await authService.SignInAsync(formData);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("123", result.UserId);
        Assert.Equal("token123", result.AccessToken);
    }

    [Fact]
    public async Task SignInAsync_ShouldReturnFailure_WhenTokenGenerationFails()
    {
        // Arrange
        var grpcMock = new Mock<AccountGrpcService.AccountGrpcServiceClient>();
        grpcMock.Setup(x => x.ValidateCredentialsAsync(It.IsAny<ValidateCredentialsRequest>(), null, null, It.IsAny<CancellationToken>()))
                .Returns(new AsyncUnaryCall<ValidateCredentialsReply>(
                    Task.FromResult(new ValidateCredentialsReply { Succeeded = true, UserId = "123", Message = "OK" }),
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { }));

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent(
                    JsonConvert.SerializeObject(new { succeeded = false }),
                    System.Text.Encoding.UTF8,
                    "application/json")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Providers:TokenServiceProvider"]).Returns("http://mockurl/");

        var authService = new AuthService(grpcMock.Object, httpClient, configMock.Object);

        var formData = new SignInFormData { Email = "test@example.com", Password = "pwd123" };

        // Act
        var result = await authService.SignInAsync(formData);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Failed to generate access token", result.Message);
        Assert.Null(result.AccessToken);
    }

}
