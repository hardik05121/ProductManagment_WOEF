
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[UnitUpsert]
   @Id int,
	@UnitName nvarchar(50),
	@BaseUnit nvarchar(50),
	@UnitCode int
AS
BEGIN

	SET NOCOUNT ON;

 IF @Id = 0
    BEGIN 
        -- Check if the BrandName already exists
        IF NOT EXISTS (SELECT * FROM Units WHERE UnitName = @UnitName)
        BEGIN
            INSERT INTO Units(UnitName, BaseUnit, UnitCode)
            VALUES(@BaseUnit, @BaseUnit, @UnitCode)
        END
        ELSE
        BEGIN
            -- Handle the case when the BrandName already exists
            RAISERROR('Unit with the same name already exists.', 16, 1);
        END
    END
    ELSE
    BEGIN
        -- Check if the BrandName already exists for a different brand (when performing an update)
        IF NOT EXISTS (SELECT * FROM Units WHERE UnitName = @UnitName AND Id <> @Id)
        BEGIN
            UPDATE Units
            SET
               UnitName = @UnitName,
                BaseUnit = @BaseUnit,
				UnitCode = @UnitCode
            WHERE Id = @Id
        END
        ELSE
        BEGIN
            -- Handle the case when the BrandName already exists for a different brand
            RAISERROR('Unit with the same name already exists for a different Unit.', 16, 1);
        END
    END
END
