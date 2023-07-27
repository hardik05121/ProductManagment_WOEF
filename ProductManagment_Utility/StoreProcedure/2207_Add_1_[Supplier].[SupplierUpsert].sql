
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SupplierUpsert]    @Id int,	@SupplierName nvarchar(50),	@ContactPerson nvarchar(50),	@WebSite nvarchar(450),	@MobileNumber bigint,	@PhoneNumber bigint,	@Address nvarchar(450),	@CountryId int,	@StateId int,	@CityId int,	@BillingAddress nvarchar(450),	@BillingCountryId int,	@BillingStateId int,	@BillingCityId int,	@ShippingAddress nvarchar(450),	@ShippingCountryId int,	@ShippingStateId int,	@ShippingCityId int,	@Description nvarchar(450),	@SupplierImage nvarchar(450)ASBEGIN	SET NOCOUNT ON; IF @Id = 0    BEGIN         -- Check if the CityName already exists        IF NOT EXISTS (SELECT * FROM Suppliers WHERE SupplierName = @SupplierName)        BEGIN            INSERT INTO Suppliers(SupplierName,ContactPerson,WebSite,MobileNumber,PhoneNumber,Address,CountryId,StateId,CityId,BillingAddress,BillingCountryId,BillingStateId,BillingCityId,ShippingAddress,ShippingCountryId,ShippingStateId,ShippingCityId,Description,SupplierImage)            VALUES(@SupplierName,@ContactPerson,@WebSite,@MobileNumber,@PhoneNumber,@Address,@CountryId,@StateId,@CityId,@BillingAddress,@BillingCountryId,@BillingStateId,@BillingCityId,@ShippingAddress,@ShippingCountryId,@ShippingStateId,@ShippingCityId,@Description,@SupplierImage)        END        ELSE        BEGIN            -- Handle the case when the BrandName already exists            RAISERROR('Supplier with the same name already exists.', 16, 1);        END    END    ELSE    BEGIN        -- Check if the BrandName already exists for a different brand (when performing an update)        IF NOT EXISTS (SELECT * FROM Suppliers WHERE SupplierName = SupplierName AND Id <> @Id)        BEGIN            UPDATE Suppliers            SET               				SupplierName = @SupplierName,				ContactPerson = @ContactPerson,				WebSite = @WebSite,				MobileNumber = @MobileNumber,				PhoneNumber = @PhoneNumber,				Address = @Address,	            CountryId = @CountryId,				StateId = @StateId,	            CityId = @CityId,				BillingAddress = @BillingAddress,	            BillingCountryId = @BillingCountryId,			 	BillingStateId = @BillingStateId,	            BillingCityId = @BillingCityId,				ShippingAddress = @ShippingAddress,				@BillingCountryId = @ShippingCountryId,				ShippingStateId = @ShippingStateId,				ShippingCityId = @ShippingCityId,				Description = @Description,				SupplierImage = @SupplierImage            WHERE Id = @Id        END        ELSE        BEGIN            -- Handle the case when the BrandName already exists for a different brand            RAISERROR('Supplier with the same name already exists for a different State.', 16, 1);        END    ENDEND
