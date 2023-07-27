
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CategoryUpsert]
   @Id int,
	@Name nvarchar(50),
	@Description nvarchar(450),
	@IsActive bit
AS
BEGIN

	SET NOCOUNT ON;

 IF @Id = 0
    BEGIN 
        -- Check if the BrandName already exists
        IF NOT EXISTS (SELECT * FROM Categories WHERE Name = @Name)
        BEGIN
            INSERT INTO Categories(Name,Description,IsActive)
            VALUES(@Name, @Description,@IsActive)
        END
        ELSE
        BEGIN
            -- Handle the case when the BrandName already exists
            RAISERROR('Category with the same name already exists.', 16, 1);
        END
    END
    ELSE
    BEGIN
        -- Check if the BrandName already exists for a different brand (when performing an update)
        IF NOT EXISTS (SELECT * FROM Categories WHERE Name = @Name AND Id <> @Id)
        BEGIN
            UPDATE Categories
            SET
                Name = @Name,
                Description = @Description
            WHERE Id = @Id
        END
        ELSE
        BEGIN
            -- Handle the case when the BrandName already exists for a different brand
            RAISERROR('Category with the same name already exists for a different brand.', 16, 1);
        END
    END
END
