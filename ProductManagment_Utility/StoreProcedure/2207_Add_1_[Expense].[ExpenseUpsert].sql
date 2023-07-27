
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ExpenseUpsert]
	
    @Id int,
	@CreatedDate datetime2(7),
	@ExpenseDate datetime2(7),
	@Reference nvarchar(450),
	@Amount int,
	@ExpenseCategoryId int,
	@UserId int,
	@Note nvarchar(450),
	@ExpenseFile nvarchar(450)
	
AS
BEGIN

	SET NOCOUNT ON;

 IF @Id = 0
    BEGIN 
        -- Check if the BrandName already exists
        IF NOT EXISTS (SELECT * FROM Expenses WHERE Reference = @Reference)
        BEGIN
            INSERT INTO Expenses(CreatedDate,ExpenseDate,Reference,Amount,ExpenseCategoryId,UserId,Note,ExpenseFile)
            VALUES(@CreatedDate,@ExpenseDate,@Reference,@Amount,@ExpenseCategoryId,@UserId,@Note,@ExpenseFile)
        END
        ELSE
        BEGIN
            -- Handle the case when the BrandName already exists
            RAISERROR('Expense with the same name already exists.', 16, 1);
        END
    END
    ELSE
    BEGIN
        -- Check if the BrandName already exists for a different brand (when performing an update)
        IF NOT EXISTS (SELECT * FROM Expenses WHERE Reference = @Reference AND Id <> @Id)
        BEGIN
            UPDATE Expenses
            SET
               
				ExpenseDate = @ExpenseDate,
				Reference = @Reference,
				Amount = @Amount,
				ExpenseCategoryId = @ExpenseCategoryId,
				UserId = @UserId,
				Note = @Note,
				ExpenseFile = @ExpenseFile

            WHERE Id = @Id
        END
        ELSE
        BEGIN
            -- Handle the case when the BrandName already exists for a different brand
            RAISERROR('Expense with the same name already exists for a different state.', 16, 1);
        END
    END
END