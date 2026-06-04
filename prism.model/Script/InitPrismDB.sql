use TestManagementDB;

DBCC CHECKIDENT ('ResultType', RESEED, 0);
DBCC CHECKIDENT ('Project', RESEED, 0);
DBCC CHECKIDENT ('TestJob', RESEED, 0);
DBCC CHECKIDENT ('TestBuild', RESEED, 0);

EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all";
delete from TestJob;
delete from Project;
delete from TestBuild;
delete from ResultType;
EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all";

SET IDENTITY_INSERT ResultType ON;
insert into ResultType (id, name) values (0, 'NotExecuted');
insert into ResultType (id, name) values (1, 'Pass');
insert into ResultType (id, name) values (2, 'Fail');
insert into ResultType (id, name) values (3, 'Blocked');
insert into ResultType (id, name) values (4, 'InProgress');
insert into ResultType (id, name) values (5, 'Hang');
insert into ResultType (id, name) values (6, 'Paused');
insert into ResultType (id, name) values (7, 'Aborted');
SET IDENTITY_INSERT ResultType OFF;
select * from ResultType order  by id;									   

SET IDENTITY_INSERT Project ON;
insert into Project (id, name) values (1, 'Huggingface');
SET IDENTITY_INSERT Project OFF;
select * from Project order by id;
