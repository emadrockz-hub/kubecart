-- KubeCart Orders - Initial Schema
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

-- 2) Carts
IF OBJECT_ID('dbo.Carts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Carts (
        CartId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Carts PRIMARY KEY DEFAULT (NEWID()),
        UserId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Carts_Status DEFAULT ('Active'),
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_Carts_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        UpdatedAtUtc DATETIME2(0) NULL
    );

    CREATE INDEX IX_Carts_UserId ON dbo.Carts(UserId);
END
GO

-- 3) CartItems
IF OBJECT_ID('dbo.CartItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CartItems (
        CartItemId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CartItems PRIMARY KEY,
        CartId UNIQUEIDENTIFIER NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        AddedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_CartItems_AddedAtUtc DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT FK_CartItems_Carts FOREIGN KEY (CartId) REFERENCES dbo.Carts(CartId)
    );

    CREATE INDEX IX_CartItems_CartId ON dbo.CartItems(CartId);
    CREATE INDEX IX_CartItems_ProductId ON dbo.CartItems(ProductId);
END
GO

-- 4) Orders
IF OBJECT_ID('dbo.Orders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders (
        OrderId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Orders PRIMARY KEY DEFAULT (NEWID()),
        UserId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Orders_Status DEFAULT ('Pending'),
        TotalAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Orders_TotalAmount DEFAULT (0),
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_Orders_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        UpdatedAtUtc DATETIME2(0) NULL
    );

    CREATE INDEX IX_Orders_UserId ON dbo.Orders(UserId);
    CREATE INDEX IX_Orders_Status ON dbo.Orders(Status);
END
GO

-- 5) OrderItems (snapshot fields + computed LineTotal)
IF OBJECT_ID('dbo.OrderItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems (
        OrderItemId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderItems PRIMARY KEY,
        OrderId UNIQUEIDENTIFIER NOT NULL,

        ProductId INT NOT NULL,
        ProductName NVARCHAR(200) NOT NULL,
        ImageUrl NVARCHAR(500) NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        Quantity INT NOT NULL,

        LineTotal AS (CONVERT(DECIMAL(18,2), UnitPrice * Quantity)) PERSISTED,

        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_OrderItems_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(OrderId)
    );

    CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId);
    CREATE INDEX IX_OrderItems_ProductId ON dbo.OrderItems(ProductId);
END
GO

-- 6) AuditLogs
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs (
        AuditLogId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
        EventType NVARCHAR(100) NOT NULL,
        ActorUserId UNIQUEIDENTIFIER NULL,
        CorrelationId NVARCHAR(100) NULL,
        Details NVARCHAR(MAX) NULL,
        CreatedAtUtc DATETIME2(0) NOT NU
