param( $Target )

$root = [System.IO.Path]::Combine( (Split-Path $MyInvocation.MyCommand.Path), ".." )
cd $root
& build.ps1 $Target -Bootstrap