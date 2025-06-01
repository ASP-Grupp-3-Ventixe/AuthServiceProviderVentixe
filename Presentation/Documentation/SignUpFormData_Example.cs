using Presentation.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Presentation.Documentation;

public class SignUpFormData_Example : IExamplesProvider<SignUpFormData>
{
    public SignUpFormData GetExamples() => new()
    {
        Email = "kaspar@domain.com",
        Password = "BytMig123!",
    };

}
