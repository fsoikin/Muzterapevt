if not exists( select * from sys.columns where object_id = object_id('Pages') and name = 'TagsStandIn' ) 
	alter table Pages add TagsStandIn nvarchar(max)
GO

if not exists( select * from sys.columns where object_id = object_id('Pages') and name = 'ReferenceName' ) 
	alter table Pages add ReferenceName nvarchar(200)
GO