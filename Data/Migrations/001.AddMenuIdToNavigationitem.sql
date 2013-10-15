if not exists( select * from sys.columns where object_id = object_id('NavigationItems') and name = 'MenuId' ) 
	alter table NavigationItems add MenuId nvarchar(100)
go

update NavigationItems set MenuId = 'TopMenu.First' where MenuId is null
GO