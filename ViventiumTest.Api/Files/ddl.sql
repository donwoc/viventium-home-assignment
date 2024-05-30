create database ViventiumTest
go

use ViventiumTest
go


create table Company 
(
    CompanyId int not null,
    Code      varchar(50) not null,
    Description      varchar(50) not null,


    constraint [PK_Company] primary key (CompanyId)
)
go

create table Department
(
    DepartmentId int not null,
	CompanyId int not null,
    Name varchar(50) not null,

    constraint [PK_Department] primary key (DepartmentId),
	constraint [FK_Department] foreign key (CompanyId) references Company on delete cascade
)
go

create table Employee 
(
    EmployeeNumber varchar(50) not null,
    CompanyId  int not null,
    FirstName  varchar(50) not null,
    LastName   varchar(50) not null,
    Email      varchar(50) not null,
    HireDate   datetime null,
    DepartmentId int not null,
    ManagerEmployeeNumber varchar(50) null,
    
    constraint [PK_Employee] primary key (EmployeeNumber, CompanyId),
    constraint [FK_Employee_Company] foreign key (CompanyId) references Company(CompanyId),
    constraint [FK_Employee_Employee] foreign key (ManagerEmployeeNumber, CompanyId) references Employee(EmployeeNumber, CompanyId),
    constraint [FK_Employee_Department] foreign key (DepartmentId) references Department(DepartmentId)
)
go