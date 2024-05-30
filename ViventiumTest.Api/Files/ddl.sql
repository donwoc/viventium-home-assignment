CREATE DATABASE ViventiumTest
go

USE ViventiumTest
GO


CREATE TABLE Company 
(
    CompanyId int not null,
    Code      varchar(50) not null,
    Name      varchar(50) not null,


    constraint [PK_Company] primary key (CompanyId)
)
GO

CREATE TABLE Department
(
    DepartmentId int not null,
    Name varchar(50) not null,

    CONSTRAINT [PK_Department] PRIMARY KEY (DepartmentId)
)
go

CREATE TABLE Employee 
(
    EmployeeNumber varchar(50) not null,
    CompanyId  int not null,
    FirstName  varchar(50) not null,
    LastName   varchar(50) not null,
    Email      varchar(50) not null,
    HireDate   date null,
    DepartmentId int not null,
    ManagerEmployeeNumber varchar(50) null,
    
    constraint [PK_Employee] primary key (EmployeeNumber),
    constraint [FK_Employee_Company] foreign key (CompanyId) references Company(CompanyId),
    constraint [FK_Employee_Employee] foreign key (ManagerEmployeeNumber) references Employee(EmployeeNumber),
    constraint [FK_Employee_Department] foreign key (DepartmentId) references Department(DepartmentId)
)
GO