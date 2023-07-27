USE [ProductManagment_WOEF]
GO

/****** Object:  StoredProcedure [dbo].[ProductDeleteById]    Script Date: 7/24/2023 6:23:22 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[ProductDeleteById]
    @Id int
AS
BEGIN

	SET NOCOUNT ON;

  DELETE Products
	WHERE Id= @Id

END
GO


