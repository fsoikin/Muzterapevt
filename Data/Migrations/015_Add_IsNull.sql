if not exists( select * from sys.columns where object_id = object_id('dbo.SpecialistSpecializations') and name = 'IsNull' ) 
	alter table dbo.SpecialistSpecializations add [IsNull] bit not null default 0
	exec sp_executesql N'update dbo.SpecialistSpecializations set [IsNull] = 1 where [Order] = 0'
GO

if not exists( select * from sys.columns where object_id = object_id('dbo.SpecialistProfessions') and name = 'IsNull' ) 
	alter table dbo.SpecialistProfessions add [IsNull] bit not null default 0
	exec sp_executesql N'update dbo.SpecialistProfessions set [IsNull] = 1 where [Order] = 0'
GO

