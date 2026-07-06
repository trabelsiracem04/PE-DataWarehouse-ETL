USE MyDataWarehouse;
ALTER TABLE FactSales
ADD TotalAmount AS (UnitPrice * OrderQuantity);