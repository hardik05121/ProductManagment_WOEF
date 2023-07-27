-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CustomerUpsert]    @Id int,	@CustomerName nvarchar(50),	@ContactPerson nvarchar(50),	@Email nvarchar(450),	@MobileNumber bigint,	@PhoneNumber bigint,	@WebSite nvarchar(50),	@Address nvarchar(50),	@CountryId int,	@Stateid int,	@CityId int,	@Description nvarchar(450),	@CustomerImage nvarchar(450),	@IsActive bitASBEGIN	SET NOCOUNT ON; IF @Id = 0    BEGIN         -- Check if the BrandName already exists        IF NOT EXISTS (SELECT * FROM Customers WHERE CustomerName = @CustomerName)        BEGIN            INSERT INTO Customers(CustomerName,ContactPerson,Email,MobileNumber,PhoneNumber,WebSite,Address,CountryId,StateId,CityId,Description,CustomerImage,IsActive)            VALUES(@CustomerName,@ContactPerson,@Email,@MobileNumber,@PhoneNumber,@WebSite,@Address,@CountryId,@StateId,@CityId,@Description,@CustomerImage,@IsActive)        END        ELSE        BEGIN            -- Handle the case when the BrandName already exists            RAISERROR('Customer with the same name already exists.', 16, 1);        END    END    ELSE    BEGIN        -- Check if the BrandName already exists for a different brand (when performing an update)        IF NOT EXISTS (SELECT * FROM Customers WHERE CustomerName = @CustomerName AND Id <> @Id)        BEGIN            UPDATE Customers            SET                CustomerName = @CustomerName,				ContactPerson = @ContactPerson,				Email = @Email,				MobileNumber = @MobileNumber,				PhoneNumber = @PhoneNumber,				WebSite = @WebSite,				Address = @Address,				CountryId = @CountryId,				StateId = @Stateid,				CityId = @CityId,				Description = @Description,				CustomerImage = @CustomerImage            WHERE Id = @Id        END        ELSE        BEGIN            -- Handle the case when the BrandName already exists for a different brand            RAISERROR('Customer with the same name already exists for a different brand.', 16, 1);        END    ENDEND