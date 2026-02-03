-- KubeCart Identity - Initial Schema
-- Safe to re-run (idempotent checks)

-- 1) SchemaMigrations
IF OBJECT_ID('dbo.SchemaMigrations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SchemaMigrations (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SchemaMigrations PRIMARY KEY,
        Version NVARCHAR(50) NOT NULL,
        AppliedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_SchemaMigrations_AppliedAtUtc DEFAULT (SYSUTCDATETIME())
    );

    CREATE UNIQUE INDEX UX_SchemaMigrations_Version ON dbo.SchemaMigrations(Version);
END
GO

-- 2) Users
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        UserId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Users PRIMARY KEY DEFAULT (NEWID()),
        Email NVARCHAR(256) NOT NULL,
        FullName NVARCHAR(200) NULL,

        PasswordHash VARBINARY(64) NOT NULL,
        PasswordSalt VARBINARY(32) NOT NULL,

        IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_Users_CreatedAtUtc DEFAULT (SYSUTCDATETIME())
    );

    CREATE UNIQUE INDEX UX_Users_Email ON dbo.Users(Email);
END
GO

-- 3) Roles
IF OBJECT_ID('dbo.Roles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles (
        RoleId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL
    );

    CREATE UNIQUE INDEX UX_Roles_Name ON dbo.Roles(Name);
END
GO

-- 4) UserRoles (many-to-many)
IF OBJECT_ID('dbo.UserRoles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserRoles (
        UserId UNIQUEIDENTIFIER NOT NULL,
        RoleId INT NOT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_UserRoles_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId)
    );
END
GO

-- 5) AuditLogs
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs (
        AuditLogId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
        EventType NVARCHAR(100) NOT NULL,
        ActorUserId UNIQUEIDENTIFIER NULL,
        CorrelationId NVARCHAR(100) NULL,
        Details NVARCHAR(MAX) NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_AuditLogs_CreatedAtUtc DEFAULT (SYSUTCDATETIME())
    );

    CREATE INDEX IX_AuditLogs_EventType ON dbo.AuditLogs(EventType);
    CREATE INDEX IX_AuditLogs_ActorUserId ON dbo.AuditLogs(ActorUserId);
END
GO

-- 6) Seed Roles (Admin, Customer)
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = 'Admin')
    INSERT INTO dbo.Roles(Name) VALUES ('Admin');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = 'Customer')
    INSERT INTO dbo.Roles(Name) VALUES ('Customer');
GO

-- 7) Record migration version
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE Version = '001_init')
    INSERT INTO dbo.SchemaMigrations(Version) VALUES ('001_init');
GO
