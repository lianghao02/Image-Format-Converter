using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace PoliceImageToolkit.Services;

public class UpdateService : IUpdateService
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(6)
    };

    public string GetInstalledVersion()
    {
        try
        {
            // 1. 優先讀取執行檔同目錄的 version.txt
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string txtPath = Path.Combine(baseDir, "version.txt");
            if (File.Exists(txtPath))
            {
                string text = File.ReadAllText(txtPath).Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text.StartsWith("v", StringComparison.OrdinalIgnoreCase) ? text : $"v{text}";
                }
            }

            // 2. 備援讀取應用程式資源
            var uri = new Uri("pack://application:,,,/version.txt", UriKind.Absolute);
            var streamInfo = System.Windows.Application.GetResourceStream(uri);
            if (streamInfo != null)
            {
                using var reader = new StreamReader(streamInfo.Stream);
                string text = reader.ReadToEnd().Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text.StartsWith("v", StringComparison.OrdinalIgnoreCase) ? text : $"v{text}";
                }
            }
        }
        catch
        {
            // 忽略讀取例外，採用組件預設版本
        }

        // 3. 最後備援：讀取組件版本號
        var asm = Assembly.GetExecutingAssembly();
        var ver = asm.GetName().Version;
        if (ver != null)
        {
            return $"v{ver.Major}.{ver.Minor}.{ver.Build}";
        }

        return "v11.2.0";
    }

    public async Task<UpdateCheckResult> CheckForUpdateAsync(string owner = "lianghao02", string repo = "Police-Image-Toolkit")
    {
        string currentVer = GetInstalledVersion();
        string requestUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("User-Agent", "Police-Image-Toolkit-Updater");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                return new UpdateCheckResult(
                    UpdateStatus.Failed,
                    currentVer,
                    currentVer,
                    string.Empty,
                    string.Empty,
                    $"https://github.com/{owner}/{repo}/releases",
                    string.Empty,
                    $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}"
                );
            }

            string jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;

            string tagName = root.TryGetProperty("tag_name", out var tagProp) ? tagProp.GetString() ?? "" : "";
            string releaseTitle = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "" : "";
            string releaseNotes = root.TryGetProperty("body", out var bodyProp) ? bodyProp.GetString() ?? "" : "";
            string releaseUrl = root.TryGetProperty("html_url", out var urlProp) ? urlProp.GetString() ?? "" : $"https://github.com/{owner}/{repo}/releases";
            string downloadUrl = "";

            // 解析 assets 中的 exe 下載連結
            if (root.TryGetProperty("assets", out var assetsProp) && assetsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var asset in assetsProp.EnumerateArray())
                {
                    if (asset.TryGetProperty("browser_download_url", out var dlProp))
                    {
                        string dl = dlProp.GetString() ?? "";
                        if (dl.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            downloadUrl = dl;
                            break;
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(tagName))
            {
                return new UpdateCheckResult(
                    UpdateStatus.Failed,
                    currentVer,
                    currentVer,
                    releaseTitle,
                    releaseNotes,
                    releaseUrl,
                    downloadUrl,
                    "無法自回應中取得有效的 tag_name 版本標籤。"
                );
            }

            // 版本比對 (比較語義版本)
            bool hasUpdate = CompareVersions(tagName, currentVer) > 0;

            return new UpdateCheckResult(
                hasUpdate ? UpdateStatus.UpdateAvailable : UpdateStatus.Latest,
                currentVer,
                tagName,
                releaseTitle,
                releaseNotes,
                releaseUrl,
                downloadUrl
            );
        }
        catch (HttpRequestException ex)
        {
            return new UpdateCheckResult(
                UpdateStatus.NetworkError,
                currentVer,
                currentVer,
                string.Empty,
                string.Empty,
                $"https://github.com/{owner}/{repo}/releases",
                string.Empty,
                $"網路連線失敗：{ex.Message}"
            );
        }
        catch (TaskCanceledException)
        {
            return new UpdateCheckResult(
                UpdateStatus.NetworkError,
                currentVer,
                currentVer,
                string.Empty,
                string.Empty,
                $"https://github.com/{owner}/{repo}/releases",
                string.Empty,
                "連線逾時，請確認網路環境或防火牆設定。"
            );
        }
        catch (Exception ex)
        {
            return new UpdateCheckResult(
                UpdateStatus.Failed,
                currentVer,
                currentVer,
                string.Empty,
                string.Empty,
                $"https://github.com/{owner}/{repo}/releases",
                string.Empty,
                $"檢查更新時發生未預期錯誤：{ex.Message}"
            );
        }
    }

    /// <summary>
    /// 比對兩個版本字串（例如 "v11.3.0" 與 "v11.2.0"）
    /// 回傳 > 0 代表 v1 大於 v2；回傳 0 代表相等；回傳 < 0 代表 v1 小於 v2
    /// </summary>
    public static int CompareVersions(string v1, string v2)
    {
        var ver1 = NormalizeVersion(v1);
        var ver2 = NormalizeVersion(v2);

        return ver1.CompareTo(ver2);
    }

    private static Version NormalizeVersion(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return new Version(0, 0, 0);

        string clean = raw.Trim().TrimStart('v', 'V');
        // 去除 commit hash 或額外後綴 (例如 11.2.0-beta 或 11.2.0+abc)
        int dashIdx = clean.IndexOf('-');
        if (dashIdx > 0) clean = clean[..dashIdx];
        int plusIdx = clean.IndexOf('+');
        if (plusIdx > 0) clean = clean[..plusIdx];

        var parts = clean.Split('.');
        int major = parts.Length > 0 && int.TryParse(parts[0], out int maj) ? maj : 0;
        int minor = parts.Length > 1 && int.TryParse(parts[1], out int min) ? min : 0;
        int build = parts.Length > 2 && int.TryParse(parts[2], out int bld) ? bld : 0;

        return new Version(major, minor, build);
    }
}
