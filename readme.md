# Sushi MicroOrm - a fast and easy .Net object mapper
[![NuGet version (Sushi.MicroOrm)](https://img.shields.io/nuget/v/Sushi.MicroOrm.svg?style=flat-square)](https://www.nuget.org/packages/Sushi.MicroOrm/)
[![Build status](https://dev.azure.com/supershift/Mediakiwi/_apis/build/status/Micro%20ORM)](https://dev.azure.com/supershift/Mediakiwi/_build/latest?definitionId=38)
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
			Id(x => x.Id, "Id");
			Map(x => x.Name, "Name");			
			Map(x => x.Price, "Price");						
		}
	}

	public int Id { get; set; }
	public string Name { get; set; }	
	public decimal Price { get; set; }	
}
```
And then retrieve it:
```csharp
var query = _connector.CreateQuery();
query.Add(x => x.Id, 1204);
var product = await connector.GetFirstAsync(query);
```
## Mapping
You can map a class to a table by defining a mapping class. In the mapping class' constructor you can map properties to columns using lambda expressions.

Given the following table 'Products':

| Column name | Column type |        |
| ----------- | ----------- | ------ |
| Id | int | PK, not null |
| Name | nvarchar(4000) | not null |
| Description | varchar(4000) | not null |
| Price | decimal(15,3) | not null |
| ExternalId | int | null |
| BarCode | varbinary(4000) | not null |
| GUID | uniqueidentifier | not null |
| ProductTypeID | int | null |

You can map this using the following C# code:
```csharp
public class Product
{
	public class ProductMap : DataMap<Product>
	{
		public ProductMap()
		{
			Table("Products");
			Id(x => x.Id, "Id").Identity();
			Map(x => x.Name, " Name").SqlType(System.Data.SqlDbType.NVarChar);
			Map(x => x.Description, " Description").SqlType(System.Data.SqlDbType.VarChar);
			Map(x => x.Price, "Price");
			Map(x => x.ExternalId, " ExternalId");
			Map(x => x.BarCode, " BarCode");
			Map(x => x.GUID, " Guid");
			Map(x => x.ProductTypeID, " ProductTypeID");
		}
	}

	public int Id { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public decimal Price { get; set; }
	public int? ExternalId { get; set; }
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
You can use a Connector<T> object to query the database. Use the generic paramter T specify the type of the objects you want your connector to return. Class T will be scanned for a subclass of type DataMap to use as mapping. 

A simple example to get all objects in a table:
```csharp
var query = _connector.CreateQuery();
var products = await _connector.GetAllAsync(query);
```
This will create an object 'products' of type QueryListResult<Product>. QueryListResult inherits .NET's generic List.

You can add a where clause to your query:
```csharp
var query = _connector.CreateQuery();
query.Add(x => x.Price, 15);
var products = await _connector.GetAllAsync(filter);
```
This will get all rows from the database where Price = 15.
## Get a connector object
Use dependency injection to get an instance of Connector<T>:
```csharp
public class MyController
{
	private readonly Connector<Product> _controller;

	public MyController(Connector<Product> controller)
	{
		_controller = controller;
	}
}
```
Set up dependency injection in your startup:
```csharp
var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var services = builder.Services;

var connectionString = configuration.GetConnectionString("MyDatabase");

service.AddMicroORM(connectionString);
```
## Write your own query
You can write your own queries and let Sushi Micro ORM do the mapping for you. It is best practice to use SQL Parameters and never use user input directly in your queries.
```csharp
int productId = 1;

var query = _connector.CreateQuery();
query.SqlText = @"
SELECT COUNT(*)
FROM Products
WHERE Product_Id = @productId";
query.AddParameter("@productId", productId);

var product = await _connector.GetSingleAsync(query);
```
## Write your own clause
You can add custom SQL to your where clauses. You can mix them with lambda expressions:
```csharp
var query = _connector.CreateQuery();
query.AddSql("LEN(Product_Name) > @length");
query.AddParameter("@length", 12);
query.Add(x => x.Price, 1, ComparisonOperator.GreaterThanOrEquals);

var products = await connector.GetAllAsync(query);
```
## Insert and update
For tables with an identity generated primary key Sushi MicroOrm can determine if a write operation is an insert or update. You can call the Save() method on the connector:
```csharp

var product = new Product()
{
	Description = "New insert test",
	Name = "New insert",
	ExternalId = null,
	Price = 12.50M,
	BarCode = Encoding.UTF8.GetBytes("SKU-12345678"),
	GUID = Guid.NewGuid(),
	ProductTypeID = Product.ProducType.Hifi
};
await _connector.SaveAsync(product);
```
If your primary key property is set to the default value 0 an insert will be performed. If the primary key property has is not 0, an update will be performed.

You can also explicitly call insert or update:
```csharp
await _connector.InsertAsync(product);
product.Name = "A new name";
await _connector.UpdateAsync(product);
```
## Configuration
Use the MicroORM configuration builder to customize configuration:
```csharp
serviceCollection.AddMicroORM(connectionString, c =>
{
    c.Options = o =>
    {
        o.DefaultCommandTimeOut = 45;
    };
	c.ConnectionStringProvider.AddMappedConnectionString<LogItem>(loggingConnectionString);
});
```
You can add multiple connection strings and map them to specific types and namespaces. 

You can override the default connection string and command timeout on the DataQuery object:
```csharp
var query = _connector.CreateQuery();
query.CommandTimeout = 5;
query.ConnectionString = myOverrideConnectionString;
var result = await _connector.GetAllAsync(query);
```