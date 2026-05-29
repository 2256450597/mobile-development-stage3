namespace TastyMealPlanner.Helpers;

public static class ErrorHandler
{
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
