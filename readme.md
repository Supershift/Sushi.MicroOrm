# Sushi MicroOrm - a fast and easy .Net Standard object mapper
## Features
Sushi MicroOrm is a NuGet library that allows you to easily map objects to a Sql Server database. Queries and mappings can be defined typesafe using lambda expressions.
## Quick start
Map your object to a table:
```csharp
public class Product
{
	public class ProductMap : DataMap<Product>
	{
		public ProductMap()
		{
			Table("Products");
			Id(x => x.ID, "ID");
			Map(x => x.Name, "Name");			
			Map(x => x.Price, "Price");						
		}
	}

	public int ID { get; set; }
	public string Name { get; set; }	
	public decimal Price { get; set; }	
}
```
And then retrieve it:
```csharp
int productID = 1240;
var connector = new Connector<Product>();
var product = connector.FetchSingle(productID);
```
## Mapping
You can map a class to a table by defining a mapping class. In the mapping class' constructor you can map properties to columns using lambda expressions.

Given the following table 'Products':
Column name | Column type |        |
----------- | ----------- | ------ 
ID | int | PK, not null
Name | nvarchar(4000) | not null
Description | varchar(4000) | not null
Price | decimal(15,3) | not null
ExternalID | int | null
BarCode | varbinary(4000) | not null
GUID | uniqueidentifier | not null
ProductTypeID | int | null

You can map this using the following C# code:
```csharp
public class Product
{
	public class ProductMap : DataMap<Product>
	{
		public ProductMap()
		{
			Table("Products");
			Id(x => x.ID, "ID").Identity();
			Map(x => x.Name, " Name").SqlType(System.Data.SqlDbType.NVarChar);
			Map(x => x.Description, " Description").SqlType(System.Data.SqlDbType.VarChar);
			Map(x => x.Price, "Price");
			Map(x => x.ExternalID, " ExternalID");
			Map(x => x.BarCode, " BarCode");
			Map(x => x.GUID, " Guid");
			Map(x => x.ProductTypeID, " ProductTypeID");
		}
	}

	public int ID { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public decimal Price { get; set; }
	public int? ExternalID { get; set; }
	public byte[] BarCode { get; set; }
	public Guid GUID { get; set; }
	public ProducType? ProductTypeID { get; set; }

	public enum ProducType
	{
		Hifi = 1,
		Computer = 2,
		Living = 3,
		Food = 4
	}
}
```

Sushi Micro ORM will automatically deduce the SQL DB Type from the type of your property. You can also set the SQL DB Type explicitly. For strings it is best practice to do this. Strings are mapped to NVarChar by default, which can lead to degraded performance when filtering on columns that are VarChar in the database.

## Querying
You can create a Connector<T> object to query the database. Use the generic paramter T specify the type of the objects you want your connector to return. Class T will be scanned for a subclass of type DataMap to use as mapping. You can also supply a mapping object explicitly when creating the connector object.

A simple example to fetch all objects in a table:
```csharp
var connector = new Connector<Product>();
var products = connector.FetchAll();
```
This will create an object 'products' of type List<Product>.

You can add a where clause to your query by creating a filter object:
```csharp
var connector = new Connector<Product>();
var filter = connector.CreateDataFilter();
filter.Add(x => x.Price, 15);
var products = connector.FetchAll(filter);
```
This will fetch all rows from the database where Price = 15.
## Write your own query
You can write your own queries and let Sushi Micro ORM do the mapping for you. It is best practice to use SQL Parameters and never use user input directly in your queries.
```csharp
int productID = 1;

var query = @"
SELECT COUNT(*)
FROM cat_Products
WHERE Product_Key = @productID";

var connector = new Connector<Product>();

var filter = new DataFilter<Product>();
filter.AddParameter("@productID", System.Data.SqlDbType.Int, productID);

var product = connector.FetchSingle(query, filter);
```
## Write your own clause
You can add custom SQL to your where clauses. You can mix them with lambda expressions:
```csharp
var connector = new Connector<Product>();

var filter = connector.CreateDataFilter();
filter.AddSql("LEN(Product_Name) > @length");
filter.AddParameter("@length", 12);
filter.Add(x => x.Price, 1, ComparisonOperator.GreaterThanOrEquals);

var products = connector.FetchAll(filter);
```
## Insert and update
For tables with an identity generated primary key Sushi MicroOrm can determine if a write operation is an insert or update. You can call the Save() method on the connector:
```csharp
var connector = new Connector<Product>();
var product = new Product()
{
	Description = "New insert test",
	Name = "New insert",
	ExternalID = null,
	Price = 12.50M,
	BarCode = Encoding.UTF8.GetBytes("SKU-12345678"),
	GUID = Guid.NewGuid(),
	ProductTypeID = Product.ProducType.Hifi
};
connector.Save(product);
```
If your primary key property is set to the default value 0 an insert will be performed. If the primary key property has is not 0, an update will be performed.

You can also explicitly call insert or update:
```csharp
var connector = new Connector<Product>();
connector.Insert(product);
product.Name = "A new name";
connector.Update(product);
```
## Configure connection string
You can set the default connection string you want your connectors to use on the static class DatabaseConfiguration:
```csharp
string connectionString = "Server=yourSever.database.windows.net;Initial Catalog=yourDatabase;Persist Security Info=False;User ID=yourUserID;Password=yourPassword;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
DatabaseConfiguration.SetDefaultConnectionString(connectionString);
```
It is advised to set this once in the startup logic for your application.

You can add multiple connection strings and map them to specific types and namespaces. 
You can also explicitly provide a connection string when creating a Connector instance.