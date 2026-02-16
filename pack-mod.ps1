# 将 manifest.json、README.md、icon.png 和 Release 构建的 DLL 打包成 zip
# 版本号以 PluginInfo.cs 中 PLUGIN_VERSION 为准，打包时会同步写入 manifest。
# 用法: .\pack-mod.ps1  或  .\pack-mod.ps1 -OutputDir ".\dist"

param(
    [string]$OutputDir = "."
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

$dllName = "BattlefieldAnalysisBaseCollectSpaceJunk.dll"
$pluginInfoPath = Join-Path $scriptDir "PluginInfo.cs"
$manifestPath = Join-Path $scriptDir "manifest.json"
$readmePath = Join-Path $scriptDir "README.md"
$changelogPath = Join-Path $scriptDir "CHANGELOG.md"
$iconPath = Join-Path $scriptDir "icon.png"
$binRelease = Join-Path $scriptDir "bin\Release"

# 从 PluginInfo.cs 读取 PLUGIN_VERSION 作为版本号（单一来源）
if (-not (Test-Path $pluginInfoPath)) {
    Write-Error "未找到 PluginInfo.cs"
    exit 1
}
$versionLine = Get-Content $pluginInfoPath -Raw | Select-String -Pattern 'PLUGIN_VERSION\s*=\s*"([^"]+)"' -AllMatches
if (-not $versionLine -or -not $versionLine.Matches.Groups[1].Success) {
    Write-Error "无法从 PluginInfo.cs 解析 PLUGIN_VERSION"
    exit 1
}
$version = $versionLine.Matches.Groups[1].Value

# 查找 DLL（支持 bin\Release\net472\ 等任意子目录）
$dllPath = Get-ChildItem -Path $binRelease -Filter $dllName -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1

if (-not $dllPath) {
    Write-Error "未找到 $dllName，请先执行 Release 构建: dotnet build -c Release"
    exit 1
}

if (-not (Test-Path $manifestPath)) {
    Write-Error "未找到 manifest.json"
    exit 1
}

if (-not (Test-Path $readmePath)) {
    Write-Error "未找到 README.md"
    exit 1
}

$zipName = "BattlefieldAnalysisBaseCollectSpaceJunk-$version.zip"
$zipPath = Join-Path $OutputDir $zipName

# 确保输出目录存在
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

# 创建临时目录并复制文件（zip 根目录即为 mod 根目录）
$tempDir = Join-Path $env:TEMP "BattlefieldAnalysisBaseCollectSpaceJunk-pack-$(Get-Random)"
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

try {
    $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
    $manifest.version_number = $version
    $manifest | ConvertTo-Json -Depth 4 -Compress | Set-Content (Join-Path $tempDir "manifest.json") -Encoding UTF8
    Copy-Item $readmePath (Join-Path $tempDir "README.md") -Force
    if (Test-Path $changelogPath) {
        Copy-Item $changelogPath (Join-Path $tempDir "CHANGELOG.md") -Force
    }
    if (Test-Path $iconPath) {
        Copy-Item $iconPath (Join-Path $tempDir "icon.png") -Force
    }
    Copy-Item $dllPath.FullName (Join-Path $tempDir $dllName) -Force

    # 删除已存在的同名 zip
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }

    Compress-Archive -Path "$tempDir\*" -DestinationPath $zipPath -CompressionLevel Optimal
    Write-Host "已生成: $zipPath" -ForegroundColor Green
}
finally {
    Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
}
