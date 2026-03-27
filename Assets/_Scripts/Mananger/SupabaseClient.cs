using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class SupabaseClient
{
    // projectUrl: https://lrjdyfoumqxmtobozmdh.supabase.co
    // anonKey: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9... (JWT — see SupabaseConfig asset)

    static TrSupabaseConfig _config;
    static TrSupabaseConfig Config
    {
        get
        {
            if (_config == null)
            {
                _config = Resources.Load<TrSupabaseConfig>("SupabaseConfig");
                if (_config == null)
                    Debug.LogError("SupabaseConfig asset not found in Resources/");
            }
            return _config;
        }
    }

    static string ProjectUrl => Config?.projectUrl ?? "";
    static string AnonKey    => Config?.anonKey    ?? "";

    public static string AccessToken { get; set; } = "";

    // REST API URL 헬퍼
    public static string RestUrl(string table, string query = "") =>
        $"{ProjectUrl}/rest/v1/{table}{(query != "" ? "?" + query : "")}";

    public static string AuthUrl(string path) =>
        $"{ProjectUrl}/auth/v1/{path}";

    // 공통 헤더 설정
    public static void SetHeaders(UnityWebRequest req, bool withAuth = true)
    {
        req.SetRequestHeader("apikey", AnonKey);
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Prefer", "return=representation");
        if (withAuth && AccessToken != "")
            req.SetRequestHeader("Authorization", $"Bearer {AccessToken}");
    }

    // GET
    public static IEnumerator Get(string url, Action<string> onSuccess, Action<string> onError = null)
    {
        using var req = UnityWebRequest.Get(url);
        SetHeaders(req);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(req.downloadHandler.text);
        else
            onError?.Invoke($"[{req.responseCode}] {req.error} | {req.downloadHandler.text}");
    }

    // POST
    public static IEnumerator Post(string url, string json, Action<string> onSuccess, Action<string> onError = null)
    {
        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        SetHeaders(req);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(req.downloadHandler.text);
        else
            onError?.Invoke($"[{req.responseCode}] {req.error} | {req.downloadHandler.text}");
    }

    // PATCH
    public static IEnumerator Patch(string url, string json, Action<string> onSuccess, Action<string> onError = null)
    {
        using var req = new UnityWebRequest(url, "PATCH");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        SetHeaders(req);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(req.downloadHandler.text);
        else
            onError?.Invoke($"[{req.responseCode}] {req.error} | {req.downloadHandler.text}");
    }

    // UPSERT
    public static IEnumerator Upsert(string url, string json, Action<string> onSuccess, Action<string> onError = null)
    {
        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        SetHeaders(req);
        // Upsert 전용: Prefer 헤더를 merge-duplicates로 덮어쓰기
        req.SetRequestHeader("Prefer", "resolution=merge-duplicates");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(req.downloadHandler.text);
        else
            onError?.Invoke($"[{req.responseCode}] {req.error} | {req.downloadHandler.text}");
    }

    // DELETE
    public static IEnumerator Delete(string url, Action onSuccess, Action<string> onError = null)
    {
        using var req = UnityWebRequest.Delete(url);
        SetHeaders(req);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke();
        else
            onError?.Invoke($"[{req.responseCode}] {req.error} | {req.downloadHandler.text}");
    }
}
