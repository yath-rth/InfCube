using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using TMPro;

[RequireComponent(typeof(ConnectionManager))]
public class Auth : MonoBehaviour
{
    public static Auth instance;

    [Header("UI References")]
    [SerializeField] GameObject authUI;
    [SerializeField] TMP_InputField usernameInput;
    [SerializeField] TMP_InputField passwordInput;

    [Header("Backend Configuration")]
    [Tooltip("Base URL for the backend HTTP API (not WebSocket)")]
    public string baseUrl = "http://localhost:8080";

    [Header("Auth State")]
    public string token { get; private set; }
    public string refreshToken { get; private set; }
    public string username { get; private set; }
    public bool isAuthenticated => !string.IsNullOrEmpty(token);

    ConnectionManager connMgr;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        connMgr = GetComponent<ConnectionManager>();
    }

    /// <summary>
    /// Register a new account. Returns true on success, false on failure.
    /// </summary>
    public IEnumerator Register(string usernameStr, string passwordStr)
    {
        var body = new { username = usernameStr, password = passwordStr };
        var json = JsonConvert.SerializeObject(body);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (var request = new UnityEngine.Networking.UnityWebRequest(baseUrl + "/auth/register", "POST"))
        {
            request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Register failed: {request.error}");
                OnAuthFailed(request.error);
                yield break;
            }

            var response = JsonConvert.DeserializeObject<AuthResponse>(request.downloadHandler.text);
            token = response.token;
            refreshToken = response.refreshToken;
            username = usernameStr;

            Debug.Log($"Registered successfully. Token: {token}");
            OnAuthenticated(token, refreshToken, username);

            authUI.SetActive(false);
        }
    }

    /// <summary>
    /// Login with existing credentials. Returns true on success, false on failure.
    /// </summary>
    public IEnumerator Login(string usernameStr, string passwordStr)
    {
        var body = new { username = usernameStr, password = passwordStr };
        var json = JsonConvert.SerializeObject(body);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (var request = new UnityEngine.Networking.UnityWebRequest(baseUrl + "/auth/login", "POST"))
        {
            request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Login failed: {request.error}");
                Debug.LogError($"Response body: {request.downloadHandler.text}"); // add this
                OnAuthFailed(request.error);
                yield break;
            }

            var response = JsonConvert.DeserializeObject<AuthResponse>(request.downloadHandler.text);
            token = response.token;
            refreshToken = response.refreshToken;
            username = usernameStr;

            Debug.Log($"Logged in successfully. Token: {token}");
            OnAuthenticated(token, refreshToken, username);

            authUI.SetActive(false);
        }
    }

    /// <summary>
    /// Logout and clear auth state.
    /// </summary>
    public IEnumerator Logout()
    {
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogWarning("No active token to logout.");
            ClearAuthState();
            yield break;
        }

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes("{}");

        using (var request = new UnityEngine.Networking.UnityWebRequest(baseUrl + "/auth/logout", "POST"))
        {
            request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + token);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Logout server call failed: {request.error} — clearing local state anyway.");
            }
            else
            {
                Debug.Log("Logged out successfully.");
            }

            ClearAuthState();
            authUI.SetActive(true);
        }
    }

    /// <summary>
    /// Refresh the JWT token using the stored refresh token.
    /// </summary>
    public IEnumerator RefreshToken()
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            Debug.LogWarning("No refresh token available.");
            ClearAuthState();
            yield break;
        }

        var body = new { token = refreshToken };
        var json = JsonConvert.SerializeObject(body);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (var request = new UnityEngine.Networking.UnityWebRequest(baseUrl + "/auth/refresh", "POST"))
        {
            request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Token refresh failed: {request.error}");
                ClearAuthState();
                yield break;
            }

            var response = JsonConvert.DeserializeObject<AuthResponse>(request.downloadHandler.text);
            token = response.token;
            refreshToken = response.refreshToken;

            Debug.Log("Token refreshed successfully.");
        }
    }

    /// <summary>
    /// Call before joining a WebSocket game session — attaches auth info as query params.
    /// The backend can read userId/username from the handshake attributes (see CustomHandshakeInterceptor).
    /// </summary>
    public string GetAuthenticatedGameUrl(string originalWsUrl)
    {
        if (string.IsNullOrEmpty(token)) return originalWsUrl;

        // If no explicit URL is passed, use the one from ConnectionManager
        var wsUrl = string.IsNullOrEmpty(originalWsUrl) ? connMgr.url : originalWsUrl;

        bool hasQuery = wsUrl.Contains("?");
        var sep = hasQuery ? "&" : "?";

        // The CustomHandshakeInterceptor currently reads a 'token' query param (commented out, but ready to enable).
        return $"{wsUrl}{sep}userId={username}&token={Uri.EscapeDataString(token)}";
    }

    // ── Coroutine entry points for UI buttons ───────────────────────────────

    public void OnLoginButton()
    {
        if (string.IsNullOrEmpty(usernameInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            Debug.LogWarning("Username and password fields must be filled.");
            return;
        }
        StartCoroutine(Login(usernameInput.text, passwordInput.text));
    }

    public void OnRegisterButton()
    {
        if (string.IsNullOrEmpty(usernameInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            Debug.LogWarning("Username and password fields must be filled.");
            return;
        }
        StartCoroutine(Register(usernameInput.text, passwordInput.text));
    }

    public void OnLogoutButton()
    {
        StartCoroutine(Logout());
    }

    // ── Internal helpers ───────────────────────────────────────────────────

    private void ClearAuthState()
    {
        token = null;
        refreshToken = null;
        username = null;
        Debug.Log("Auth state cleared.");
    }

    public event System.Action<string, string, string> OnAuthenticatedEvent;
    public event System.Action<string> OnAuthFailedEvent;

    private void OnAuthenticated(string tok, string refreshTok, string user)
    {
        OnAuthenticatedEvent?.Invoke(tok, refreshTok, user);
    }

    private void OnAuthFailed(string msg)
    {
        OnAuthFailedEvent?.Invoke(msg);
    }

    // ── DTOs ───────────────────────────────────────────────────────────────

    [System.Serializable]
    public class AuthResponse
    {
        public string token;
        public string refreshToken; // matches Java backend field name exactly

    }
}
