
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SupplierById]	@Id intASBEGIN	-- SET NOCOUNT ON added to prevent extra result sets from	-- interfering with SELECT statements.	SET NOCOUNT ON;	SELECT *	FROM Suppliers	WHERE Id=@Id	END
