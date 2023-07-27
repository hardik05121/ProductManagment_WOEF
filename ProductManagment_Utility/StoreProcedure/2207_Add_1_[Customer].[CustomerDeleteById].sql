
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CustomerDeleteById] 	@Id intASBEGIN	-- SET NOCOUNT ON added to prevent extra result sets from	-- interfering with SELECT statements.	SET NOCOUNT ON;    DELETE Customers	WHERE Id= @IdEND
