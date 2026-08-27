namespace PoliceImageToolkit.Services;

public enum UpdateStatus
{
    Latest,        // 已是最新版本
    UpdateAvailable, // 發現新版本
    NetworkError,  // 網路連線失敗或逾時
    Failed         // API 解析失敗或其他異常
}

public record UpdateCheckResult(
    UpdateStatus Status,
    string CurrentVersion,
    string LatestVersion,
    string ReleaseTitle,
    string ReleaseNotes,
    string ReleaseUrl,
    string DownloadUrl,
    string ErrorMessage = ""
);

public interface IUpdateService
{
    string GetInstalledVersion();
    Task<UpdateCheckResult> CheckForUpdateAsync(string owner = "lianghao02", string repo = "Police-Image-Toolkit");
}
