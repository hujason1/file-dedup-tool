param(
    [string]$OutName = "文件查重工具.exe"
)

# 编译脚本：用 Windows 自带的 .NET Framework 编译器，无需 Visual Studio
# 用法：pwsh -File build.ps1        （或右键“使用 PowerShell 运行”）
# 说明：源码是 C# 5 写法（不能用 $"" 插值、不能用 C# 6+ 语法），这是 csc.exe 的限制

Set-Location $PSScriptRoot

$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) {
    $csc = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
}
if (-not (Test-Path $csc)) {
    Write-Error "未找到 csc.exe，请确认已安装 .NET Framework 4.x"
    exit 1
}

# 上一次编译的 exe 可能还开着，先结束进程，否则写文件会失败
Get-Process | Where-Object { $_.ProcessName -like "*文件查重*" } | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 300

# 注意：把路径作为数组传入，不要手工加引号
$srcs = @(Get-ChildItem -Path "src\*.cs" | ForEach-Object { $_.FullName })

& $csc /nologo /target:winexe /platform:anycpu /optimize+ /codepage:65001 `
    /win32manifest:src\app.manifest /win32icon:src\app.ico `
    /out:$OutName `
    /r:System.dll /r:System.Core.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll `
    $srcs

if (Test-Path $OutName) {
    $f = Get-Item $OutName
    Write-Host ("编译成功：{0}（{1:N0} 字节）" -f $f.Name, $f.Length)
} else {
    Write-Error "编译失败，请查看上面的 csc 报错"
}
