use TestManagementDB;

delete from ResultType;
insert into ResultType (id, name) values (0, 'Pass');
insert into ResultType (id, name) values (1, 'Fail');
insert into ResultType (id, name) values (2, 'NotExecuted');
insert into ResultType (id, name) values (3, 'Blocked');
insert into ResultType (id, name) values (4, 'InProgress');
insert into ResultType (id, name) values (5, 'Hang');
insert into ResultType (id, name) values (6, 'Paused');
insert into ResultType (id, name) values (7, 'Aborted');
select * from ResultType;									   

delete from Project;
insert into Project (id, name) values (0, 'Huggingface');
select * from Project;
