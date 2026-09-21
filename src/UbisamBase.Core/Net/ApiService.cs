using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using UbisamBase.Core.Bootstrap;
using UbisamBase.Core.Logging;

namespace UbisamBase.Core.Net;

/// <summary>인증을 어떻게 붙이는지.</summary>
public enum ApiAuthMode
{
    /// <summary>인증 없음(사내망 IP 제한 등). 주소만으로 부른다.</summary>
    None,

    /// <summary>Authorization: Bearer {토큰}</summary>
    BearerHeader,

    /// <summary>{HeaderName}: {토큰} — 사내 API가 고유 헤더를 쓰는 경우.</summary>
    CustomHeader,

    /// <summary>Cookie: {토큰} — 세션 쿠키를 그대로 넘기는 경우.</summary>
    Cookie
}

/// <summary>
/// 사내 API를 대신 호출해 응답 본문(JSON 문자열)만 돌려주는 공용 서비스.
///
/// <para><b>왜 플랫폼에 있나</b> — 토큰이 필요한 API는 화면(WebView2 등)에서 직접 부르면 토큰이 그
/// 문서에 섞여 들어간다. 앱이 대신 부르고 <b>숫자만</b> 넘기면 토큰은 앱 설정 파일에만 남는다.
/// 내보낸 HTML·.ub 같은 파일에는 절대 들어가지 않는다. 브라우저에서 막히는 CORS도 우회된다.</para>
///
/// <para>토큰은 <c>D:\UbisamConfig\Settings\ApiSettings.json</c>에만 저장되고, 로그에도 남기지 않는다.</para>
/// </summary>
public sealed class ApiService
{
    private static readonly Logger Log = new("Api");
    private static readonly HttpClient Http = new();
    private static ApiService? current;

    private readonly Dictionary<string, CacheEntry> cache = new(StringComparer.OrdinalIgnoreCase);

    public static ApiService Current => current ??= new ApiService();

    public ApiSettings Settings { get; private set; } = ApiSettings.Load();

    /// <summary>설정 파일을 다시 읽는다(설정 화면에서 토큰을 바꾼 뒤 등).</summary>
    public void ReloadSettings() => Settings = ApiSettings.Load();

    /// <summary>
    /// 주소를 호출해 응답 본문을 그대로 돌려준다. 인증 헤더는 설정에 따라 이 안에서 붙으므로
    /// 부르는 쪽(모듈·편집기)은 토큰을 몰라도 된다.
    ///
    /// 같은 주소를 <see cref="ApiSettings.MinIntervalMs"/> 안에 다시 부르면 서버를 때리지 않고
    /// 직전 응답을 돌려준다 — 화면 갱신 주기가 짧아도 호출 제한에 걸리지 않게 하기 위한 것이다.
    /// 실패하면 예외를 던진다(메시지는 사용자에게 보여줘도 되는 내용만 담는다).
    /// </summary>
    public async Task<string> FetchAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("주소가 비어 있습니다.", nameof(url));
        }

        if (TryGetCached(url, out var cached))
        {
            return cached;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyAuth(request);

        try
        {
            using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(1, Settings.TimeoutSeconds)));
            using var response = await Http.SendAsync(request, cts.Token).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                // 본문에 토큰이 되비칠 수 있으므로 상태 코드만 남긴다.
                Log.W($"API 호출 실패 ({(int)response.StatusCode}) : {Safe(url)}");
                throw new InvalidOperationException($"서버가 {(int)response.StatusCode} 응답을 돌려줬습니다.");
            }

            lock (cache)
            {
                cache[url] = new CacheEntry(body, DateTime.UtcNow);
            }

            return body;
        }
        catch (OperationCanceledException)
        {
            Log.W($"API 호출 시간 초과 : {Safe(url)}");
            throw new InvalidOperationException($"{Settings.TimeoutSeconds}초 안에 응답이 오지 않았습니다.");
        }
        catch (HttpRequestException ex)
        {
            Log.W($"API 호출 실패 : {Safe(url)} — {ex.Message}");
            throw new InvalidOperationException("서버에 연결하지 못했습니다.");
        }
    }

    /// <summary>예외 대신 성공 여부로 받고 싶을 때. 실패하면 <paramref name="error"/>에 사유가 담긴다.</summary>
    public async Task<(bool Ok, string Body, string Error)> TryFetchAsync(string url)
    {
        try
        {
            return (true, await FetchAsync(url).ConfigureAwait(false), string.Empty);
        }
        catch (Exception ex)
        {
            return (false, string.Empty, ex.Message);
        }
    }

    private bool TryGetCached(string url, out string body)
    {
        lock (cache)
        {
            if (cache.TryGetValue(url, out var entry) &&
                (DateTime.UtcNow - entry.At).TotalMilliseconds < Math.Max(0, Settings.MinIntervalMs))
            {
                body = entry.Body;
                return true;
            }
        }

        body = string.Empty;
        return false;
    }

    private void ApplyAuth(HttpRequestMessage request)
    {
        var token = Settings.Token;
        if (string.IsNullOrEmpty(token))
        {
            return;
        }

        switch (Settings.AuthMode)
        {
            case ApiAuthMode.BearerHeader:
                request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + token);
                break;

            case ApiAuthMode.CustomHeader:
                var name = string.IsNullOrWhiteSpace(Settings.HeaderName) ? "X-Api-Key" : Settings.HeaderName;
                request.Headers.TryAddWithoutValidation(name, token);
                break;

            case ApiAuthMode.Cookie:
                request.Headers.TryAddWithoutValidation("Cookie", token);
                break;
        }
    }

    /// <summary>로그에 남길 주소 — 쿼리에 토큰이 섞여 있을 수 있으므로 물음표 앞까지만 남긴다.</summary>
    private static string Safe(string url)
    {
        var index = url.IndexOf('?');
        return index < 0 ? url : url.Substring(0, index) + "?...";
    }

    private readonly struct CacheEntry
    {
        public CacheEntry(string body, DateTime at)
        {
            Body = body;
            At = at;
        }

        public string Body { get; }

        public DateTime At { get; }
    }
}

/// <summary>ApiService 설정. 토큰이 들어 있으므로 앱 설정 폴더에만 저장한다.</summary>
public sealed class ApiSettings
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static string FilePath => Path.Combine(UbisamAppBuilder.ConfigRoot, "Settings", "ApiSettings.json");

    public ApiAuthMode AuthMode { get; set; } = ApiAuthMode.None;

    /// <summary>AuthMode가 CustomHeader일 때 쓸 헤더 이름.</summary>
    public string HeaderName { get; set; } = "X-Api-Key";

    /// <summary>토큰/키/쿠키 값. 이 파일 밖으로 나가지 않는다.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>같은 주소를 이 시간 안에 다시 부르면 서버 대신 직전 응답을 쓴다(호출 제한 대비).</summary>
    public int MinIntervalMs { get; set; } = 1000;

    public int TimeoutSeconds { get; set; } = 10;

    public static ApiSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                return JsonSerializer.Deserialize<ApiSettings>(File.ReadAllText(FilePath), JsonOptions) ?? new ApiSettings();
            }
        }
        catch
        {
            // 손상된 파일은 무시하고 기본값(인증 없음)으로 간다.
        }

        return new ApiSettings();
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(FilePath, JsonSerializer.Serialize(this, JsonOptions));
    }
}
