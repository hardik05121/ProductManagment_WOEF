
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[StateById]
    @Id int
AS
BEGIN

	SET NOCOUNT ON;

   	SELECT *
	FROM States
	WHERE Id=@Id
END