if not exists( select * from sys.columns where object_id = object_id('dbo.Specialists') and name = 'ContactEmail' ) 
	exec sp_rename 'Specialists.Email', 'ContactEmail', 'COLUMN'
	alter table dbo.Specialists add PublicEmail nvarchar(100) null
	exec sp_executesql N'update dbo.Specialists set PublicEmail = ContactEmail where IsEmailPublic = 1'
	alter table dbo.Specialists drop column IsEmailPublic
GO

if not exists( select * from sys.columns where object_id = object_id('dbo.Specialists') and name = 'ContactPhone' ) 
	exec sp_rename 'Specialists.Phone', 'ContactPhone', 'COLUMN'
	alter table dbo.Specialists add PublicPhone nvarchar(100) null
	exec sp_executesql N'update dbo.Specialists set PublicPhone = ContactPhone where IsPhonePublic = 1'
	alter table dbo.Specialists drop column IsPhonePublic
GO
