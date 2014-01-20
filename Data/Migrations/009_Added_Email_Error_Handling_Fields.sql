if not exists( select * from sys.columns where object_id = object_id('dbo.Emails') and name = 'ErrorCount' ) 
	alter table dbo.Emails add ErrorCount int not null default 0
GO
if not exists( select * from sys.columns where object_id = object_id('dbo.Emails') and name = 'LastError' ) 
	alter table dbo.Emails add LastError nvarchar(max)
GO
if not exists( select * from sys.columns where object_id = object_id('dbo.Emails') and name = 'Sent' ) 
	alter table dbo.Emails add [Sent] datetime2
GO
if not exists( select * from sys.columns where object_id = object_id('dbo.EmailServiceConfig') and name = 'MaxErrorsPerEmail' ) 
	alter table dbo.EmailServiceConfig add MaxErrorsPerEmail int not null default 5
GO