# This script simply looks if NodeJS is installed
# and if it is, runs TypeScript compiler under Node,
# otherwise just runs tsc.exe
#
# NOTE: it simply looks under Program Files, because I don't know
# another way to determine if NodeJS is installed.
# If you happen to know such a way, feel free.
#
# Hint: "Get-Command node" didn't work. :-)
# 

$node = @(
	( 
		"c:\program files\nodejs\node.exe", 
		"c:\program files (x86)\nodejs\node.exe"
	) |
	? { test-path $_ }
)
$myPath = Split-Path -Parent $MyInvocation.MyCommand.Definition

if ( $node.Length -gt 0 ) {
  . "$($node[0])" $(Join-Path $myPath "tsc.js") $args
} else {
  . $(Join-Path $myPath "tsc.exe") $args
}

if ( -not ($?) ) { exit -1 }