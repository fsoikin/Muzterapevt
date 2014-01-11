if not exists( select * from sys.columns where object_id = object_id('dbo.Specialists') and name = 'Ignored' ) 
	alter table dbo.Specialists add Ignored bit not null default 0
GO