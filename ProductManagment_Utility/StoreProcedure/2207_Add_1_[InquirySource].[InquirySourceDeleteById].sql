
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[InquirySourceDeleteById]
    @Id int
AS
BEGIN

	SET NOCOUNT ON;

  DELETE InquirySources
	WHERE Id= @Id

END
