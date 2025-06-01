using Presentation.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Presentation.Documentation;

public class SignInFormData_Example : IExamplesProvider<SignInFormData>
{
    public SignInFormData GetExamples() => new()
    {
        Email = "kaspar@domain.com",
        Password = "BytMig123!",
    };

}
