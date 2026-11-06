-- База данных учебной программы «Учёт пенсионеров».
-- Все записи в таблице Pensioners вымышленные.
-- Скрипт можно запускать повторно: уже созданные объекты он не трогает.

IF DB_ID(N'PensionBase') IS NULL
    CREATE DATABASE PensionBase;
GO

USE PensionBase;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
CREATE TABLE dbo.Users
(
    Id           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    Login        NVARCHAR(50)      NOT NULL CONSTRAINT UQ_Users_Login UNIQUE,
    FullName     NVARCHAR(150)     NOT NULL,
    PasswordHash VARBINARY(32)     NOT NULL,
    PasswordSalt VARBINARY(16)     NOT NULL,
    CreatedAt    DATETIME2(0)      NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSDATETIME()
);
GO

IF OBJECT_ID(N'dbo.PensionTypes', N'U') IS NULL
CREATE TABLE dbo.PensionTypes
(
    Id   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PensionTypes PRIMARY KEY,
    Name NVARCHAR(100)     NOT NULL CONSTRAINT UQ_PensionTypes_Name UNIQUE
);
GO

IF OBJECT_ID(N'dbo.Pensioners', N'U') IS NULL
CREATE TABLE dbo.Pensioners
(
    Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Pensioners PRIMARY KEY,
    LastName      NVARCHAR(50)      NOT NULL,
    FirstName     NVARCHAR(50)      NOT NULL,
    MiddleName    NVARCHAR(50)      NULL,
    BirthDate     DATE              NOT NULL,
    Gender        NCHAR(1)          NOT NULL CONSTRAINT CK_Pensioners_Gender CHECK (Gender IN (N'М', N'Ж')),
    Snils         CHAR(14)          NOT NULL CONSTRAINT UQ_Pensioners_Snils UNIQUE,
    Address       NVARCHAR(200)     NULL,
    Phone         NVARCHAR(20)      NULL,
    PensionTypeId INT               NOT NULL CONSTRAINT FK_Pensioners_PensionTypes REFERENCES dbo.PensionTypes(Id),
    PensionAmount DECIMAL(10,2)     NOT NULL CONSTRAINT CK_Pensioners_Amount CHECK (PensionAmount >= 0),
    AssignedDate  DATE              NOT NULL
);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.PensionTypes)
INSERT INTO dbo.PensionTypes (Name) VALUES
    (N'Страховая по старости'),
    (N'Страховая по инвалидности'),
    (N'Страховая по потере кормильца'),
    (N'Государственная'),
    (N'Социальная');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Pensioners)
INSERT INTO dbo.Pensioners
    (LastName, FirstName, MiddleName, BirthDate, Gender, Snils, Address, Phone, PensionTypeId, PensionAmount, AssignedDate)
VALUES
    (N'Иванов',    N'Пётр',      N'Сергеевич',    '1956-03-14', N'М', '000-000-001 00', N'г. Канск, ул. Учебная, д. 1',  N'+7 000 000-00-01', 1, 24150.40, '2019-04-01'),
    (N'Смирнова',  N'Анна',      N'Ивановна',     '1961-07-22', N'Ж', '000-000-002 00', N'г. Канск, ул. Учебная, д. 2',  N'+7 000 000-00-02', 1, 21870.15, '2018-08-01'),
    (N'Кузнецов',  N'Виктор',    N'Павлович',     '1958-11-05', N'М', '000-000-003 00', N'г. Канск, ул. Учебная, д. 3',  N'+7 000 000-00-03', 2, 16320.00, '2015-12-01'),
    (N'Попова',    N'Мария',     N'Алексеевна',   '1953-01-30', N'Ж', '000-000-004 00', N'г. Канск, ул. Учебная, д. 4',  N'+7 000 000-00-04', 1, 19640.75, '2008-02-01'),
    (N'Соколов',   N'Николай',   N'Андреевич',    '1949-09-18', N'М', '000-000-005 00', N'г. Канск, ул. Учебная, д. 5',  N'+7 000 000-00-05', 4, 31200.00, '2009-10-01'),
    (N'Лебедева',  N'Ольга',     N'Викторовна',   '1964-05-09', N'Ж', '000-000-006 00', N'г. Канск, ул. Учебная, д. 6',  N'+7 000 000-00-06', 3, 14780.30, '2021-06-01'),
    (N'Козлов',    N'Александр', N'Михайлович',   '1955-12-27', N'М', '000-000-007 00', N'г. Канск, ул. Учебная, д. 7',  N'+7 000 000-00-07', 1, 26410.90, '2016-01-01'),
    (N'Новикова',  N'Татьяна',   N'Николаевна',   '1959-04-16', N'Ж', '000-000-008 00', N'г. Канск, ул. Учебная, д. 8',  N'+7 000 000-00-08', 1, 20955.60, '2014-05-01'),
    (N'Морозов',   N'Геннадий',  N'Фёдорович',    '1951-08-03', N'М', '000-000-009 00', N'г. Канск, ул. Учебная, д. 9',  N'+7 000 000-00-09', 5, 13680.00, '2016-09-01'),
    (N'Волкова',   N'Галина',    N'Петровна',     '1957-02-11', N'Ж', '000-000-010 00', N'г. Канск, ул. Учебная, д. 10', N'+7 000 000-00-10', 2, 15290.45, '2012-03-01'),
    (N'Фёдоров',   N'Юрий',      N'Дмитриевич',   '1960-10-25', N'М', '000-000-011 00', N'г. Канск, ул. Учебная, д. 11', N'+7 000 000-00-11', 1, 23305.20, '2020-11-01'),
    (N'Михайлова', N'Людмила',   N'Григорьевна',  '1954-06-07', N'Ж', '000-000-012 00', N'г. Канск, ул. Учебная, д. 12', N'+7 000 000-00-12', 1, 18725.85, '2009-07-01');
GO
