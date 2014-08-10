param(
	[string]$DeployTarget,
	[string]$SmtpServer,
	[string]$SmtpUser,
	[string]$SmtpPassword
)

pushd (split-path -parent $MyInvocation.MyCommand.Definition)

@(
	".\$DeployTarget\web_smtp.config", 
	".\log4net.config"
	) | 
	% {
		get-content $_ |
		% { $_ `
				-replace '__smtp_server__', $SmtpServer `
				-replace '__smtp_user__', $SmtpUser `
				-replace '__smtp_password__', $SmtpPassword
	 	} |
		set-content "..\Web\$(split-path -leaf $_)"
	}

copy "$DeployTarget\web_settings.config" "..\Web\" -Force

popd