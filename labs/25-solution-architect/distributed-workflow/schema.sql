IF OBJECT_ID('dbo.LabAccounts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LabAccounts (
        AccountId nvarchar(64) NOT NULL PRIMARY KEY,
        Available int NOT NULL CONSTRAINT CK_LabAccounts_Available CHECK (Available >= 0),
        Reserved int NOT NULL CONSTRAINT CK_LabAccounts_Reserved CHECK (Reserved >= 0)
    );
END;

IF OBJECT_ID('dbo.LabReservations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LabReservations (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        AccountId nvarchar(64) NOT NULL REFERENCES dbo.LabAccounts(AccountId),
        IdempotencyKey nvarchar(128) NOT NULL,
        PayloadHash char(64) NOT NULL,
        Quantity int NOT NULL CONSTRAINT CK_LabReservations_Quantity CHECK (Quantity > 0),
        Status nvarchar(24) NOT NULL,
        CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_LabReservations_Account_Key UNIQUE (AccountId, IdempotencyKey)
    );
END;

IF OBJECT_ID('dbo.LabOutbox', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LabOutbox (
        EventId uniqueidentifier NOT NULL PRIMARY KEY,
        ReservationId uniqueidentifier NOT NULL REFERENCES dbo.LabReservations(Id),
        Simulation nvarchar(24) NOT NULL,
        PublishedAt datetime2 NULL,
        CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
    CREATE INDEX IX_LabOutbox_Pending ON dbo.LabOutbox(PublishedAt, CreatedAt);
END;

IF OBJECT_ID('dbo.LabInbox', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LabInbox (
        EventId uniqueidentifier NOT NULL PRIMARY KEY,
        ProcessedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END;
