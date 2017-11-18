CREATE TABLE [dbo].[Tickers] (
    [Id]       INT            IDENTITY (1, 1) NOT NULL,
    [Name]     NVARCHAR (200) NULL,
    [Symbol]   NVARCHAR (50)  NOT NULL,
    [Importer] NVARCHAR (50)  NOT NULL,
    [Tags]     NVARCHAR (MAX) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[TickerLists] (
    [Id]   INT            IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (200) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[TickerLists_Tickers] (
    [TickerId]     INT NOT NULL,
    [TickerListId] INT NOT NULL,
    CONSTRAINT [PK_TickerLists_Tickers] PRIMARY KEY CLUSTERED ([TickerId] ASC, [TickerListId] ASC)
);

CREATE TABLE [dbo].[SignalConfigs] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Endpoint]    NVARCHAR (50)  NOT NULL,
    [ExtraParams] NVARCHAR (MAX) NULL,
    [Type]        NVARCHAR (50)  NOT NULL,
    [Name]        NVARCHAR (50)  NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[ScanConfigs] (
    [Id]             INT           IDENTITY (1, 1) NOT NULL,
    [TickerListId]   INT           NOT NULL,
    [SignalConfigId] INT           NOT NULL,
    [Name]           NVARCHAR (50) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[CachedCandleKeys] (
    [Id]       INT           IDENTITY (1, 1) NOT NULL,
    [FromDate] DATETIME2 (7) NOT NULL,
    [ToDate]   DATETIME2 (7) NOT NULL,
    [TickerId] INT           NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[CachedCandles] (
    [Id]                INT             IDENTITY (1, 1) NOT NULL,
    [DateTime]          DATETIME2 (7)   NOT NULL,
    [Open]              DECIMAL (18, 6) NOT NULL,
    [High]              DECIMAL (18, 6) NOT NULL,
    [Low]               DECIMAL (18, 6) NOT NULL,
    [Close]             DECIMAL (18, 6) NOT NULL,
    [Interval]          VARCHAR (50)    NOT NULL,
    [CachedCandleKeyId] INT             NOT NULL,
    [Volume]            DECIMAL (18, 6) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CachedCandles_CachedCandleKeys] FOREIGN KEY ([CachedCandleKeyId]) REFERENCES [dbo].[CachedCandleKeys] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_CachedCandles_CachedCandleKeyId]
    ON [dbo].[CachedCandles]([CachedCandleKeyId] ASC);

