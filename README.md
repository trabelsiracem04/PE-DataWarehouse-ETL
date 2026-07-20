# SQL Server Data Warehouse ETL + Web Dashboard

## Overview

This project implements a complete ETL pipeline using **SQL Server Integration Services (SSIS)** to populate a SQL Server Data Warehouse from multiple CSV/flat files. The pipeline follows a classic star schema design, loading dimension tables first, then the fact table(s) that reference them.

On top of the warehouse, a **web dashboard** built with React and .NET provides real-time BI visualizations via an SSAS OLAP cube and detailed tabular reports queried directly from the relational data warehouse.

## Technologies

- SQL Server
- SQL Server Integration Services (SSIS)
- T-SQL
- Visual Studio 2022 (SSDT)
- SQL Server Analysis Services (SSAS) — OLAP cube
- **.NET 10** — Web API backend
- **React 19 + TypeScript** — Frontend SPA
- **Recharts** — Charting library
- **chroma-js** — Color scale generation
- **QuestPDF** — PDF report export
- **ClosedXML** — Excel report export

## Features

- CSV / flat file extraction
- Data type conversion
- Sorting & deduplication
- Merge Join
- Lookup transformations
- Dimension table loading
- Fact table loading
- Foreign key relationships between fact and dimension tables
- OLAP cube built on top of the warehouse (SSAS)
- **Interactive web dashboard** with 9 live chart panels
- **5 tabular reports** with Excel & PDF export

---

## Star Schema

![Star Schema](images/star_schema.png)

**Fact table:** `FactSales`

**Dimensions:**
- `DimDate`
- `DimEmployee`
- `DimProducts`
- `DimResellers`
- `DimPromotion`
- `DimCurrencies`
- `DimSalesTerritory`

## ETL Pipeline

![fact pipeline](images/fact_pipeline.png)
![dimension pipeline](images/dimension_pipeline.png)

---

## Prerequisites

- SQL Server (with a running instance and a database created for the warehouse)
- SQL Server Analysis Services (SSAS) with the cube deployed
- Visual Studio 2022 with SQL Server Integration Services projects extension installed
- Source data files placed locally (see `data/` above)
- **Node.js 18+** (for the frontend)
- **.NET 10 SDK** (for the backend)

## How to Run — ETL Pipeline

1. Create the database.
2. Execute the SQL scripts in `sql/` to create tables, primary keys, and foreign key constraints.
3. Place the source CSV/flat files inside `data/`.
4. Open the SSIS project (`PE_DW_ETL/`) in Visual Studio.
5. Update the connection managers if your file paths or SQL Server instance differ from the defaults.
6. Execute `Dimensions.dtsx` first — this loads all dimension tables.
7. Execute `Fact.dtsx` — this loads the fact table(s), which depend on the dimensions being populated first.

## Cube

An SSAS cube (`My Data Warehouse`) is built on top of the warehouse, with `FactSales` as the measure group and each `Dim*` table as a dimension, including a Date dimension linked via the `FactSales` → `DimDate` foreign key.

**Measures:**
- Total Revenue — `SUM(FactSales.TotalAmount)`
- Total Quantity — `SUM(FactSales.OrderQuantity)`
- Order Count — `COUNT(FactSales.SaleID)`

**Dimension attributes available for slicing:** Product Name, Employee Name, Territory Region, Promotion Name, Currency, Date (Year, Quarter, Month).

---

## Web Dashboard

### How to Run — Backend

```bash
cd PE_DW_Web/backend
dotnet run
```

The API starts on `http://localhost:5125`.

### How to Run — Frontend

```bash
cd PE_DW_Web/frontend
npm install
npm run dev
```

The dev server starts on `http://localhost:5173`.

### Configuration

Edit `PE_DW_Web/backend/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DataWarehouse": "Data Source=localhost;Initial Catalog=MyDataWarehouse;Integrated Security=True;TrustServerCertificate=True"
  },
  "Ssas": {
    "Server": "localhost\\SQL2022",
    "Catalog": "PE_BI",
    "Cube": "My Data Warehouse"
  }
}
```

- `ConnectionStrings:DataWarehouse` — SQL Server relational database (used for tabular reports)
- `Ssas:Server` — SSAS instance (used for dashboard chart data)
- `Ssas:Cube` — deployed cube name

The frontend API base URL defaults to `http://localhost:5125`. Override via the `VITE_API_BASE_URL` environment variable or `.env` file.

### Dashboard Page (`/`)

Displays 9 real-time chart panels (KPI cards, revenue trends, territory/currency/product/employee breakdowns) powered by the SSAS cube via MDX queries.

### Reports Page (`/reports`)

Provides 5 tabular reports (Sales Detail, Product Performance, Employee Sales, Territory Summary, Promotion Effectiveness) with date/territory/product filters and export to **Excel (.xlsx)** or **PDF**.
