param( 
  [Parameter(Mandatory=$false,Position=1)][string]$Target,
  [Parameter(Mandatory=$false)][string]$Args,
  [Parameter(Mandatory=$false)][switch]$Bootstrap 
)

if ($Bootstrap) {
  & "tools\nuget.exe" "install" "FAKE" "-OutputDirectory" "tools" "-ExcludeVersion"
}

& Invoke-Expression "tools\FAKE\tools\Fake.exe Build\build.fsx $(if ($Target) { "target=$Target" }) $Args"