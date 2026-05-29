namespace TastyMealPlanner.Helpers;

/// <summary>Provides reusable input validation methods returning (isValid, errorMessage) tuples.</summary>
public static class ValidationHelper
{
    public static (bool IsValid, string? Error) ValidateRequired(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (false, $"{fieldName} is required.");
        return (true, null);
    }

    public static (bool IsValid, string? Error) ValidateMinLength(string? value, string fieldName, int minLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length < minLength)
            return (false, $"{fieldName} must be at least {minLength} characters.");
        return (true, null);
    }

    public static (bool IsValid, string? Error) ValidateMaxLength(string? value, string fieldName, int maxLength)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Trim().Length > maxLength)
            return (false, $"{fieldName} must be no more than {maxLength} characters.");
        return (true, null);
    }

    /// <summary>Validates a shopping item: name is required and must be ≤50 characters.</summary>
    public static (bool IsValid, string? Error) ValidateShoppingItem(string? name, string? quantity)
    {
        var (nameValid, nameError) = ValidateRequired(name, "Item name");
        if (!nameValid) return (false, nameError);

        var (lengthValid, lengthError) = ValidateMaxLength(name, "Item name", 50);
        if (!lengthValid) return (false, lengthError);

        if (!string.IsNullOrWhiteSpace(quantity))
        {
            var (qtyValid, qtyError) = ValidateMaxLength(quantity, "Quantity", 20);
            if (!qtyValid) return (false, qtyError);
        }

        return (true, null);
    }
}
