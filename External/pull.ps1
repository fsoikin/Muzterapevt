(
	'Libraries\Utils\bin\debug\erecruit.Utils',
	'Composition\bin\debug\erecruit.Composition'
) |
% { @( ($_ + '.dll'), ($_ + '.pdb') ) } |
% { 'c:\work\HRNet\' + $_ } |
% { xcopy $_ (Join-Path (Split-Path $MyInvocation.MyCommand.Path) ".\") /y }