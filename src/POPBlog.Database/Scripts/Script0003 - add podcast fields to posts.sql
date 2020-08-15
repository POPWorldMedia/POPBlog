ALTER TABLE POSTS
ADD [IsPodcastPost] bit DEFAULT(0) NOT NULL,
	[FileName] nvarchar(256) NULL,
	[DownloadCount] int DEFAULT(0) NOT NULL,
	[Length] nvarchar(256) NULL,
	[Size] nvarchar(256) NULL
GO