using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.Common.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class NotEmptyGuidAttribute() : ValidationAttribute(DefaultErrorMessage)
{
    public const string DefaultErrorMessage = "The {0} field must not be empty";

    public override bool IsValid(object? value)
    {
        //NotEmpty doesn't necessarily mean required
        if (value is null)
            return true;

        switch (value)
        {
            case Guid guid:
                return guid != Guid.Empty;
            default:
                return true;
        }
    }
}
