using Unity.Services.Authentication;
using UnityEngine;
using Unity.Services.Core;
using System.Threading.Tasks;
using System;
public class AuthManager : MonoBehaviour
{
    public static AuthManager instance;
    public static bool userAuthenticated = false;
    
    private void Awake()
    {
        if(instance != null) Destroy(this);
        instance = this;
    }
    private async void Start()
    {
        await SignInCachedUserAsync();
        SetupEvents();
    }
    public void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += OnSignIn;
        AuthenticationService.Instance.SignInFailed += OnSignInFailed;
        AuthenticationService.Instance.SignedOut += OnSignOut;
        AuthenticationService.Instance.Expired += OnSessionExpired;
    }

    public async Task SignInCachedUserAsync()
    {
        if (!AuthenticationService.Instance.SessionTokenExists)
        {
            return;
        }

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }
    public void OnSignIn() {
        Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
        Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");
        userAuthenticated = true;
    }
    public void OnSignInFailed(RequestFailedException err)
    {
        Debug.LogError(err);
        userAuthenticated = false;
    }
    public void OnSignOut() {
        Debug.Log("Player signed out.");
        userAuthenticated = false;
    }
    public void OnSessionExpired() {
        Debug.Log("Player session expired.");
        userAuthenticated = false;
    }

    [ContextMenu("Test Sign Up")]
    private void TestSignUp() {
        SignUp("admin","Admin@123");
    }

    [ContextMenu("Test Sign In")]
    private void TestSignIn()
    {
        SignIn("admin", "Admin@123");
    }

    [ContextMenu("Test Sign Out")]
    private void TestSignOut()
    {
        SignOut();
    }
    public async void SignUp(string username, string password)
    {
        
        
        await SignUpWithUsernamePasswordAsync(username, password);
        await UpdatePlayerNameAsync(NameGenerator.instance.GenerateName());
    }
   

    public async void SignIn(string username, string password)
    {
        await SignInWithUsernamePasswordAsync(username, password);
    }

    public void SignOut()
    {
        AuthenticationService.Instance.SignOut(true);
    }

    public async void UpdatePassword(string currentPassword, string newPassword)
    {
        await UpdatePasswordAsync(currentPassword, newPassword);
    }
    public async void UpdatePlayerName(string newName)
    {
        await UpdatePlayerNameAsync(newName);
    }
    public async Task UpdatePlayerNameAsync(string newName)
    {
        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
            Debug.Log("Player name updated.");
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }
    async Task SignUpWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            Debug.Log("SignUp is successful.");
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }

    async Task SignInWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            Debug.Log("SignIn is successful.");
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }
    async Task UpdatePasswordAsync(string currentPassword, string newPassword)
    {
        try
        {
            await AuthenticationService.Instance.UpdatePasswordAsync(currentPassword, newPassword);
            Debug.Log("Password updated.");
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }

}
