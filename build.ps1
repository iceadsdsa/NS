$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$csc64 = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$csc = if (Test-Path $csc64) { $csc64 } else { Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
if (-not (Test-Path $csc)) { Write-Error 'csc.exe not found (.NET Framework 4.x required)'; exit 1 }
New-Item -ItemType Directory -Force -Path dist, dist\dev, assets | Out-Null

if (-not (Test-Path assets\NS.ico)) {
    Add-Type -AssemblyName System.Drawing
    $bmp = New-Object System.Drawing.Bitmap(64, 64)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)
    $green = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(14, 124, 102))
    foreach ($e in @(@(0,0), @(36,0), @(0,36), @(36,36))) { $g.FillEllipse($green, $e[0], $e[1], 28, 28) }
    $g.FillRectangle($green, 14, 0, 36, 64)
    $g.FillRectangle($green, 0, 14, 64, 36)
    $font = New-Object System.Drawing.Font('Segoe UI', 26, [System.Drawing.FontStyle]::Bold)
    $sf = New-Object System.Drawing.StringFormat
    $sf.Alignment = [System.Drawing.StringAlignment]::Center
    $sf.LineAlignment = [System.Drawing.StringAlignment]::Center
    $g.DrawString('NS', $font, [System.Drawing.Brushes]::White, (New-Object System.Drawing.RectangleF(0, 0, 64, 64)), $sf)
    $icon = [System.Drawing.Icon]::FromHandle($bmp.GetHicon())
    $fs = [System.IO.File]::Create((Join-Path $PSScriptRoot 'assets\NS.ico'))
    $icon.Save($fs)
    $fs.Close()
    $g.Dispose()
    $bmp.Dispose()
}

$ui = 'src\Catalog.cs', 'src\Scanner.cs', 'src\Lang.cs', 'src\UiControls.cs', 'src\MainForm.cs'

& $csc /nologo /optimize+ /codepage:65001 /target:winexe /platform:anycpu /win32manifest:app.manifest /win32icon:assets\NS.ico `
    /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll `
    /out:dist\NS.exe $ui src\Program.cs
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

& $csc /nologo /optimize+ /codepage:65001 /target:winexe /platform:anycpu `
    /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll `
    /out:dist\dev\UiPreview.exe $ui tests\UiPreview.cs
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

& $csc /nologo /optimize+ /codepage:65001 /target:exe /platform:anycpu /r:System.dll `
    /out:dist\dev\ScannerTests.exe tests\ScannerTests.cs src\Catalog.cs src\Scanner.cs src\Lang.cs
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

& .\dist\dev\ScannerTests.exe
exit $LASTEXITCODE
