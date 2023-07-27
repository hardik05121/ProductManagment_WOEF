
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CountryUpsert]
   @Id int,
	@CountryName nvarchar(50),
	@IsActive bit
AS
BEGIN

	SET NOCOUNT ON;

 IF @Id = 0
    BEGIN 
        -- Check if the BrandName already exists
        IF NOT EXISTS (SELECT * FROM Countries WHERE CountryName = @CountryName)
        BEGIN
            INSERT INTO Countries(CountryName, IsActive)
            VALUES(@CountryName, @IsActive)
        END
        ELSE
        BEGIN
            -- Handle the case when the BrandName already exists
            RAISERROR('Countries with the same name already exists.', 16, 1);
        END
    END
    ELSE
    BEGIN

        IF NOT EXISTS (SELECT * FROM Countries WHERE CountryName = @CountryName AND Id <> @Id)
        BEGIN
            UPDATE Countries
            SET
                CountryName = @CountryName,
                IsActive = @IsActive
            WHERE Id = @Id
        END
        ELSE
        BEGIN
            -- Handle the case when the BrandName already exists for a different brand
            RAISERROR('Countries with the same name already exists for a different country.', 16, 1);
        END
    END
END