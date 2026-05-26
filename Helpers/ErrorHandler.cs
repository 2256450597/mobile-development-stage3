namespace TastyMealPlanner.Helpers;

/// <summary>
/// Centralised error handling for hardware operations.
/// Provides consistent user-facing error messages.
/// </summary>
public static class ErrorHandler
{
    /// <summary>
    /// Displays an error alert to the user with consistent formatting.
    /// </summary>
    public static async Task ShowErrorAsync(string title, string message)
    {
        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlert(
                $"Error: {title}",
                message,
                "OK");
        }
    }

    /// <summary>
    /// Handles exceptions from hardware operations with user-friendly messages.
    /// </summary>
    public static async Task<bool> HandleHardwareErrorAsync(Exception ex, string operation)
    {
        var message = ex switch
        {
            PermissionException => "Permission was denied. Please enable the required permission in device settings.",
            UnauthorizedAccessException => "Access was denied. Please check your device permissions.",
            TimeoutException => "The operation timed out. Please try again.",
            InvalidOperationException ioe => ioe.Message,
            _ => $"An unexpected error occurred while {operation}. Please try again."
        };

        await ShowErrorAsync(operation, message);
        return false;
    }

    /// <summary>
    /// Wraps a hardware operation with standardised error handling.
    /// </summary>
    public static async Task<bool> WrapHardwareOperationAsync(
        Func<Task> operation, string operationName)
    {
        try
        {
            await operation();
            return true;
        }
        catch (Exception ex)
        {
            await HandleHardwareErrorAsync(ex, operationName);
            return false;
        }
    }
}
