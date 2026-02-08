Add-Type -AssemblyName System.Drawing

function Add-RoundedRect {
    param(
        [System.Drawing.Drawing2D.GraphicsPath]$Path,
        [System.Drawing.RectangleF]$Rect,
        [single]$Radius
    )
    $diam = $Radius * 2
    $Path.AddArc($Rect.X, $Rect.Y, $diam, $diam, 180, 90)
    $Path.AddArc($Rect.Right - $diam, $Rect.Y, $diam, $diam, 270, 90)
    $Path.AddArc($Rect.Right - $diam, $Rect.Bottom - $diam, $diam, $diam, 0, 90)
    $Path.AddArc($Rect.X, $Rect.Bottom - $diam, $diam, $diam, 90, 90)
    $Path.CloseFigure()
}

$size = 256
$bmp = New-Object System.Drawing.Bitmap $size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::FromArgb(0,0,0,0))

$bgRect = New-Object System.Drawing.RectangleF 0,0,$size,$size
$bgBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush $bgRect, ([System.Drawing.Color]::FromArgb(255,62,90,118)), ([System.Drawing.Color]::FromArgb(255,35,47,70)), [System.Drawing.Drawing2D.LinearGradientMode]::Vertical
$g.FillRectangle($bgBrush, $bgRect)

$antennaRect = New-Object System.Drawing.RectangleF ($size*0.75), ($size*0.15), ($size*0.08), ($size*0.55)
$antennaPath = New-Object System.Drawing.Drawing2D.GraphicsPath
Add-RoundedRect $antennaPath $antennaRect ($size*0.02)
$antennaBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255,79,88,108))
$g.FillPath($antennaBrush, $antennaPath)
$antennaPath.Dispose()

$bodyRect = New-Object System.Drawing.RectangleF ($size*0.12), ($size*0.38), ($size*0.6), ($size*0.34)
$bodyPath = New-Object System.Drawing.Drawing2D.GraphicsPath
Add-RoundedRect $bodyPath $bodyRect ($size*0.06)
$bodyBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255,83,113,135))
$g.FillPath($bodyBrush, $bodyPath)
$bodyPath.Dispose()

$baseRect = New-Object System.Drawing.RectangleF ($size*0.1), ($size*0.64), ($size*0.65), ($size*0.08)
$basePath = New-Object System.Drawing.Drawing2D.GraphicsPath
Add-RoundedRect $basePath $baseRect ($size*0.04)
$shadowBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(80,0,0,0))
$g.FillPath($shadowBrush, $basePath)
$basePath.Dispose()

$wifiCenter = New-Object System.Drawing.PointF ($bodyRect.X + $bodyRect.Width / 2.0), ($size*0.24)
for ($i = 0; $i -lt 3; $i++) {
    $radius = $size*0.08 + $i*$size*0.035
    $rect = New-Object System.Drawing.RectangleF ($wifiCenter.X - $radius), ($wifiCenter.Y - $radius), ($radius*2), ($radius*2)
    $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255,254,97,97)), ($size*0.02)
    $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $g.DrawArc($pen, $rect, 205, 130)
    $pen.Dispose()
}

$indicatorColors = @(
    [System.Drawing.Color]::FromArgb(255,114,219,205),
    [System.Drawing.Color]::FromArgb(255,114,219,205),
    [System.Drawing.Color]::FromArgb(255,252,209,111),
    [System.Drawing.Color]::FromArgb(255,252,209,111)
)
for ($i = 0; $i -lt $indicatorColors.Length; $i++) {
    $brush = New-Object System.Drawing.SolidBrush $indicatorColors[$i]
    $diam = $size*0.07
    $x = $bodyRect.X + $size*0.08 + $i*($diam*0.95)
    $y = $bodyRect.Bottom - $size*0.12
    $g.FillEllipse($brush, $x, $y, $diam, $diam)
    $brush.Dispose()
}

$pngStream = New-Object System.IO.MemoryStream
$bmp.Save($pngStream, [System.Drawing.Imaging.ImageFormat]::Png)
$pngBytes = $pngStream.ToArray()
$pngStream.Dispose()

$iconPath = "c:/DevLocal/Kk.Kharts/KK.Kharts-IoT/Kk.GatewayFinder.Win/GatewayFinder.ico"
$fs = [System.IO.File]::Create($iconPath)
$bw = New-Object System.IO.BinaryWriter($fs)
$bw.Write([UInt16]0)
$bw.Write([UInt16]1)
$bw.Write([UInt16]1)
$bw.Write([byte]0)
$bw.Write([byte]0)
$bw.Write([byte]0)
$bw.Write([byte]0)
$bw.Write([UInt16]1)
$bw.Write([UInt16]32)
$bw.Write([UInt32]$pngBytes.Length)
$bw.Write([UInt32]22)
$bw.Write($pngBytes)
$bw.Flush()
$bw.Dispose()
$fs.Dispose()

$bgBrush.Dispose()
$antennaBrush.Dispose()
$bodyBrush.Dispose()
$shadowBrush.Dispose()
$g.Dispose()
$bmp.Dispose()
