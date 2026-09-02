Add-Type -AssemblyName System.Drawing

$srcPath = "c:\Users\ararg\source\AIRepos\SpaceImpact\SpaceDodger.Android\Resources\mipmap-xxxhdpi\spacedodgerico.png"
$destDownloads = "C:\Users\ararg\Downloads\SpaceDodger_Icon_512x512.png"
$destRepo = "c:\Users\ararg\source\AIRepos\SpaceImpact\SpaceDodger_Icon_512x512.png"

$src = [System.Drawing.Image]::FromFile($srcPath)
$dest = New-Object System.Drawing.Bitmap 512, 512
$g = [System.Drawing.Graphics]::FromImage($dest)

$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality

$g.DrawImage($src, 0, 0, 512, 512)

$dest.Save($destDownloads, [System.Drawing.Imaging.ImageFormat]::Png)
$dest.Save($destRepo, [System.Drawing.Imaging.ImageFormat]::Png)

$g.Dispose()
$dest.Dispose()
$src.Dispose()

Write-Output "SUCCESS"
