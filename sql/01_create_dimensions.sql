USE MyDataWarehouse;

CREATE TABLE DimPromotion (
    PromotionKey    SMALLINT PRIMARY KEY,
    PromotionName   VARCHAR(50) NOT NULL
);

CREATE TABLE DimEmployee (
    EmployeeKey   SMALLINT PRIMARY KEY,
    FirstName     VARCHAR(50) NOT NULL,
    LastName      VARCHAR(50) NOT NULL,
    Title         VARCHAR(100) NULL
);

CREATE TABLE DimProducts (
    ProductKey    SMALLINT PRIMARY KEY,
    ProductName   VARCHAR(50) NOT NULL
);

CREATE TABLE DimCurrencies (
    CurrencyKey            SMALLINT PRIMARY KEY,
    CurrencyAlternateKey   VARCHAR(50) NOT NULL
);

CREATE TABLE DimSalesTerritory (
    SalesTerritoryKey    SMALLINT PRIMARY KEY,
    SalesTerritoryRegion  VARCHAR(50) NOT NULL,
    SalesTerritoryCountry   VARCHAR(50) NOT NULL,
    SalesTerritoryGroup   VARCHAR(50) NOT NULL
);

CREATE TABLE DimResellers (
    ResellerKey    SMALLINT PRIMARY KEY,
    ResellerName   VARCHAR(50) NOT NULL
);