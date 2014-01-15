if not exists( select * from sys.columns where object_id = object_id('dbo.Users') and name = 'UniqueId' ) 
	alter table dbo.Users add UniqueId nvarchar(900) not null
GO