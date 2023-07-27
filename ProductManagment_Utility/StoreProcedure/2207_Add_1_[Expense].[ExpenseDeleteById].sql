USE [ProductManagment_WOEF]
GO

/****** Object:  StoredProcedure [dbo].[ExpenseDeleteById]    Script Date: 7/24/2023 6:30:36 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[ExpenseDeleteById]
    @Id int
AS
BEGIN

	SET NOCOUNT ON;

  DELETE Expenses
	WHERE Id= @Id

END
GO


