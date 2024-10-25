using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Melodee.Services;

public class UserService : IDisposable
{
    private readonly AuthenticationStateProvider _stateProvider;
    private Task _task = Task.CompletedTask;

    public event EventHandler<UserChangedEventArgs>? UserChanged;

    public ClaimsPrincipal User { get; set; } = new ClaimsPrincipal();

    public UserService(AuthenticationStateProvider authenticationStateProvider)
    {
        _stateProvider = authenticationStateProvider;
        _stateProvider.AuthenticationStateChanged += this.OnUserChanged;
        // get the current user. as this is an async method we just assign it to a class variable
        _task = this.GetUserAsync(_stateProvider.GetAuthenticationStateAsync());
    }

    public async ValueTask<ClaimsPrincipal> GetUserAsync()
    {
        // probably complete, but we don't know for sure so always await it
        await _task;
        return User;
    }

    private void OnUserChanged(Task<AuthenticationState> task)
    {
        // As this is a fire and forget event we assign the new task to our class task
        // which we can await if we need to
        _task = this.GetChangedUserAsync(task);
    }

    private async Task GetChangedUserAsync(Task<AuthenticationState> task)
    {
        // get the user
        await this.GetUserAsync(task);
        // raise our fire and forget event now we've got the new user
        this.UserChanged?.Invoke(this, new UserChangedEventArgs(User));
    }

    private async Task GetUserAsync(Task<AuthenticationState> task)
    {
        // Reset the user to a new ClaimsPrincipal which is effectivity no user
        this.User = new ClaimsPrincipal();
        
        // get the state object
        var state = await task;

        // update the user
        if (state is not null)
            this.User = state.User;
    }

    public void Dispose()
         => _stateProvider.AuthenticationStateChanged -= this.OnUserChanged;
}

public class UserChangedEventArgs : EventArgs
{
    public ClaimsPrincipal? User { get; private set; }

    public UserChangedEventArgs(ClaimsPrincipal? user)
        => User = user;
}

For reference, here's the code for 
