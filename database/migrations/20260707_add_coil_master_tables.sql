SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;
GO

IF OBJECT_ID(N'dbo.CLCoils', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CLCoils
    (
        Id int IDENTITY(1,1) NOT NULL,
        Code varchar(50) NOT NULL,
        ManagementCode varchar(50) NULL,
        Name nvarchar(100) NOT NULL,
        DefaultMode varchar(4) NOT NULL CONSTRAINT DF_CLCoils_DefaultMode DEFAULT ('NONE'),
        GeometryCode varchar(10) NOT NULL CONSTRAINT DF_CLCoils_GeometryCode DEFAULT ('2510'),
        Length int NOT NULL,
        Height int NOT NULL,
        Tubes int NULL,
        NumberOfRows int NOT NULL,
        FinSpacing_mm float NOT NULL,
        NumberOfCircuits int NOT NULL,
        IdHeaderType int NULL,
        Active bit NOT NULL CONSTRAINT DF_CLCoils_Active DEFAULT (1),
        Remark nvarchar(max) NULL,
        CreatedAt datetime NOT NULL CONSTRAINT DF_CLCoils_CreatedAt DEFAULT (GETDATE()),
        CreatedBy varchar(64) NULL,
        UpdatedAt datetime NULL,
        UpdatedBy varchar(64) NULL,
        CONSTRAINT PK_CLCoils PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_CLCoils_Code UNIQUE (Code),
        CONSTRAINT CK_CLCoils_DefaultMode CHECK (DefaultMode IN ('NONE', 'CWD', 'HWD', 'HCD')),
        CONSTRAINT CK_CLCoils_GeometryCode CHECK (GeometryCode IN ('2510')),
        CONSTRAINT CK_CLCoils_Dimensions CHECK (Length > 0 AND Height > 0),
        CONSTRAINT CK_CLCoils_Tubes CHECK (Tubes IS NULL OR Tubes > 0),
        CONSTRAINT CK_CLCoils_Rows CHECK (NumberOfRows > 0),
        CONSTRAINT CK_CLCoils_FinSpacing CHECK (FinSpacing_mm > 0),
        CONSTRAINT CK_CLCoils_Circuits CHECK (NumberOfCircuits > 0),
        CONSTRAINT FK_CLCoils_HeaderType FOREIGN KEY (IdHeaderType) REFERENCES dbo.CLEnumItems(Id)
    );

    CREATE INDEX IX_CLCoils_ManagementCode ON dbo.CLCoils (ManagementCode);
    CREATE INDEX IX_CLCoils_DefaultMode ON dbo.CLCoils (DefaultMode, Active);
END;
GO

IF OBJECT_ID(N'dbo.CLHeatRecoveryModelCoils', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CLHeatRecoveryModelCoils
    (
        Id int IDENTITY(1,1) NOT NULL,
        IdHeatRecoveryModel int NOT NULL,
        IdCoil int NOT NULL,
        CoilMode varchar(3) NOT NULL,
        IsDefault bit NOT NULL CONSTRAINT DF_CLHeatRecoveryModelCoils_IsDefault DEFAULT (0),
        Active bit NOT NULL CONSTRAINT DF_CLHeatRecoveryModelCoils_Active DEFAULT (1),
        SortOrder int NOT NULL CONSTRAINT DF_CLHeatRecoveryModelCoils_SortOrder DEFAULT (0),
        Remark nvarchar(max) NULL,
        CreatedAt datetime NOT NULL CONSTRAINT DF_CLHeatRecoveryModelCoils_CreatedAt DEFAULT (GETDATE()),
        CreatedBy varchar(64) NULL,
        UpdatedAt datetime NULL,
        UpdatedBy varchar(64) NULL,
        CONSTRAINT PK_CLHeatRecoveryModelCoils PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_CLHeatRecoveryModelCoils_ModelCoilMode UNIQUE (IdHeatRecoveryModel, IdCoil, CoilMode),
        CONSTRAINT CK_CLHeatRecoveryModelCoils_CoilMode CHECK (CoilMode IN ('CWD', 'HWD', 'HCD')),
        CONSTRAINT FK_CLHeatRecoveryModelCoils_CLHeatRecoveryModels FOREIGN KEY (IdHeatRecoveryModel) REFERENCES dbo.CLHeatRecoveryModels(Id),
        CONSTRAINT FK_CLHeatRecoveryModelCoils_CLCoils FOREIGN KEY (IdCoil) REFERENCES dbo.CLCoils(Id)
    );

    CREATE INDEX IX_CLHeatRecoveryModelCoils_ModelMode ON dbo.CLHeatRecoveryModelCoils (IdHeatRecoveryModel, CoilMode, Active, IsDefault);
    CREATE INDEX IX_CLHeatRecoveryModelCoils_Coil ON dbo.CLHeatRecoveryModelCoils (IdCoil);
END;
GO

COMMIT TRANSACTION;
GO
