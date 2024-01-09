USE [ContentDatabase]
GO

/****** Object: SqlProcedure [dbo].[DeleteData] Script Date: 1/8/2024 11:57:39 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE DeleteData
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DELETE su
	FROM 
		SocialUsers SU
		left join UserDatabase.dbo.AspNetUsers au
			on SU.UserId = au.id
	where 
		au.id is null

	DELETE FROM Posts

	DELETE FROM Groups

	DELETE FROM PostGroups

	DELETE FROM PostMetadatas

	DELETE FROM Views

	DELETE FROM Votes
END
