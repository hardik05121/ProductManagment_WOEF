USE [ProductManagment_WOEF]
GO

/****** Object:  StoredProcedure [dbo].[StateUpsert]    Script Date: 7/24/2023 2:16:59 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[StateUpsert]
   @Id int,
	@StateName nvarchar(50),
	@CountryId int,
	@IsActive bit
AS
BEGIN

	SET NOCOUNT ON;

 IF @Id = 0
    BEGIN 
        -- Check if the BrandName already exists
        IF NOT EXISTS (SELECT * FROM States WHERE StateName = @StateName)
        BEGIN
            INSERT INTO States(StateName,CountryId,IsActive)
            VALUES(@StateName, @CountryId,@IsActive)
        END
        ELSE
        BEGIN
            -- Handle the case when the BrandName already exists
            RAISERROR('State with the same name already exists.', 16, 1);
        END
    END
    ELSE
    BEGIN
        -- Check if the BrandName already exists for a different brand (when performing an update)
        IF NOT EXISTS (SELECT * FROM States WHERE StateName = @StateName AND Id <> @Id)
        BEGIN
            UPDATE States
            SET
                StateName = @StateName,
                CountryId = @CountryId,
				IsActive = @IsActive
            WHERE Id = @Id
        END
        ELSE
        BEGIN
            -- Handle the case when the BrandName already exists for a different brand
            RAISERROR('State with the same name already exists for a different state.', 16, 1);
        END
    END
END
GO


