if not exists( select * from sys.columns where object_id = object_id('dbo.Sites') and name = 'FriendlyName' ) 
	alter table dbo.Sites add FriendlyName nvarchar(200) null
GO