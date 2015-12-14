if object_id('dbo.Regions') is null
	create table dbo.Regions (
		ID int identity primary key,
		ParentID int null,
		ShortName nvarchar(50) not null,
		Name nvarchar(200) not null,

		constraint FK_Region_Parent foreign key(ParentID) references dbo.Regions
	)
GO

if object_id('dbo.SpecialistRegions') is null
	create table dbo.SpecialistRegions (
		Specialist_Id int not null,
		Region_Id int not null,

		constraint FK_SpecialistRegions_Region foreign key(Region_Id) references dbo.Regions,
		constraint FK_SpecialistRegions_Specialist foreign key(Specialist_Id) references dbo.Specialists
	)
GO