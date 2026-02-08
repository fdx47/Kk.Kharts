using CommunityToolkit.Mvvm.ComponentModel;

namespace Kk.Kharts.Maui.ViewModels;

/// <summary>
/// Base class for all ViewModels with common functionality.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;

    protected void SetError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    protected void ClearError()
    {
        ErrorMessage = null;
        HasError = false;
    }

    protected async Task ExecuteAsync(Func<Task> operation, string? errorMessage = null)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ClearError();
            await operation();
        }
        catch (Exception ex)
        {
            SetError(errorMessage ?? ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected async Task<T?> ExecuteAsync<T>(Func<Task<T>> operation, string? errorMessage = null)
    {
        if (IsBusy) return default;

        try
        {
            IsBusy = true;
            ClearError();
            return await operation();
        }
        catch (Exception ex)
        {
            SetError(errorMessage ?? ex.Message);
            return default;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
