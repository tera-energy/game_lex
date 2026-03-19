using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class SupabaseClient
{
    public const string ProjectUrl = "https://lrjdyfoumqxmtobozmdh.supabase.co";
    public const string AnonKey    = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImxyamR5Zm91bXF4bXRvYm96bWRoIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzM4MTc3NTUsImV4cCI6MjA4OTM5Mzc1NX0.ooLm3UEK0YkZeTico72OAVlfEYUOfj5AAGE1wXwU0B0";

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
            onError?.Invoke(req.error);
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
            onError?.Invoke(req.error);
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
            onError?.Invoke(req.error);
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
            onError?.Invoke(req.error);
    }
}
