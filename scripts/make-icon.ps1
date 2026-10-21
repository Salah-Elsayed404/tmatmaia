<#
.SYNOPSIS
    Regenerates the neon tomato icon from code.

.DESCRIPTION
    Draws the icon at 256px with System.Drawing, then writes a multi-size
    .ico (256 down to 16px) for the app and a .png preview for the docs.
    Edit the colors or shapes below and rerun to change the icon.
#>
param(
    [string]$OutIco = (Join-Path $PSScriptRoot '..\src\Tmatmaia\Assets\tmatmaia.ico'),
    [string]$OutPng = (Join-Path $PSScriptRoot '..\docs\icon.png')
)Add-Type -AssemblyName System.Drawing
$ErrorActionPreference = 'Stop'

function New-RoundRect([float]$x, [float]$y, [float]$w, [float]$h, [float]$r) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $r * 2
    $p.AddArc($x, $y, $d, $d, 180, 90)
    $p.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $p.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
    $p.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $p.CloseFigure()
    $p
}

function Draw-Neon($g, $path, [System.Drawing.Color]$c, [float]$core) {
    foreach ($s in @(@(7, 14), @(5, 26), @(3.4, 45), @(2.2, 80), @(1.5, 140))) {
        $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb($s[1], $c)), ($core * $s[0])
        $pen.LineJoin = 'Round'; $pen.StartCap = 'Round'; $pen.EndCap = 'Round'
        $g.DrawPath($pen, $path)
    }
    $pen = New-Object System.Drawing.Pen $c, $core
    $pen.LineJoin = 'Round'; $pen.StartCap = 'Round'; $pen.EndCap = 'Round'
    $g.DrawPath($pen, $path)
    $hot = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(230, 255, 235, 240)), ($core * 0.35)
    $hot.LineJoin = 'Round'; $hot.StartCap = 'Round'; $hot.EndCap = 'Round'
    $g.DrawPath($hot, $path)
}

$S = 256
$bmp = New-Object System.Drawing.Bitmap $S, $S, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = 'AntiAlias'
$g.Clear([System.Drawing.Color]::Transparent)

# Dark tile
$tile = New-RoundRect 8 8 240 240 52
$tileBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush (New-Object System.Drawing.Point 0, 0), (New-Object System.Drawing.Point 256, 256), ([System.Drawing.Color]::FromArgb(255, 26, 10, 40)), ([System.Drawing.Color]::FromArgb(255, 8, 6, 20))
$g.FillPath($tileBrush, $tile)
$g.DrawPath((New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(90, 255, 46, 99)), 2), $tile)

$red = [System.Drawing.Color]::FromArgb(255, 255, 46, 99)
$green = [System.Drawing.Color]::FromArgb(255, 57, 255, 20)

# Tomato body: two overlapping lobes for the classic dimpled shape
$body = New-Object System.Drawing.Drawing2D.GraphicsPath
$body.AddBezier(128, 82, 168, 66, 222, 86, 214, 150)
$body.AddBezier(214, 150, 206, 206, 160, 220, 128, 216)
$body.AddBezier(128, 216, 96, 220, 50, 206, 42, 150)
$body.AddBezier(42, 150, 34, 86, 88, 66, 128, 82)
$body.CloseFigure()

$fill = New-Object System.Drawing.Drawing2D.PathGradientBrush $body
$fill.CenterPoint = New-Object System.Drawing.PointF 110, 130
$fill.CenterColor = [System.Drawing.Color]::FromArgb(120, 255, 46, 99)
$fill.SurroundColors = @([System.Drawing.Color]::FromArgb(25, 255, 46, 99))
$g.FillPath($fill, $body)
Draw-Neon $g $body $red 6

# Shine arc
$shine = New-Object System.Drawing.Drawing2D.GraphicsPath
$shine.AddArc(68, 108, 70, 70, 190, 60)
Draw-Neon $g $shine ([System.Drawing.Color]::FromArgb(255, 255, 160, 190)) 3.5

# Leaves: five-point star calyx plus stem
$cx = 128; $cy = 84
$star = New-Object System.Drawing.Drawing2D.GraphicsPath
$pts = @()
for ($i = 0; $i -lt 10; $i++) {
    $a = [Math]::PI * (-90 + $i * 36) / 180
    $r = if ($i % 2 -eq 0) { 40 } else { 13 }
    $pts += New-Object System.Drawing.PointF ($cx + $r * [Math]::Cos($a) * 1.25), ($cy + $r * [Math]::Sin($a) * 0.62)
}
$star.AddPolygon([System.Drawing.PointF[]]$pts)
Draw-Neon $g $star $green 4.5
$stem = New-Object System.Drawing.Drawing2D.GraphicsPath
$stem.AddBezier(128, 80, 126, 64, 132, 52, 146, 44)
Draw-Neon $g $stem $green 5

$g.Dispose()
$bmp.Save($OutPng, [System.Drawing.Imaging.ImageFormat]::Png)

# Build a multi-size ICO with PNG-compressed entries
$sizes = 256, 128, 64, 48, 32, 24, 16
$blobs = foreach ($sz in $sizes) {
    $b = New-Object System.Drawing.Bitmap $sz, $sz, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $gg = [System.Drawing.Graphics]::FromImage($b)
    $gg.InterpolationMode = 'HighQualityBicubic'; $gg.SmoothingMode = 'AntiAlias'; $gg.PixelOffsetMode = 'HighQuality'
    $gg.DrawImage($bmp, 0, 0, $sz, $sz); $gg.Dispose()
    $ms = New-Object System.IO.MemoryStream
    $b.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    , $ms.ToArray()
}
$fs = [System.IO.File]::Create($OutIco)
$w = New-Object System.IO.BinaryWriter $fs
$w.Write([uint16]0); $w.Write([uint16]1); $w.Write([uint16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $sz = $sizes[$i]; $d = if ($sz -ge 256) { 0 } else { $sz }
    $w.Write([byte]$d); $w.Write([byte]$d); $w.Write([byte]0); $w.Write([byte]0)
    $w.Write([uint16]1); $w.Write([uint16]32)
    $w.Write([uint32]$blobs[$i].Length); $w.Write([uint32]$offset)
    $offset += $blobs[$i].Length
}
foreach ($bl in $blobs) { $w.Write($bl) }
$w.Close()
Write-Host "Icon: $OutIco"; Write-Host "Preview: $OutPng"