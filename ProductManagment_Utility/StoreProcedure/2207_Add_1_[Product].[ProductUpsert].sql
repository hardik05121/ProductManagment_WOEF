USE [ProductManagment_WOEF]
GO

/****** Object:  StoredProcedure [dbo].[ProductUpsert]    Script Date: 7/26/2023 12:23:03 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[ProductUpsert]
@Id int,
	@Name nvarchar(50),
	@Code nvarchar(50),
	@BrandId int,
	@CategoryId int,
	@UnitId int,
	@WarehouseId int,
	@TaxId int,
	@SkuCode nvarchar(50),
	@SkuName nvarchar(50),
	@SalesPrice float,
	@PurchasePrice float,
	@MRP float,
	@BarcodeNumber bigint,
	@Description nvarchar(50),
	@IsActive bit,
	@CreatedDate datetime2(7),
	@UpdatedDate datetime2(7),
	@ProductImage nvarchar(450)



AS
BEGIN

	SET NOCOUNT ON;

 IF @Id = 0
    BEGIN 
        -- Check if the BrandName already exists
        IF NOT EXISTS (SELECT * FROM Products WHERE Name = @Name)
        BEGIN
            INSERT INTO Products(Name,Code,BrandId,CategoryId,UnitId,WarehouseId,TaxId,SkuCode,SkuName,SalesPrice,PurchasePrice,MRP,BarcodeNumber,Description,IsActive,CreatedDate,UpdatedDate)
            VALUES(@Name,@Code,@BrandId,@CategoryId,@UnitId,@WarehouseId,@TaxId,@SkuCode,@SkuName,@SalesPrice,@PurchasePrice,@MRP,@BarcodeNumber,@Description,@IsActive,@CreatedDate,@UpdatedDate)
        END
        ELSE
        BEGIN
            -- Handle the case when the BrandName already exists
            RAISERROR('Product with the same name already exists.', 16, 1);
        END
    END
    ELSE
    BEGIN
        -- Check if the BrandName already exists for a different brand (when performing an update)
        IF NOT EXISTS (SELECT * FROM Products WHERE Name = @Name AND Id <> @Id)
        BEGIN
            UPDATE Products
            SET
                Name = @Name,
				Code = @Code,
				BrandId = @BrandId,
				CategoryId = @CategoryId,
				UnitId = @UnitId,
				WarehouseId = @WarehouseId,
				TaxId = @TaxId,
				SkuCode = @SkuCode,
				SkuName = @SkuName,
				SalesPrice = @SalesPrice,
				PurchasePrice = @PurchasePrice,
				MRP = @MRP,
				BarcodeNumber = @BarcodeNumber,
				Description = @Description,
				IsActive = @IsActive,
				CreatedDate = @CreatedDate,
				UpdatedDate = @UpdatedDate

            WHERE Id = @Id
        END
        ELSE
        BEGIN
            -- Handle the case when the BrandName already exists for a different brand
            RAISERROR('Product with the same name already exists for a different Id.', 16, 1);
        END
    END
END
GO


