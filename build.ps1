param( 
  [Parameter(Mandatory=$false,Position=1)][string]$Target,
  [Parameter(Mandatory=$false)][switch]$Bootstrap 
)

if ($Bootstrap) {
  & "tools\nuget.exe" "install" "FAKE" "-OutputDirectory" "tools" "-ExcludeVersion"
}

& "tools\FAKE\tools\Fake.exe" Build\build.fsx $(if ($Target) { "target=$Target" }) $args