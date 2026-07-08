# ============================================================
# setup-google-adc.ps1
# Xác thực Google Calendar qua Application Default Credentials
# Không cần JSON key — dùng tài khoản Google cá nhân
# ============================================================

Write-Host ""
Write-Host "=== Google Calendar - Application Default Credentials Setup ===" -ForegroundColor Cyan
Write-Host ""

# 1. Kiểm tra gcloud đã cài chưa
if (-not (Get-Command "gcloud" -ErrorAction SilentlyContinue)) {
    Write-Host "❌ gcloud CLI chưa được cài." -ForegroundColor Red
    Write-Host ""
    Write-Host "Cài đặt tại: https://cloud.google.com/sdk/docs/install" -ForegroundColor Yellow
    Write-Host "  → Chọn Windows → Download installer → Chạy GoogleCloudSDKInstaller.exe"
    Write-Host ""
    Write-Host "Sau khi cài xong, chạy lại script này."
    exit 1
}

Write-Host "✅ gcloud CLI đã cài." -ForegroundColor Green
$version = gcloud version --format="value(Google Cloud SDK)" 2>$null | Select-Object -First 1
Write-Host "   Version: $version" -ForegroundColor Gray
Write-Host ""

# 2. Kiểm tra ADC hiện tại
Write-Host "==> Kiểm tra Application Default Credentials hiện tại..." -ForegroundColor Cyan
$adcCheck = gcloud auth application-default print-access-token 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ ADC đã được cấu hình." -ForegroundColor Green
    $account = gcloud config get-value account 2>$null
    Write-Host "   Account: $account" -ForegroundColor Gray
} else {
    Write-Host "⚠️  ADC chưa được cấu hình. Tiến hành đăng nhập..." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Trình duyệt sẽ mở ra để bạn đăng nhập bằng tài khoản Google." -ForegroundColor White
    Write-Host "Hãy chọn tài khoản có quyền truy cập Google Calendar." -ForegroundColor White
    Write-Host ""
    
    gcloud auth application-default login --scopes="https://www.googleapis.com/auth/calendar"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Đăng nhập thất bại. Thử lại hoặc chạy thủ công: gcloud auth application-default login"
        exit 1
    }
    Write-Host ""
    Write-Host "✅ Đăng nhập thành công!" -ForegroundColor Green
}

# 3. Cập nhật appsettings (CalendarId)
Write-Host ""
$calendarId = Read-Host "Nhập Google Calendar ID (Enter để dùng 'primary')"
if ([string]::IsNullOrWhiteSpace($calendarId)) {
    $calendarId = "primary"
}

$projectPath = "src\EducationCenter.Crm.Api\EducationCenter.Crm.Api.csproj"

dotnet user-secrets set "GoogleCalendar:CalendarId" $calendarId --project $projectPath
dotnet user-secrets set "GoogleCalendar:UseApplicationDefaultCredentials" "true" --project $projectPath

Write-Host ""
Write-Host "✅ Cấu hình hoàn tất!" -ForegroundColor Green
Write-Host ""
Write-Host "Cấu hình đã lưu:" -ForegroundColor Cyan
Write-Host "  CalendarId: $calendarId"
Write-Host "  Auth mode : Application Default Credentials (ADC)"
Write-Host ""
Write-Host "Tiếp theo:" -ForegroundColor Cyan
Write-Host "  1. Chia sẻ Google Calendar với tài khoản bạn vừa đăng nhập"
Write-Host "     → calendar.google.com → Settings → Share with specific people"
Write-Host "     → Thêm email Google của bạn → Make changes to events"
Write-Host ""
Write-Host "  2. Restart backend:"
Write-Host "     dotnet run --project src\EducationCenter.Crm.Api\EducationCenter.Crm.Api.csproj --launch-profile http"
Write-Host ""
Write-Host "  3. Log sẽ hiển thị:"
Write-Host "     'Google Calendar: using Application Default Credentials (ADC)'"
