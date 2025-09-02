# BrowserSelector Image Resize Script
# Usage: .\resize_images.ps1

param(
    [switch]$Force
)

# Image folder settings
$ImageFolder = "D:\Project\BrowserSelector\src\BrowserSelector.Presentation\Resources\Images"
$IconFile = Join-Path $ImageFolder "BrowserSelector_Icon.png"
$LogoFile = Join-Path $ImageFolder "BrowserSelector_Logo.png"

# Resize function
function Resize-Image {
    param(
        [string]$SourceFile,
        [string]$OutputFile,
        [int]$Width,
        [int]$Height
    )
    
    try {
        Write-Host "  Resizing: ${Width}x${Height}..." -NoNewline
        
        $sourceImage = [System.Drawing.Image]::FromFile($SourceFile)
        $destImage = New-Object System.Drawing.Bitmap($Width, $Height)
        $graphics = [System.Drawing.Graphics]::FromImage($destImage)
        
        # High quality resize settings
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        
        # Draw image
        $graphics.DrawImage($sourceImage, 0, 0, $Width, $Height)
        
        # Save
        $destImage.Save($OutputFile, [System.Drawing.Imaging.ImageFormat]::Png)
        
        # Dispose resources
        $graphics.Dispose()
        $destImage.Dispose()
        $sourceImage.Dispose()
        
        Write-Host " OK" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host " FAILED" -ForegroundColor Red
        Write-Error "Error: $($_.Exception.Message)"
        return $false
    }
}

# Main processing
Write-Host "BrowserSelector Image Resize Script" -ForegroundColor Green
Write-Host "===================================" -ForegroundColor Green
Write-Host ""

# Check if image folder exists
if (!(Test-Path $ImageFolder)) {
    Write-Error "Error: Image folder not found: $ImageFolder"
    exit 1
}

Write-Host "Image folder: $ImageFolder" -ForegroundColor Cyan
Write-Host ""

# Process icon file
if (!(Test-Path $IconFile)) {
    Write-Warning "Warning: Icon file not found: $IconFile"
} else {
    Write-Host "Processing icon file: $IconFile" -ForegroundColor Yellow
    
    $iconSizes = @(16, 32, 48, 256)
    foreach ($size in $iconSizes) {
        $outputFile = Join-Path $ImageFolder "BrowserSelector_Icon_${size}.png"
        
        if ((Test-Path $outputFile) -and !$Force) {
            Write-Host "  Skip: BrowserSelector_Icon_${size}.png already exists" -ForegroundColor Gray
        } else {
            Write-Host "  Creating: BrowserSelector_Icon_${size}.png (${size} x ${size})" -ForegroundColor White
            
            $success = Resize-Image -SourceFile $IconFile -OutputFile $outputFile -Width $size -Height $size
            
            if ($success) {
                Write-Host "  OK: BrowserSelector_Icon_${size}.png created" -ForegroundColor Green
            } else {
                Write-Host "  FAILED: BrowserSelector_Icon_${size}.png creation failed" -ForegroundColor Red
            }
        }
    }
}

Write-Host ""

# Process logo file
if (!(Test-Path $LogoFile)) {
    Write-Warning "Warning: Logo file not found: $LogoFile"
} else {
    Write-Host "Processing logo file: $LogoFile" -ForegroundColor Yellow
    
    # Get original logo dimensions to calculate aspect ratio
    try {
        $originalImage = [System.Drawing.Image]::FromFile($LogoFile)
        $originalWidth = $originalImage.Width
        $originalHeight = $originalImage.Height
        $originalImage.Dispose()
        
        Write-Host "  Original dimensions: ${originalWidth} x ${originalHeight}" -ForegroundColor Cyan
        
        # Calculate 16:9 aspect ratio sizes
        # Base width sizes: 120, 180, 240
        # Height = Width * (9/16)
        $logoWidths = @(120, 180, 240)
        foreach ($width in $logoWidths) {
            $height = [Math]::Round($width * 9 / 16)
            $outputFile = Join-Path $ImageFolder "BrowserSelector_Logo_${width}.png"
            
            if ((Test-Path $outputFile) -and !$Force) {
                Write-Host "  Skip: BrowserSelector_Logo_${width}.png already exists" -ForegroundColor Gray
            } else {
                Write-Host "  Creating: BrowserSelector_Logo_${width}.png (${width} x ${height})" -ForegroundColor White
                
                $success = Resize-Image -SourceFile $LogoFile -OutputFile $outputFile -Width $width -Height $height
                
                if ($success) {
                    Write-Host "  OK: BrowserSelector_Logo_${width}.png created" -ForegroundColor Green
                } else {
                    Write-Host "  FAILED: BrowserSelector_Logo_${width}.png creation failed" -ForegroundColor Red
                }
            }
        }
    } catch {
        Write-Error "Error reading logo file: $($_.Exception.Message)"
    }
}

Write-Host ""
Write-Host "Processing complete!" -ForegroundColor Green
Write-Host "Output folder: $ImageFolder" -ForegroundColor Cyan

# Show created files
Write-Host ""
Write-Host "Created files:" -ForegroundColor Yellow

$foundFiles = @()
$foundFiles += Get-ChildItem $ImageFolder -Filter "BrowserSelector_Icon_*.png" -ErrorAction SilentlyContinue
$foundFiles += Get-ChildItem $ImageFolder -Filter "BrowserSelector_Logo_*.png" -ErrorAction SilentlyContinue

if ($foundFiles.Count -eq 0) {
    Write-Host "  No files found" -ForegroundColor Red
    Write-Host ""
    Write-Host "Debug information:" -ForegroundColor Yellow
    Write-Host "  Image folder: $ImageFolder"
    Write-Host "  Icon file: $IconFile"
    Write-Host "  Logo file: $LogoFile"
    Write-Host ""
    Write-Host "Files in folder:" -ForegroundColor Yellow
    try {
        $existingFiles = Get-ChildItem $ImageFolder -Filter "*.png" -ErrorAction Stop
        if ($existingFiles.Count -eq 0) {
            Write-Host "  No PNG files found"
        } else {
            foreach ($file in $existingFiles) {
                Write-Host "    $($file.Name)"
            }
        }
    } catch {
        Write-Host "  Cannot access folder: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    foreach ($file in $foundFiles | Sort-Object Name) {
        Write-Host "  $($file.Name)" -ForegroundColor White
    }
}

Write-Host ""
Write-Host "Script complete. Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")