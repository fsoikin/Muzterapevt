if not exists( select * from sys.columns where object_id = object_id('dbo.Pages') and name = 'SiteId' ) 
	alter table dbo.Pages add SiteId uniqueidentifier not null default '0CF5AEB6-6396-4E8B-888E-D646DE322AEB'
GO
if not exists( select * from sys.columns where object_id = object_id('dbo.NavigationItems') and name = 'SiteId' ) 
	alter table dbo.NavigationItems add SiteId uniqueidentifier not null default '0CF5AEB6-6396-4E8B-888E-D646DE322AEB'
GO
if not exists( select * from sys.columns where object_id = object_id('dbo.Files') and name = 'SiteId' ) 
	alter table dbo.Files add SiteId uniqueidentifier not null default '0CF5AEB6-6396-4E8B-888E-D646DE322AEB'
GO
if not exists( select * from sys.columns where object_id = object_id('dbo.Texts') and name = 'SiteId' ) 
	alter table dbo.Texts add SiteId uniqueidentifier not null default '0CF5AEB6-6396-4E8B-888E-D646DE322AEB'
GO

if not exists( select * from sys.tables where object_id = object_id('dbo.Sites') )
	create table dbo.Sites (
		Id uniqueidentifier not null primary key,
		HostName nvarchar(100),
		Theme nvarchar(100)
	)
GO

	sp_rename 'dbo.Texts', '_Texts'
	GO

	create table dbo.Texts(
		[Id] [nvarchar](128) NOT NULL,
		[BbText] [nvarchar](max) NULL,
		[HtmlText] [nvarchar](max) NULL,
		[SiteId] [uniqueidentifier] NOT NULL,
		primary key( Id, SiteId )
	)
	GO

	insert into dbo.Texts(Id, BbText, HtmlText, SiteId) select Id, BbText, HtmlText, '0CF5AEB6-6396-4E8B-888E-D646DE322AEB' from dbo._Texts
	GO

	drop table dbo._Texts
GO