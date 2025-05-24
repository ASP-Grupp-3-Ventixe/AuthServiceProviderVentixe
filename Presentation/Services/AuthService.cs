using Grpc.Core;
using Newtonsoft.Json;
using Presentation.Models;


namespace Presentation.Services;

public interface IAuthService
{
    Task<SignUpResult> SignUpAsync(SignUpFormData formData);
    Task<SignInResult> SignInAsync(SignInFormData formData);
}

public class AuthService(AccountGrpcService.AccountGrpcServiceClient accountClient, HttpClient httpClient, IConfiguration configuration) : IAuthService
{
    private readonly AccountGrpcService.AccountGrpcServiceClient _accountClient = accountClient;
    private readonly HttpClient _httpClient = httpClient;
    private readonly IConfiguration _configuration = configuration;

    public async Task<SignUpResult> SignUpAsync(SignUpFormData formData)
    {
        var request = new CreateAccountRequest
        {
            Email = formData.Email,
            Password = formData.Password
        };

        var reply = await _accountClient.CreateAccountAsync(request);

        return new SignUpResult
        {
            Succeeded = reply.Succeeded,
            Message = reply.Message,
            UserId = reply.Succeeded ? reply.UserId : null
        };
    }

    public async Task<SignInResult> SignInAsync(SignInFormData formData)
    {
        try
        {
            var credentialsRequest = new ValidateCredentialsRequest
            {
                Email = formData.Email,
                Password = formData.Password
            };

            var credentialsReply = await _accountClient.ValidateCredentialsAsync(credentialsRequest);

            if (!credentialsReply.Succeeded)
                return new SignInResult { Succeeded = false, Message = credentialsReply.Message };

            var tokenResult = await GenerateTokenAsync(credentialsReply.UserId, formData.Email);

            if (!tokenResult.Succeeded)
                return new SignInResult { Succeeded = false, Message = "Failed to generate access token" };

            return new SignInResult
            {
                Succeeded = true,
                Message = "Login successful",
                UserId = credentialsReply.UserId,
                AccessToken = tokenResult.AccessToken
            };
        }
        catch (RpcException ex)
        {
            return new SignInResult
            {
                Succeeded = false,
                Message = $"Authentication service error: {ex.Status.Detail}"
            };
        }
        catch (Exception ex)
        {
            return new SignInResult
            {
                Succeeded = false,
                Message = "Authentication service unavailable"
            };
        }
    }

    private async Task<(bool Succeeded, string? AccessToken)> GenerateTokenAsync(string userId, string email)
    {
        try
        {
            var tokenServiceUrl = _configuration["Providers:TokenServiceProvider"];
            var tokenRequest = new
            {
                userId,
                email,
                role = "User"
            };

            var json = JsonConvert.SerializeObject(tokenRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{tokenServiceUrl}/api/Auth/token", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                return (tokenResponse?.Succeeded ?? false, tokenResponse?.AccessToken);
            }

            return (false, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token generation error: {ex.Message}");
            return (false, null);
        }
    }
}