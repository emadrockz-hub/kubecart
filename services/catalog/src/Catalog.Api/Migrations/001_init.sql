-- KubeCart Catalog - Initial Schema
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

-- 2) Categories
IF OBJECT_ID('dbo.Categories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories (
        CategoryId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Categories PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_Categories_CreatedAtUtc DEFAULT (SYSUTCDATETIME())
    );

    CREATE UNIQUE INDEX UX_Categories_Name ON dbo.Categories(Name);
END
GO

-- 3) Products
IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        ProductId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
        CategoryId INT NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(2000) NULL,
        Price DECIMAL(18,2) NOT NULL,
        Stock INT NOT NULL,
        ImageUrl NVARCHAR(500) NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_Products_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        UpdatedAtUtc DATETIME2(0) NULL,

        CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(CategoryId)
    );

    CREATE INDEX IX_Products_CategoryId ON dbo.Products(CategoryId);
    CREATE INDEX IX_Products_Name ON dbo.Products(Name);
END
GO

-- 4) ProductImages (multiple URLs per product)
IF OBJECT_ID('dbo.ProductImages', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductImages (
        ProductImageId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductImages PRIMARY KEY,
        ProductId INT NOT NULL,
        ImageUrl NVARCHAR(500) NOT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_ProductImages_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId)
    );

    CREATE INDEX IX_ProductImages_ProductId ON dbo.ProductImages(ProductId);
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

-- 6) Record migration version
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE Version = '001_init')
    INSERT INTO dbo.SchemaMigrations(Version) VALUES ('001_init');
GO
