﻿if not exists(select * from SpecialistSpecializations where name=N'Особые дети') 
	insert into SpecialistSpecializations([order], name) values(8, N'Особые дети' )
if not exists(select * from SpecialistSpecializations where name=N'Паллиативные пациенты')
  insert into SpecialistSpecializations([order], name) values(9, N'Паллиативные пациенты' )
if not exists(select * from SpecialistSpecializations where name=N'Подростки')
  insert into SpecialistSpecializations([order], name) values(10, N'Подростки' )
if not exists(select * from SpecialistSpecializations where name=N'Беременные')
  insert into SpecialistSpecializations([order], name) values(11, N'Беременные' )
if not exists(select * from SpecialistSpecializations where name=N'Работа с зависимостями')
  insert into SpecialistSpecializations([order], name) values(12, N'Работа с зависимостями' )
if not exists(select * from SpecialistSpecializations where name=N'Взрослые с неврологическими заболеваниями')
  insert into SpecialistSpecializations([order], name) values(13, N'Взрослые с неврологическими заболеваниями' )
if not exists(select * from SpecialistSpecializations where name=N'Взрослые с психическими заболеваниями')
  insert into SpecialistSpecializations([order], name) values(14, N'Взрослые с психическими заболеваниями' )
if not exists(select * from SpecialistSpecializations where name=N'Пожилые')
  insert into SpecialistSpecializations([order], name) values(15, N'Пожилые' )
if not exists(select * from SpecialistSpecializations where name=N'Здоровые взрослые')
  insert into SpecialistSpecializations([order], name) values(16, N'Здоровые взрослые' )
if not exists(select * from SpecialistSpecializations where name=N'Здоровые дети')
  insert into SpecialistSpecializations([order], name) values(17, N'Здоровые дети' )

update SpecialistSpecializations set [order] = 1000 where Name like N'%нет в списке%'