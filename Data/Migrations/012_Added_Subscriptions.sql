if object_id('dbo.Subscriptions') is null
	create table dbo.Subscriptions (
		Id int identity primary key,
		Verified bit not null default 0,
		Email nvarchar(500) not null,
		VerificationToken uniqueidentifier not null
	)
GO