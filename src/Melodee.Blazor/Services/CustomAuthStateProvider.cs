﻿using Microsoft.AspNetCore.Components.Authorization;

namespace Melodee.Blazor.Services;

/// <summary>
///     Updates the Blazor backend authentication state when the user changes.
/// </summary>
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private AuthenticationState authenticationState;

    public CustomAuthStateProvider(IAuthService service)
    {
        authenticationState = new AuthenticationState(service.CurrentUser);

        service.UserChanged += newUser =>
        {
            authenticationState = new AuthenticationState(newUser);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(newUser)));
        };
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(authenticationState);
    }
}