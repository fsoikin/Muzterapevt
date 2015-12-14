if object_id('dbo.SpecialistExperienceBrackets') is null 
	create table dbo.SpecialistExperienceBrackets (
		Id int identity primary key,
		Name nvarchar(500) not null,
		[Order] int not null,
		[IsNull] bit not null default 0
	)
GO

if not exists( select * from dbo.SpecialistExperienceBrackets ) 
	insert into dbo.SpecialistExperienceBrackets( Name, [Order], [IsNull] ) values
		( N'Нет', 0, 0 ),
		( N'Меньше года', 1, 0 ),
		( N'1-3 года', 2, 0 ),
		( N'Больше 3 лет', 3, 0 ),
		( N'Другое', 4, 1 )
GO

if columnproperty( object_id('dbo.Specialists'), 'Experience_Id', 'AllowsNull' ) is null begin
	declare @otherID int
	select top 1 @otherID = Id from dbo.SpecialistExperienceBrackets where [IsNull] = 1

	declare @sql nvarchar(1000) = N'alter table dbo.Specialists add Experience_Id int not null default ' + cast( @otherID as nvarchar(50) )
	exec sp_executesql @sql
end
GO

if columnproperty( object_id('dbo.Specialists'), 'ExperienceDescription', 'AllowsNull' ) is null
	alter table dbo.Specialists add ExperienceDescription nvarchar(max)
GO

if columnproperty( object_id('dbo.Specialists'), 'FormalEducation', 'AllowsNull' ) is null
	alter table dbo.Specialists add FormalEducation nvarchar(max)
GO

if columnproperty( object_id('dbo.Specialists'), 'MusicTherapyEducation', 'AllowsNull' ) is null
	alter table dbo.Specialists add MusicTherapyEducation nvarchar(max)
GO