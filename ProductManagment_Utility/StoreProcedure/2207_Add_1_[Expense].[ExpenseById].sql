USE [ProductManagment_WOEF]
GO

/****** Object:  StoredProcedure [dbo].[ExpenseById]    Script Date: 7/24/2023 6:31:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[ExpenseById]
    @Id int
AS
BEGIN

	SET NOCOUNT ON;

   	SELECT *
	FROM Expenses
	WHERE Id=@Id
END
GO


