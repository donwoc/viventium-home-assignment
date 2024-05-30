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

create procedure ManagerChain
(
	@CompanyId int,
	@EmployeeNumber varchar(50)
)
as
	--Find the manger chain within a company given an employee

	--Table will hold the list if managers
	declare @Manager table 
	(
		EmployeeNumber varchar(50) not null,
		Level int not null
	)

	declare @Level int
	declare @CurrentEmployeeNumber varchar(50)
	
	-- Find the first manager in the chain
	set @Level = 1
	select @CurrentEmployeeNumber = ManagerEmployeeNumber 
	  from Employee 
	 where EmployeeNumber = @EmployeeNumber
	   and CompanyId = @CompanyId

	--Move up the chain untilt there is no manager
	while @CurrentEmployeeNumber is not null and @Level < 1000 --Make sure we stop sometime
	begin
		--Avoir circular references
		if exists (select * from @Manager where EmployeeNumber = @CurrentEmployeeNumber)
		begin
			break;
		end
		
		--Add the manager to the list
		insert into @Manager values(@CurrentEmployeeNumber, @Level)

		--Find next manager
		select @CurrentEmployeeNumber = ManagerEmployeeNumber 
		  from Employee 
		 where EmployeeNumber = @CurrentEmployeeNumber 
		   and CompanyId = @CompanyId

		--Increase the level
		set @Level = @Level + 1
	end

	--Return the manager chain with names
	select a.Level, b.EmployeeNumber, b.FirstName, b.LastName
	  from @Manager a inner join
	       Employee b on a.EmployeeNumber = b.EmployeeNumber and b.CompanyId = @CompanyId
  order by a.Level

go
