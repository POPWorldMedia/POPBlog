CREATE TABLE [dbo].[Users](
	[UserID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Name] [nvarchar](256) NOT NULL,
	[Email] [nvarchar](256) NOT NULL,
	[Password] [nvarchar](256) NOT NULL
	)
GO

CREATE TABLE [dbo].[Posts](
	[PostID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[UserID] [int] NOT NULL,
	[Title] [nvarchar](256) NOT NULL,
	[FullText] [nvarchar](max) NOT NULL,
	[TimeStamp] [datetime] NOT NULL,
	[Replies] [int] NOT NULL,
	[IsPrivate] [bit] NOT NULL,
	[IsClosed] [bit] NOT NULL,
	[IsLive] [bit] NOT NULL,
	[UrlTitle] [varchar](256) NOT NULL
)
GO

ALTER TABLE [dbo].[Posts]  WITH CHECK ADD  CONSTRAINT [FK_Posts_UserID] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO

CREATE TABLE [dbo].[Contents](
	[ContentID] [varchar](256) NOT NULL PRIMARY KEY,
	[Title] [nvarchar](256) NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
	[FullText] [nvarchar](max) NOT NULL
)
GO

CREATE TABLE [dbo].[Comments](
	[CommentID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[PostID] [int] NOT NULL,
	[UserID] [int] NULL,
	[FullText] [nvarchar](max) NOT NULL,
	[TimeStamp] [datetime] NOT NULL,
	[IsLive] [bit] NOT NULL,
	[IP] [varchar](40) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Email] [nvarchar](256) NULL,
	[WebSite] [nvarchar](256) NULL
)
GO

ALTER TABLE [dbo].[Comments]  WITH CHECK ADD  CONSTRAINT [FK_Comments_PostID] FOREIGN KEY([PostID])
REFERENCES [dbo].[Posts] ([PostID])
GO

CREATE TABLE [dbo].[ImageFolders](
	[ImageFolderID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Name] [nvarchar](256) NOT NULL,
	[ParentImageFolderID] [int] NULL
)
GO

CREATE TABLE [dbo].[Images](
	[ImageID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[MimeType] [nvarchar](256) NOT NULL,
	[FileName] [nvarchar](256) NOT NULL,
	[ImageFolderID] [int] NULL,
	[TimeStamp] [datetime] NOT NULL,
	[ImageBytes] [varbinary](max) NULL
)
GO

ALTER TABLE [dbo].[Images]  WITH CHECK ADD  CONSTRAINT [FK_csb_Image_csb_ImageFolder] FOREIGN KEY([ImageFolderID])
REFERENCES [dbo].[ImageFolders] ([ImageFolderID])
GO

CREATE TABLE [dbo].[IpBans](
	[IP] [varchar](40) NOT NULL PRIMARY KEY
)
GO
