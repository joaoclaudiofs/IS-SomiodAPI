IF OBJECT_ID('dbo.Subscription', 'U') IS NOT NULL 
    DROP TABLE dbo.Subscription;
IF OBJECT_ID('dbo.ContentInstance', 'U') IS NOT NULL
    DROP TABLE dbo.ContentInstance;
IF OBJECT_ID('dbo.Container', 'U') IS NOT NULL 
    DROP TABLE dbo.Container;
IF OBJECT_ID('dbo.Application', 'U') IS NOT NULL 
    DROP TABLE dbo.Application;
GO

CREATE TABLE [dbo].[Application] (
    [Id]          INT IDENTITY(1,1) NOT NULL,
    [ResourceName] NVARCHAR(100) NOT NULL,
    [CreatedAt]    DATETIME NOT NULL DEFAULT GETDATE(),
    PRIMARY KEY CLUSTERED ([Id] ASC),
    UNIQUE NONCLUSTERED ([ResourceName] ASC)
);

CREATE TABLE [dbo].[Container] (
    [Id]            INT IDENTITY(1,1) NOT NULL,
    [ApplicationId] INT NOT NULL,
    [ResourceName]  NVARCHAR(100) NOT NULL,
    [CreatedAt]     DATETIME NOT NULL DEFAULT GETDATE(),
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_Container] UNIQUE NONCLUSTERED ([ApplicationId] ASC, [ResourceName] ASC),
    CONSTRAINT [FK_Container_Application]
        FOREIGN KEY ([ApplicationId]) REFERENCES [dbo].[Application] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[ContentInstance] (
    [Id]           INT IDENTITY(1,1) NOT NULL,
    [ContainerId]  INT NOT NULL,
    [ResourceName] NVARCHAR(100) NOT NULL,
    [ContentType]  NVARCHAR(100) NOT NULL,
    [Content]      NVARCHAR(MAX) NOT NULL,
    [CreatedAt]    DATETIME NOT NULL DEFAULT GETDATE(),
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_ContentInstance] UNIQUE NONCLUSTERED ([ContainerId] ASC, [ResourceName] ASC),
    CONSTRAINT [FK_ContentInstance_Container]
        FOREIGN KEY ([ContainerId]) REFERENCES [dbo].[Container] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[Subscription] (
    [Id]         INT IDENTITY(1,1) NOT NULL,
    [ContainerId] INT NOT NULL,
    [ResourceName] NVARCHAR(100) NOT NULL,
    [Evt]         INT NOT NULL,
    [EndPoint]    NVARCHAR(300) NOT NULL,
    [CreatedAt]   DATETIME NOT NULL DEFAULT GETDATE(),
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_Subscription] UNIQUE NONCLUSTERED ([ContainerId] ASC, [ResourceName] ASC),
    CONSTRAINT [FK_Subscription_Container]
        FOREIGN KEY ([ContainerId]) REFERENCES [dbo].[Container] ([Id]) ON DELETE CASCADE
);