$targetPath = (Split-Path $MyInvocation.MyCommand.Path)

(
	'Libraries\Utils\bin\debug\erecruit.Utils',
	'Composition\bin\debug\erecruit.Composition',
	'UI\bin\debug\erecruit.UI'
) |
% { @( ($_ + '.dll'), ($_ + '.pdb') ) } |
% { 'c:\work\HRNet\' + $_ } |
% { xcopy $_ $targetPath /y }