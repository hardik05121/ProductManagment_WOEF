
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ExpenseCategoryUpsert]
    @Id int,
	@ExpenseCategoryName nvarchar(50),
	@IsActive bit
AS
BEGIN

	SET NOCOUNT ON;

 IF @Id = 0
    BEGIN 
        -- Check if the BrandName already exists
        IF NOT EXISTS (SELECT * FROM ExpenseCategories WHERE ExpenseCategoryName = @ExpenseCategoryName)
        BEGIN
            INSERT INTO ExpenseCategories(ExpenseCategoryName, IsActive)
            VALUES(@ExpenseCategoryName, @IsActive)
        END
        ELSE
        BEGIN
            -- Handle the case when the BrandName already exists
            RAISERROR('ExpenseCategory with the same name already exists.', 16, 1);
        END
    END
    ELSE
    BEGIN
        -- Check if the BrandName already exists for a different brand (when performing an update)
        IF NOT EXISTS (SELECT * FROM ExpenseCategories WHERE ExpenseCategoryName = @ExpenseCategoryName AND Id <> @Id)
        BEGIN
            UPDATE ExpenseCategories
            SET
                ExpenseCategoryName = @ExpenseCategoryName,
                IsActive = @IsActive
            WHERE Id = @Id
        END
        ELSE
        BEGIN
            -- Handle the case when the BrandName already exists for a different brand
            RAISERROR('ExpenseCategory with the same name already exists for a different ExpenseCategory.', 16, 1);
        END
    END
END
