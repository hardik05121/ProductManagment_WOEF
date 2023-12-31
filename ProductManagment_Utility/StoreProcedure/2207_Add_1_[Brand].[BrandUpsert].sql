USE [ProductManagment_WOEF]
GO
/****** Object:  StoredProcedure [dbo].[Upsert]    Script Date: 7/22/2023 12:40:47 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[Upsert]
   @Id int,
	@BrandName nvarchar(50),
	@BrandImage nvarchar(450)
AS
BEGIN

	SET NOCOUNT ON;

 IF @Id = 0
    BEGIN 
        -- Check if the BrandName already exists
        IF NOT EXISTS (SELECT * FROM Brands WHERE BrandName = @BrandName)
        BEGIN
            INSERT INTO Brands(BrandName, BrandImage)
            VALUES(@BrandName, @BrandImage)
        END
        ELSE
        BEGIN
            -- Handle the case when the BrandName already exists
            RAISERROR('Brand with the same name already exists.', 16, 1);
        END
    END
    ELSE
    BEGIN
        -- Check if the BrandName already exists for a different brand (when performing an update)
        IF NOT EXISTS (SELECT * FROM Brands WHERE BrandName = @BrandName AND Id <> @Id)
        BEGIN
            UPDATE Brands
            SET
                BrandName = @BrandName,
                BrandImage = @BrandImage
            WHERE Id = @Id
        END
        ELSE
        BEGIN
            -- Handle the case when the BrandName already exists for a different brand
            RAISERROR('Brand with the same name already exists for a different brand.', 16, 1);
        END
    END
END
