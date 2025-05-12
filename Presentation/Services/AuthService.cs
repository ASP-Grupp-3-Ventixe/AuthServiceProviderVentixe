using Presentation.Models;

namespace Presentation.Services;

public interface IAuthService
{
    Task<SignUpResult> SignUpAsync(SignUpFormData formData);
    Task<SignInResult> SignInAsync(SignInFormData formData);
}

public class AuthService(AccountGrpcService.AccountGrpcServiceClient accountClient) : IAuthService
{
    private readonly AccountGrpcService.AccountGrpcServiceClient _accountClient = accountClient;

    public async Task<SignUpResult> SignUpAsync(SignUpFormData formData)
    {
        var request = new CreateAccountRequest
        {
            Email = formData.Email,
            Password = formData.Password
        };

        var reply = await _accountClient.CreateAccountAsync(request);

        return reply.Succeeded
            ? new SignUpResult { Succeeded = reply.Succeeded, Message = reply.Message, UserId = reply.UserId }
            : new SignUpResult { Succeeded = reply.Succeeded, Message = reply.Message };
        
    }

    public async Task<SignInResult> SignInAsync(SignInFormData formData)
    {
        var request = new ValidateCredentialsRequest
        {
            Email = formData.Email,
            Password = formData.Password
        };

        var reply = await _accountClient.ValidateCredentialsAsync(request);
        if (!reply.Succeeded)
            return new SignInResult { Succeeded = false, Message = reply.Message };

        return new SignInResult
        { 
            Succeeded = reply.Succeeded,
            Message = reply.Message,
            UserId = reply.UserId,
        };

    }
}

