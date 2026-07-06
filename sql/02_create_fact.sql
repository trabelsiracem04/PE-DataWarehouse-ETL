USE MyDataWarehouse;

CREATE TABLE FactSales (
    SaleID              VARCHAR(10) PRIMARY KEY,
    EmployeeKey         SMALLINT NOT NULL,
    ProductKey          SMALLINT NOT NULL,
    ResellerKey         SMALLINT NOT NULL,
    OrderDate           DATETIME NOT NULL,
    OrderQuantity       SMALLINT NOT NULL,
    UnitPrice           REAL NOT NULL,
    PromotionKey        SMALLINT NOT NULL,
    CurrencyKey         SMALLINT NOT NULL,
    SalesTerritoryKey   SMALLINT NOT NULL
);