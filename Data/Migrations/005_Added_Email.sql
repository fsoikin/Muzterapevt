if not exists( select * from sys.tables where object_id = object_id('dbo.Emails') ) 
	create table dbo.Emails (
		Id int not null identity primary key,
		[Subject] nvarchar(500),
		[Body] nvarchar(max),
		[ToName] nvarchar(100),
		[ToEmail] nvarchar(100),
		[FromName] nvarchar(100),
		[FromEmail] nvarchar(100)
	)
GO

if not exists( select * from sys.tables where object_id = object_id('dbo.EmailServiceConfig') ) 
	create table dbo.EmailServiceConfig (
		Id int not null identity primary key,
		CheckPeriodMilliseconds int not null default 10000,
		LastProcessedEmailId int not null default 0,
		[BatchSize] int not null default 20,
		DefaultFromEmail nvarchar(100),
		DefaultFromName nvarchar(100)
	)
GO