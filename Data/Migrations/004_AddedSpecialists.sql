if not exists( select * from sys.tables where object_id = object_id('dbo.Countries') ) 
	create table dbo.Countries (
		Id int identity primary key,
		Name nvarchar(500),
	)
GO

if not exists( select * from sys.tables where object_id = object_id('dbo.Organizations') ) 
	create table dbo.Organizations (
		Id int identity primary key,
		Name nvarchar(500),
	)
GO

if not exists( select * from sys.tables where object_id = object_id('dbo.SpecialistProfessions') ) 
	create table dbo.SpecialistProfessions (
		Id int identity primary key,
		[Order] int not null default 0,
		Name nvarchar(500),
	)

	exec sp_executesql N'
		insert into dbo.SpecialistProfessions(Name, [Order]) values(N''Другое'', 0)
		insert into dbo.SpecialistProfessions(Name, [Order]) values(N''Музыкальный терапевт'', 1)
		insert into dbo.SpecialistProfessions(Name, [Order]) values(N''Музыкант-волонтёр'', 2)
		insert into dbo.SpecialistProfessions(Name, [Order]) values(N''Психолог'', 3)
		insert into dbo.SpecialistProfessions(Name, [Order]) values(N''Невролог'', 4)
		insert into dbo.SpecialistProfessions(Name, [Order]) values(N''Координатор проекта'', 5)
		insert into dbo.SpecialistProfessions(Name, [Order]) values(N''Директор (заведующий, руководитель) учреждения'', 6)
	'
GO

if not exists( select * from sys.tables where object_id = object_id('dbo.SpecialistSpecializations') ) 
	create table dbo.SpecialistSpecializations (
		Id int identity primary key,
		[Order] int not null default 0,
		Name nvarchar(500),
	)

	exec sp_executesql N'
		insert into dbo.SpecialistSpecializations(Name, [Order]) values(N''Моей специализации нет в списке'', 0)
		insert into dbo.SpecialistSpecializations(Name, [Order]) values(N''Детские дома'', 1)
		insert into dbo.SpecialistSpecializations(Name, [Order]) values(N''Специальное образование (дети)'', 2)
		insert into dbo.SpecialistSpecializations(Name, [Order]) values(N''Специальное образование (подростки и взрослые)'', 3)
		insert into dbo.SpecialistSpecializations(Name, [Order]) values(N''Хоспис (дети)'', 4)
		insert into dbo.SpecialistSpecializations(Name, [Order]) values(N''Хоспис (подростки)'', 5)
		insert into dbo.SpecialistSpecializations(Name, [Order]) values(N''Хоспис (взрослые)'', 6)
	'
GO

if not exists( select * from sys.tables where object_id = object_id('dbo.Specialists') ) 
	create table dbo.Specialists (
		Id int identity primary key,
		Approved bit not null,
		FirstName nvarchar(100) not null,
		LastName nvarchar(100) not null,
		PatronymicName nvarchar(100) null,
		City nvarchar(100) null,
		Resume nvarchar(max) null,
		Email nvarchar(100) null,
		IsEmailPublic bit not null,
		Phone nvarchar(100) null,
		IsPhonePublic bit not null,
		Url nvarchar(500) null,

		ProfessionDescription nvarchar(max) null,
		SpecializationDescription nvarchar(max) null,

		Photo_Id int null references dbo.Files,
		Organization_Id int null references dbo.Organizations,
		Profession_Id int not null default 1 references dbo.SpecialistProfessions,
		Specialization_Id int not null default 1 references dbo.SpecialistSpecializations
	)
GO

if not exists( select * from sys.tables where object_id = object_id('dbo.SpecialistCountries') ) 
	create table dbo.SpecialistCountries (
		Specialist_Id int not null references dbo.Specialists,
		Country_Id int not null references dbo.Countries
	)
GO