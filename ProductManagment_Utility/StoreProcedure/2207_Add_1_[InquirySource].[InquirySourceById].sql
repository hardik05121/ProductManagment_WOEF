USE [ProductManagment_WOEF]
GO
/****** Object:  StoredProcedure [dbo].[BrandById]    Script Date: 7/25/2023 6:37:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[InquirySourceById]
    @Id int
AS
BEGIN

	SET NOCOUNT ON;

   	SELECT *
	FROM InquirySources
	WHERE Id=@Id
END
