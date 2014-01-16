if not exists( select * from sys.columns where object_id = object_id('dbo.Pages') and name = 'SortOrder' ) 
	alter table dbo.Pages add SortOrder int not null default 0
GO