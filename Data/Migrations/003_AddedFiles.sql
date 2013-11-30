if not exists( select * from sys.tables where object_id = object_id('Files') ) 
	create table Files (
		Id int identity primary key,
		Domain nvarchar(500),
		FilePath nvarchar(500),
		CreatedOn datetime2 not null,
		OriginalFileName nvarchar(500),

		Data varbinary(max),
		ContentType nvarchar(50)
	)
GO

if not exists( select * from sys.tables where object_id = object_id('FileVersions') ) 
	create table FileVersions (
		Id int identity primary key,
		[File_Id] int references Files not null,
		[Key] nvarchar(500),
		CreatedOn datetime2 not null,
		Data varbinary(max),
		ContentType nvarchar(50)
	)
GO