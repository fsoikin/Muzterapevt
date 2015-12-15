param( $Target )

$root = [System.IO.Path]::GetFullPath( [System.IO.Path]::Combine( (Split-Path $MyInvocation.MyCommand.Path), ".." ) )
cd $root
Write-Host $root
ls
& build.ps1 $Target -Bootstrap