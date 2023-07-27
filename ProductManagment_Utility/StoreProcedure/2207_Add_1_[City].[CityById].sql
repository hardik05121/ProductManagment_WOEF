USE [ProductManagment_WOEF]
GO

/****** Object:  StoredProcedure [dbo].[CityById]    Script Date: 7/25/2023 5:14:23 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[CityById]
    @Id int
AS
BEGIN

	SET NOCOUNT ON;

   	SELECT *
	FROM Cities
	WHERE Id=@Id
END
GO


