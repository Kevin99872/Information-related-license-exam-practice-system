Set-Location "c:\Users\kevinkai\Documents\GitHub\Information-related-license-exam-practice-system\desktop\B3"

$appName = "B3"
$targets = @(
    @{ Rid = "win-x64";    Name = "windows" },
    @{ Rid = "linux-x64";  Name = "linux" },
    @{ Rid = "osx-x64";    Name = "macOS-x64" },
    @{ Rid = "osx-arm64";  Name = "macOS-arm64" }
)

foreach ($t in $targets) {
    $outDir  = Join-Path "bin\Release" $t.Rid
    $zipPath = Join-Path "bin\Release" ("$appName-{0}.zip" -f $t.Rid)

    Write-Host "`n>>> 正在發布: $($t.Rid)" -ForegroundColor Cyan

    dotnet publish .\$appName.csproj -c Release -r $t.Rid -o $outDir --self-contained true `
        /p:PublishSingleFile=false `
        /p:PublishTrimmed=false `
        /p:PublishReadyToRun=false `
        /p:UseAppHost=true

    if ($LASTEXITCODE -ne 0) {
        Write-Host "發布失敗: $($t.Rid)" -ForegroundColor Red
        continue
    }

    # macOS：建立 .app bundle 結構
    if ($t.Rid -like "osx-*") {
        $appBundle  = Join-Path $outDir "$appName.app"
        $macOSDir   = Join-Path $appBundle "Contents\MacOS"
        $resDir     = Join-Path $appBundle "Contents\Resources"

        New-Item -ItemType Directory -Force -Path $macOSDir | Out-Null
        New-Item -ItemType Directory -Force -Path $resDir   | Out-Null

        # 複製所有發布檔案進 MacOS 資料夾（排除 .app 自己）
        Get-ChildItem -Path $outDir -Exclude "*.app" |
            Copy-Item -Destination $macOSDir -Recurse -Force

        # 寫入最基本的 Info.plist
        @"
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN"
  "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>             <string>$appName</string>
    <key>CFBundleExecutable</key>       <string>$appName</string>
    <key>CFBundleIdentifier</key>       <string>com.yourname.$($appName.ToLower())</string>
    <key>CFBundleVersion</key>          <string>1.0.0</string>
    <key>CFBundlePackageType</key>      <string>APPL</string>
    <key>NSHighResolutionCapable</key>  <true/>
</dict>
</plist>
"@ | Set-Content -Path (Join-Path $appBundle "Contents\Info.plist") -Encoding UTF8

        Write-Host "  .app bundle 已建立: $appBundle" -ForegroundColor Green
    }

    # 打包 zip
    if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
    Compress-Archive -Path (Join-Path $outDir "*") -DestinationPath $zipPath -Force
    Write-Host "  ZIP 完成: $zipPath" -ForegroundColor Green
}

Write-Host "`n所有平台打包完成！" -ForegroundColor Yellow