# ============================================================
# set-google-calendar-secret.ps1
# Đọc file Service Account JSON và lưu vào dotnet user-secrets
# Chạy: .\scripts\set-google-calendar-secret.ps1 [-JsonPath "path\to\key.json"] [-CalendarId "your-calendar-id"]
# ============================================================

param(
    [string]$JsonPath = "service-account.json",
    [string]$CalendarId = "primary"
)

$ProjectPath = "src\EducationCenter.Crm.Api\EducationCenter.Crm.Api.csproj"

if (-not (Test-Path $JsonPath)) {
    Write-Error @"
File '$JsonPath' không tìm thấy.

Hướng dẫn lấy file:
  1. Vào https://console.cloud.google.com
  2. Chọn project của bạn (hoặc tạo mới)
  3. APIs & Services -> Credentials
  4. Create Credentials -> Service Account
  5. Đặt tên, nhấn Create and Continue, skip permissions
  6. Vào Service Account vừa tạo -> Keys -> Add Key -> Create new key -> JSON
  7. Lưu file .json vào thư mục gốc của project và chạy lại script này

Sau đó chia sẻ Calendar:
  8. Vào Google Calendar (calendar.google.com)
  9. Chọn calendar muốn dùng -> Settings -> Share with specific people
  10. Thêm email của Service Account (tìm trong file JSON ở trường 'client_email')
  11. Cấp quyền: Make changes to events
  12. Copy Calendar ID (ở phần 'Integrate calendar') và chạy lại:
      .\scripts\set-google-calendar-secret.ps1 -JsonPath "ten-file.json" -CalendarId "your-calendar-id@group.calendar.google.com"
"@
    exit 1
}

Write-Host "==> Đọc file: $JsonPath" -ForegroundColor Cyan

# Đọc và validate JSON
try {
    $json = Get-Content $JsonPath -Raw
    $parsed = $json | ConvertFrom-Json
    
    if ($parsed.type -ne "service_account") {
        Write-Error "File không phải Service Account JSON. Kiểm tra lại file."
        exit 1
    }
    
    Write-Host "✅ Service Account: $($parsed.client_email)" -ForegroundColor Green
    Write-Host "   Project ID: $($parsed.project_id)" -ForegroundColor Gray
} catch {
    Write-Error "File JSON không hợp lệ: $_"
    exit 1
}

# Compact JSON (xóa whitespace) để lưu vào user-secrets
$compactJson = $json | ConvertFrom-Json | ConvertTo-Json -Compress -Depth 10

Write-Host ""
Write-Host "==> Lưu credentials vào User Secrets..." -ForegroundColor Cyan

# Set CredentialsJson
$compactJson | dotnet user-secrets set "GoogleCalendar:CredentialsJson" --project $ProjectPath
if ($LASTEXITCODE -ne 0) {
    Write-Error "Lỗi khi set GoogleCalendar:CredentialsJson"
    exit 1
}

# Set CalendarId
dotnet user-secrets set "GoogleCalendar:CalendarId" $CalendarId --project $ProjectPath
if ($LASTEXITCODE -ne 0) {
    Write-Error "Lỗi khi set GoogleCalendar:CalendarId"
    exit 1
}

Write-Host ""
Write-Host "✅ Hoàn thành! Secrets đã được lưu an toàn." -ForegroundColor Green
Write-Host ""
Write-Host "Kiểm tra:" -ForegroundColor Cyan
dotnet user-secrets list --project $ProjectPath | Select-String "GoogleCalendar"

Write-Host ""
Write-Host "⚠️  Lưu ý bảo mật:" -ForegroundColor Yellow
Write-Host "   - File $JsonPath KHÔNG được commit lên git"
Write-Host "   - Thêm vào .gitignore nếu chưa có: echo '$JsonPath' >> .gitignore"
Write-Host "   - User Secrets chỉ dành cho Development; dùng biến môi trường cho Production"
