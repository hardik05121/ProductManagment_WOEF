USE [ProductManagment_WOEF]
GO

/****** Object:  StoredProcedure [dbo].[BrandDeleteById]    Script Date: 7/22/2023 12:18:46 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[BrandDeleteById]
    @Id int
AS
BEGIN

	SET NOCOUNT ON;

  DELETE Brands
	WHERE Id= @Id

END
GO


