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
		Id uniqueidentifier not null,
		HostName nvarchar(100),
		Theme nvarchar(100)
	)
GO

	declare @sql nvarchar(max)
	select @sql = N'alter table dbo.Texts drop constraint [' + name + ']' from sys.key_constraints where parent_object_id = object_id('dbo.Texts') and type = 'PK'
	exec sp_executesql @sql

	alter table dbo.Texts add constraint [PK_dbo.Texts] primary key (Id, SiteId)
GO