if object_id('dbo.Specialist_Profession') is null
	create table dbo.Specialist_Profession (
		Specialist_Id int not null constraint FK_Specialist_Profession_Specialist references dbo.Specialists,
		Profession_Id int not null constraint FK_Specialist_Profession_Profession references dbo.SpecialistProfessions
	)

if object_id('dbo.Specialist_Specialization') is null
	create table dbo.Specialist_Specialization (
		Specialist_Id int not null constraint FK_Specialist_Specialization_Specialist references dbo.Specialists,
		Specialization_Id int not null constraint FK_Specialist_Specialization_Specialization references dbo.SpecialistSpecializations
	)
go

if exists( select * from sys.columns where name = 'Specialization_Id' and object_id = object_id('dbo.Specialists') ) begin
	
	insert into dbo.Specialist_Profession(Specialist_Id, Profession_Id)
	select Id, Profession_Id from Specialists

	insert into dbo.Specialist_Specialization(Specialist_Id, Specialization_Id)
	select Id, Specialization_Id from Specialists
	
	declare @dropFKs nvarchar(max) = ''
	select @dropFKs = @dropFKs + N' alter table Specialists drop constraint ' + name from sys.foreign_keys
	 where 
		(referenced_object_id = object_id('dbo.SpecialistSpecializations') and referenced_object_id = object_id('dbo.SpecialistProfessions'))
		or 
		(parent_object_id = object_id('dbo.Specialists'))

	select @dropFKs = @dropFKs + N' alter table Specialists drop constraint ' + dc.name from sys.default_constraints dc
	 join sys.columns c on dc.object_id = c.default_object_id
	 where dc.parent_object_id = object_id('dbo.Specialists') and (c.name = 'Specialization_Id' or c.name = 'Profession_Id')

	print @dropFKs
	
	exec sp_executesql @dropFKs
	exec sp_executesql N'alter table Specialists drop column Specialization_Id  alter table Specialists drop column Profession_Id'

end