ALTER TABLE FactSales
ADD CONSTRAINT FK_FactSales_Employee FOREIGN KEY (EmployeeKey) REFERENCES DimEmployee(EmployeeKey),
	CONSTRAINT FK_FactSales_product FOREIGN KEY (ProductKey) REFERENCES DimProducts(Productkey),
	CONSTRAINT FK_FactSlaes_Reseller FOREIGN KEY (ResellerKey) REFERENCES DimResellers(ResellerKEY),
	CONSTRAINT FK_FactSales_Currencies FOREIGN KEY (CurrencyKey) REFERENCES DimCurrencies(CurrencyKey),
	CONSTRAINT FK_FactSales_Promotion FOREIGN KEY (PromotionKey) REFERENCES DimPromotion(PromotionKey),
	CONSTRAINT FK_FactSales_SalesTerritory FOREIGN KEY (SalesTerritoryKey) REFERENCES DimSalesTerritory(SalesTerritoryKey);