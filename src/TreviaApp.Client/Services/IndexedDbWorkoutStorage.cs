using Microsoft.JSInterop;
using System.Text.Json;

namespace TreviaApp.Client.Services;

public class IndexedDbWorkoutStorage : IWorkoutOfflineStorage, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    private readonly string _fallbackPrefix = "treviaapp_workout_";

    public IndexedDbWorkoutStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _moduleTask = new Lazy<Task<IJSObjectReference>>(() =>
            _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/idb-storage.js").AsTask());
    }

    public async Task SaveCurrentWorkoutAsync(string userId, WorkoutInProgressData data)
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("saveWorkout", userId, data);
        }
        catch (JSException jsEx) when (jsEx.Message.Contains("not supported") || jsEx.Message.Contains("IndexedDB"))
        {
            await SaveToLocalStorageAsync(userId, data);
        }
        catch
        {
            try { await SaveToLocalStorageAsync(userId, data); } catch { }
        }
    }

    public async Task<WorkoutInProgressData?> LoadCurrentWorkoutAsync(string userId)
    {
        try
        {
            var module = await _moduleTask.Value;
            var result = await module.InvokeAsync<WorkoutInProgressData?>("loadWorkout", userId);
            return result;
        }
        catch
        {
            return await LoadFromLocalStorageAsync(userId);
        }
    }

    public async Task ClearCurrentWorkoutAsync(string userId)
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("clearWorkout", userId);
        }
        catch { }
        try { await ClearFromLocalStorageAsync(userId); } catch { }
    }

    public async Task<bool> HasSavedWorkoutAsync(string userId)
    {
        try
        {
            var module = await _moduleTask.Value;
            return await module.InvokeAsync<bool>("hasWorkout", userId);
        }
        catch
        {
            return await HasInLocalStorageAsync(userId);
        }
    }

    private async Task SaveToLocalStorageAsync(string userId, WorkoutInProgressData data)
    {
        var json = JsonSerializer.Serialize(data);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", _fallbackPrefix + userId, json);
    }

    private async Task<WorkoutInProgressData?> LoadFromLocalStorageAsync(string userId)
    {
        var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", _fallbackPrefix + userId);
        if (string.IsNullOrEmpty(json)) return null;
        try { return JsonSerializer.Deserialize<WorkoutInProgressData>(json); }
        catch { return null; }
    }

    private async Task ClearFromLocalStorageAsync(string userId)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", _fallbackPrefix + userId);
    }

    private async Task<bool> HasInLocalStorageAsync(string userId)
    {
        var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", _fallbackPrefix + userId);
        return !string.IsNullOrEmpty(json);
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            try { await (await _moduleTask.Value).DisposeAsync(); }
            catch (JSException) { }
        }
    }
}
