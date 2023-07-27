
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create PROCEDURE [dbo].[WarehouseUpsert]
   @Id int,
	@WarehouseName nvarchar(50),
	@ContactPerson bigint,
	@MobileNumber bigint,
	@Email nvarchar(50),
	@Address nvarchar(50),
	@IsActive bit
AS
BEGIN

	SET NOCOUNT ON;

 IF @Id = 0
    BEGIN 
        -- Check if the BrandName already exists
        IF NOT EXISTS (SELECT * FROM Warehouses WHERE WarehouseName = @WarehouseName)
        BEGIN
            INSERT INTO Warehouses(WarehouseName, ContactPerson, MobileNumber, Email, Address, IsActive)
            VALUES(@WarehouseName, @ContactPerson, @MobileNumber, @Email, @Address, @IsActive)
        END
        ELSE
        BEGIN
            -- Handle the case when the BrandName already exists
            RAISERROR('Warehouse with the same name already exists.', 16, 1);
        END
    END
    ELSE
    BEGIN
        -- Check if the BrandName already exists for a different brand (when performing an update)
        IF NOT EXISTS (SELECT * FROM Warehouses WHERE WarehouseName = @WarehouseName AND Id <> @Id)
        BEGIN
            UPDATE Warehouses
            SET
                WarehouseName = @WarehouseName,
                ContactPerson = @ContactPerson,
				MobileNumber = @MobileNumber,
				Email = @Email,
				Address = @Address,
				IsActive = @IsActive

            WHERE Id = @Id
        END
        ELSE
        BEGIN
            -- Handle the case when the BrandName already exists for a different brand
            RAISERROR('Warehouse with the same name already exists for a different Warehouse.', 16, 1);
        END
    END
END
