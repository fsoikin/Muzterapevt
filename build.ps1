# [CmdletBinding]
param( $Target )

# & nuget "install" "FAKE" "-OutputDirectory" "tools" "-ExcludeVersion"
& "Tools\FAKE\tools\Fake.exe" Build\build.fsx $(if ($Target) { "target=$Target" }) $args