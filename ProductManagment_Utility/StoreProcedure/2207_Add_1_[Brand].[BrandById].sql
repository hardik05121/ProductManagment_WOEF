USE [ProductManagment_WOEF]
GO

/****** Object:  StoredProcedure [dbo].[BrandById]    Script Date: 7/22/2023 12:17:35 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[BrandById]
    @Id int
AS
BEGIN

	SET NOCOUNT ON;

   	SELECT *
	FROM Brands
	WHERE Id=@Id
END
GO


