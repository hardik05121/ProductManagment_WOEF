
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[TaxUpsert]
   @Id int,
	@Name nvarchar(50),
	@Percentage float
AS
BEGIN

	SET NOCOUNT ON;

 IF @Id = 0
    BEGIN 
        -- Check if the BrandName already exists
        IF NOT EXISTS (SELECT * FROM Taxs WHERE Name = @Name)
        BEGIN
            INSERT INTO Taxs(Name, Percentage)
            VALUES(@Name, @Percentage)
        END
        ELSE
        BEGIN
            -- Handle the case when the BrandName already exists
            RAISERROR('Tax with the same name already exists.', 16, 1);
        END
    END
    ELSE
    BEGIN
        -- Check if the BrandName already exists for a different brand (when performing an update)
        IF NOT EXISTS (SELECT * FROM Taxs WHERE Name = @Name AND Id <> @Id)
        BEGIN
            UPDATE Taxs
            SET
                Name = @Name,
                Percentage = @Percentage
            WHERE Id = @Id
        END
        ELSE
        BEGIN
            -- Handle the case when the BrandName already exists for a different brand
            RAISERROR('Brand with the same name already exists for a different brand.', 16, 1);
        END
    END
END
