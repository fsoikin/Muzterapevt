param( $Target )

& "tools\nuget.exe" "install" "FAKE" "-OutputDirectory" "tools" "-ExcludeVersion"
& "tools\FAKE\tools\Fake.exe" Build\build.fsx $(if ($Target) { "target=$Target" }) $args