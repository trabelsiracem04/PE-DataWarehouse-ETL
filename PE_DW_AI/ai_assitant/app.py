import pyodbc

conn = pyodbc.connect(
    "DRIVER={ODBC Driver 17 for SQL Server};"
    "SERVER=localhost;"
    "DATABASE=MyDataWarehouse;"
    "Trusted_Connection=yes;"
)
cursor = conn.cursor()

cursor.execute("SELECT COUNT(*) FROM FactSales")

print(cursor.fetchone())