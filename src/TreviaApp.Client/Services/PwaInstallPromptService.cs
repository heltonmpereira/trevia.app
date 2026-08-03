using Microsoft.JSInterop;

namespace TreviaApp.Client.Services;

public interface IPwaInstallPromptService
{
    event Action? OnPromptChanged;
    bool CanInstall { get; }
    Task<bool> ShowInstallPromptAsync();
    bool IsRunningStandalone { get; }
}

public class PwaInstallPromptService : IPwaInstallPromptService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    private IJSObjectReference? _module;
    private bool _canInstall;
    private bool _isRunningStandalone;
    private DotNetObjectReference<PwaInstallPromptService>? _dotNetRef;

    public event Action? OnPromptChanged;

    public bool CanInstall
    {
        get => _canInstall;
        private set
        {
            if (_canInstall != value)
            {
                _canInstall = value;
                OnPromptChanged?.Invoke();
            }
        }
    }

    public bool IsRunningStandalone => _isRunningStandalone;

    public PwaInstallPromptService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _moduleTask = new Lazy<Task<IJSObjectReference>>(() =>
            _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/pwa-install.js").AsTask());
    }

    public async Task InitializeAsync()
    {
        try
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            _module = await _moduleTask.Value;
            _isRunningStandalone = await _module.InvokeAsync<bool>("isStandalone");
            CanInstall = await _module.InvokeAsync<bool>("registerInstallListener", _dotNetRef);
        }
        catch (JSException)
        {
            CanInstall = false;
        }
    }

    [JSInvokable]
    public void SetCanInstall(bool value)
    {
        CanInstall = value;
    }

    public async Task<bool> ShowInstallPromptAsync()
    {
        if (_module == null) return false;
        return await _module.InvokeAsync<bool>("showInstallPrompt");
    }

    public async ValueTask DisposeAsync()
    {
        if (_module != null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (JSException) { }
        }
        _dotNetRef?.Dispose();
    }
}
