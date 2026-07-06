# SQL Server Data Warehouse ETL

## Overview

This project implements a complete ETL pipeline using SQL Server Integration Services (SSIS) to populate a SQL Server Data Warehouse from multiple CSV files.

## Technologies

- SQL Server
- SQL Server Integration Services (SSIS)
- T-SQL
- Visual Studio 2022

## Features

- CSV Extraction
- Data Conversion
- Sorting
- Merge Join
- Lookup
- Dimension Loading
- Fact Table Loading

## Star Schema

![Star Schema](images/star_schema.png)

## ETL Pipeline

![fact pipeline](images/fact_pipeline.png)
![dimensionpipeline](images/dimension_pipeline.png)

## Project Structure

database/
ssis/
docs/
images/

## How to Run

1. Create the database.
2. Execute SQL scripts.
3. Place CSV files inside `/data`.
4. Open the SSIS project.
5. Execute `Dimensions.dtsx`.
6. Execute `Fact.dtsx`.