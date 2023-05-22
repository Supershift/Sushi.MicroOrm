using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sushi.MicroORM.Mapping;
using Sushi.MicroORM.Supporting;
using Sushi.MicroORM.ManualTests.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using NuGet.Frameworks;

namespace Sushi.MicroORM.ManualTests
{
    [TestClass]
    public class ConnectorTest
    {
        private readonly IConnector<Order> _connectorOrders;
        private readonly IConnector<Product> _connectorProducts;                
        private readonly ServiceProvider _serviceProvider;

        public ConnectorTest()
        {
            var configuration = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
            .AddEnvironmentVariables()
            .Build();

            // get connection strings
            string connectionString = configuration.GetConnectionString("TestDatabase");
            var connectionString2 = configuration.GetConnectionString("Customers");
            var connectionString3 = configuration.GetConnectionString("Addresses");

            // register dependencies
            IServiceCollection serviceCollection = new ServiceCollection();

            // add micro orm
            serviceCollection.AddMicroORM(connectionString, c =>
            {
                c.ConnectionStringProvider.AddMappedConnectionString("Sushi.MicroORM.ManualTests.DAL.Customers", connectionString2);
                c.ConnectionStringProvider.AddMappedConnectionString(typeof(DAL.Customers.Address), connectionString3);
            });

            // build provider
            _serviceProvider = serviceCollection.BuildServiceProvider();

            // create default connector
            _connectorOrders = CreateConnector<Order>();
            _connectorProducts = CreateConnector<Product>();
        }

        [TestMethod]
        public async Task GetSingleByFilterAsync()
        {
            int id = 2;

            var filter = _connectorOrders.CreateQuery();
            filter.Add(x => x.ID, id);

            var order = await _connectorOrders.GetFirstAsync(filter);

            Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");

            Assert.AreEqual(id, order?.ID);
        }

        [TestMethod]
        public async Task GetSingleBySqlAsync()
        {
            int id = 3;

            string sql = $"SELECT * FROM cat_Orders WHERE Order_Key = {id}"; //this is BAD PRACTICE! always use parameters
            var query = _connectorOrders.CreateQuery();
            query.SqlQuery = sql;

            var order = await _connectorOrders.GetFirstAsync(query);

            Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");

            Assert.IsTrue(order?.ID == id);
        }

        [TestMethod]
        public async Task GetSingleBySqlAndFilterAsync()
        {
            int id = 3;

            string sql = $"SELECT * FROM cat_Orders WHERE Order_Key = @orderID"; //this is BAD PRACTICE! always use parameters

            var query = _connectorOrders.CreateQuery();
            query.SqlQuery = sql;
            query.AddParameter("@orderID", id);


            var order = await _connectorOrders.GetFirstAsync(query);
            
            Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");

            Assert.IsTrue(order?.ID == id);
        }

        [TestMethod]
        public async Task GetSingleMultilevelAsync()
        {
            int productID = 1;
            var connector = CreateConnector<Product>();
            var query = connector.CreateQuery();
            query.Add(x => x.ID, productID);
            var product = await connector.GetFirstAsync(query);

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(product, Newtonsoft.Json.Formatting.Indented));

            Assert.IsTrue(product.MetaData.Identification.GUID != Guid.Empty);
        }

        [TestMethod]
        public async Task GetAllMaxResults()
        {
            int maxResults = 2;

            var connector = CreateConnector<Order>();
            var filter = connector.CreateQuery();
            filter.MaxResults = maxResults;
            var orders = await connector.GetAllAsync(filter);

            Assert.AreEqual(maxResults, orders.Count);
        }

        [TestMethod]
        public async Task GetAllByDateOnly()
        {
            var connector = CreateConnector<Order>();
            var query = connector.CreateQuery();
            query.Add(x => x.DeliveryDate, new DateOnly(2000, 1, 1), ComparisonOperator.GreaterThanOrEquals);
            var orders = await connector.GetAllAsync(query);

            Assert.IsTrue(orders.Count > 0);
        }

        [TestMethod]
        public async Task TestCancellation()
        {
            var cts = new CancellationTokenSource();
            var filter = _connectorOrders.CreateQuery();
            cts.Cancel();
            try
            {
                var orders = await _connectorOrders.GetAllAsync(filter, cts.Token);
                Assert.Fail();
            }
            catch(TaskCanceledException)
            {
                
            }
        }

        [TestMethod]
        public async Task GetAllBySqlAsync()
        {
            string sql = "SELECT TOP(3) * FROM cat_Orders";
            var query = _connectorOrders.CreateQuery();
            query.SqlQuery = sql;
            var orders = await _connectorOrders.GetAllAsync(query);
            foreach (var order in orders)
            {
                Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");
            }

            Assert.IsTrue(orders.Count > 0);
        }

        [TestMethod]
        public async Task GetAllWithJoin()
        {
            int customerID = 1;

            var connector = CreateConnector<BookedRoom>();
            var filter = connector.CreateQuery();
            filter.Add(x => x.CustomerID, customerID);
            var bookings = await connector.GetAllAsync(filter);

            Console.Write(Newtonsoft.Json.JsonConvert.SerializeObject(bookings, Newtonsoft.Json.Formatting.Indented));
            Assert.IsTrue(bookings.Count > 0);
        }

        [TestMethod]
        public async Task GetPagingAsync()
        {
            var request = _connectorOrders.CreateQuery();
            request.AddPaging(5, 2);
                
            var orders = await _connectorOrders.GetAllAsync(request);
            foreach (var order in orders)
            {
                Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");
            }
            
            Console.WriteLine("Total number of rows: " + orders.TotalNumberOfRows);
            Console.WriteLine("Total number of pages: " + orders.TotalNumberOfPages);

            Assert.IsTrue(orders.Count == request.Paging.NumberOfRows);            
            Assert.IsTrue(orders.TotalNumberOfPages.HasValue);
            Assert.IsTrue(orders.TotalNumberOfRows.HasValue);
        }

        [TestMethod]
        public async Task GetAllOrderedBy()
        {
            var ConnectorOrders = CreateConnector<Order>();

            var filter = _connectorOrders.CreateQuery();
            filter.AddOrder(x => x.ID, SortOrder.DESC);
            filter.MaxResults = 2;
            var orders = await ConnectorOrders.GetAllAsync(filter);

            foreach (var order in orders)
            {
                Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");
            }

            Assert.AreEqual(2, orders.Count);
            Assert.IsTrue(orders[0].ID > orders[1].ID);
        }

        [TestMethod]
        public async Task GetWhereInIntAsync()
        {

            var request = _connectorOrders.CreateQuery();
            //request.WhereClause.Add(new DatabaseDataValueColumn("Order_Key", System.Data.SqlDbType.Int, new int[] { 1, 2,3 }, ComparisonOperator.In));
            request.Add(x => x.ID, new int[] { 1, 2, 3 }, ComparisonOperator.In);
            var orders = await _connectorOrders.GetAllAsync(request);
            foreach (var order in orders)
            {
                Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");
            }

            Assert.IsTrue(orders.Count == 3);
        }

        [TestMethod]
        public async Task GetWhereInEmptyEnumerableAsync()
        {
            var request = _connectorOrders.CreateQuery();
            
            request.Add(x => x.ID, new int[] { }, ComparisonOperator.In);
            var orders = await _connectorOrders.GetAllAsync(request);
            foreach (var order in orders)
            {
                Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");
            }

            Assert.AreEqual(0, orders.Count);
        }

        [TestMethod]
        public async Task GetWhereInStringAsync()
        {
            var request = _connectorProducts.CreateQuery();
            var names = new string[]
            {
                "TV",
                "Laser Mouse",
                "E-reader"
            };

            request.Add(x => x.MetaData.Name, names, ComparisonOperator.In);
            var products = await _connectorProducts.GetAllAsync(request);
            foreach (var product in products)
            {
                Console.WriteLine($"{product.ID} - {product.MetaData.Name}");
            }

            Assert.IsTrue(products.Count == 3);
        }

        [TestMethod]
        public async Task GetWhereGreaterThanStringAsync()
        {
            var filter = _connectorProducts.CreateQuery();

            filter.Add(x => x.MetaData.Description, "", ComparisonOperator.GreaterThan);
            var products = await _connectorProducts.GetAllAsync(filter);
            foreach (var product in products)
            {
                Console.WriteLine($"{product.ID} - {product.MetaData.Name}");
            }

            Assert.IsTrue(products.Count > 0);
        }

        [TestMethod]
        public async Task GetNotExistingAsync()
        {
            var query = _connectorOrders.CreateQuery();
            query.Add(x => x.ID, -1);
            var order = await _connectorOrders.GetFirstAsync(query);
            Assert.IsTrue(order == null);
        }
        [TestMethod]
        public async Task ExecuteNonQueryAsync()
        {
            string name = DateTime.UtcNow.Ticks.ToString();
            int productID = 1;
            var sql = @"
UPDATE cat_Products
SET Product_Name = @name
WHERE Product_Key = @productID";
            var query = _connectorProducts.CreateQuery();
            query.AddParameter(@"name", System.Data.SqlDbType.VarChar, name);
            query.AddParameter("@productID", System.Data.SqlDbType.Int, productID);
            query.SqlQuery = sql;
            await _connectorProducts.ExecuteNonQueryAsync(query);

            //check if name was updated
            var query2 = _connectorProducts.CreateQuery();
            query2.Add(x => x.ID, productID);
            var product = await _connectorProducts.GetFirstAsync(query2);
            Assert.AreEqual(name, product.MetaData.Name);
        }

        [TestMethod]
        public async Task ExecuteScalarAsync()
        {
            string name = DateTime.UtcNow.Ticks.ToString();
            int productID = 1;
            var sql = @"
SELECT COUNT(*)
FROM cat_Products
WHERE Product_Key = @productID";
            var query = _connectorProducts.CreateQuery();
            query.SqlQuery = sql;
            query.AddParameter("@productID", System.Data.SqlDbType.Int, productID);
            var count = await _connectorProducts.ExecuteScalarAsync<int>(query);
            Assert.AreEqual(1, count);

        }

        [TestMethod]
        public async Task ExecuteSetAsync()
        {
            var ConnectorProducts = CreateConnector<Product>();
            string name = DateTime.UtcNow.Ticks.ToString();
            var sql = @"
SELECT DISTINCT(Product_ProductTypeID)
FROM cat_Products";
            var query = ConnectorProducts.CreateQuery();
            query.SqlQuery = sql;
            var productTypes = await ConnectorProducts.ExecuteSetAsync<Product.ProducType?>(query);

            foreach (var productType in productTypes)
            {
                Console.WriteLine(productType == null ? "NULL" : productType.ToString());
            }

            Assert.AreEqual(5, productTypes.Count);
        }

        [TestMethod]
        public async Task ExecuteSetWithFilterAsync()
        {   
            int productID = 1;
            var sql = @"
SELECT DISTINCT(Product_ProductTypeID)
FROM cat_Products
WHERE Product_Key > @productID";
            var query = _connectorProducts.CreateQuery();
            query.SqlQuery = sql;
            query.AddParameter("@productID", System.Data.SqlDbType.Int, productID);
            var productTypes = await _connectorProducts.ExecuteSetAsync<Product.ProducType?>(query);

            foreach (var productType in productTypes)
            {
                Console.WriteLine(productType == null ? "NULL" : productType.ToString());
            }
            Assert.AreEqual(4, productTypes.Count);
        }

        [TestMethod]
        public async Task SaveNewAsync()
        {
            var order = new Order()
            {
                CustomerID = 1,
                Created = DateTime.UtcNow,
                Created2 = DateTime.UtcNow,
                CreatedOffset = DateTimeOffset.Now,
                DeliveryTime = new TimeSpan(3, 20, 35),
                DeliveryTime2 = DateTime.UtcNow.TimeOfDay
            };
            await _connectorOrders.SaveAsync(order);

            Assert.IsTrue(order.ID > 0);
        }

        [TestMethod]
        public async Task SaveExistingAsync()
        {
            //save the order
            var query = _connectorOrders.CreateQuery();
            query.Add(x => x.ID, 20);
            var order = await _connectorOrders.GetFirstAsync(query);

            string newComments = DateTime.UtcNow.ToString();

            order.Comments = newComments;
            order.DeliveryTime = null;
            order.DeliveryTime2 = DateTime.UtcNow.TimeOfDay;
            await _connectorOrders.SaveAsync(order);

            //retrieve order again from database
            order = await _connectorOrders.GetFirstAsync(query);

            Assert.AreEqual(newComments, order.Comments);
        }

        [TestMethod]
        public async Task BulkInsertAutoIncrement()
        {
            var ConnectorOrders = CreateConnector<Order>();

            var orders = new List<Order>();

            int numberOfRows = 100;

            //assign unique ID to orders so we can check if all items were inserterd
            var uniqueID = Guid.NewGuid().ToString();
            for (int i = 0; i < numberOfRows; i++)
            {
                var order = new Order()
                {
                    CustomerID = i,
                    Created = DateTime.UtcNow,
                    DeliveryTime = new TimeSpan(3, 20, 35),
                    DeliveryTime2 = DateTime.UtcNow.TimeOfDay,
                    Comments = uniqueID
                };
                orders.Add(order);
            }

            await ConnectorOrders.BulkInsertAsync(orders);

            //retrieve orders
            var query = ConnectorOrders.CreateQuery();
            query.Add(x => x.Comments, uniqueID);
            var retrievedOrders = await ConnectorOrders.GetAllAsync(query);
            Assert.AreEqual(numberOfRows, retrievedOrders.Count);
        }

        [TestMethod]
        public async Task BulkInsertIdentityInsert()
        {
            var identifiers = new List<Identifier>();

            int numberOfRows = 100;

            //assign unique ID to identifiers so we can check if all items were inserterd
            var uniqueID = Guid.NewGuid();
            for (int i = 0; i < numberOfRows; i++)
            {
                var order = new Identifier()
                {
                    GUID = Guid.NewGuid(),
                    Batch = uniqueID
                };
                identifiers.Add(order);
            }

            var connector = CreateConnector<Identifier>();
            await connector.BulkInsertAsync(identifiers, true);

            //retrieve 
            var query = connector.CreateQuery();
            query.Add(x => x.Batch, uniqueID);
            var retrievedRows = await connector.GetAllAsync(query);
            Assert.AreEqual(numberOfRows, retrievedRows.Count);
        }

        [TestMethod]
        public async Task BulkInsertCompositeKey()
        {
            var rows = new List<CompositeKey>();

            int numberOfRows = 100;

            int offset = Guid.NewGuid().GetHashCode();
            for (int i = offset; i < offset + numberOfRows; i++)
            {
                var row = new CompositeKey()
                {
                    FirstID = i,
                    SecondID = Guid.NewGuid().GetHashCode(),
                    SomeValue = i.ToString()
                };
                rows.Add(row);
            }

            var connector = CreateConnector<CompositeKey>();
            await connector.BulkInsertAsync(rows);
        }

        [TestMethod]
        public async Task GetWithTableValuedParameterAsync()
        {
            var sproc = "EXEC sp_GetOrders @customerIDs";

            var customerTable = new System.Data.DataTable();
            customerTable.Columns.Add();
            customerTable.Rows.Add(98);
            customerTable.Rows.Add(99);

            var connector = CreateConnector<Order>();
            var query = connector.CreateQuery();
            query.SqlQuery = sproc;
            query.AddParameter("@customerIDs", customerTable, "cat_CustomerTableType");
            var orders = await connector.GetAllAsync(query);

            var count98 = orders.Count(x => x.CustomerID == 98);
            var count99 = orders.Count(x => x.CustomerID == 99);

            Assert.AreEqual(orders.Count, count98 + count99);
        }

        [TestMethod]
        public async Task InsertAsync()
        {
            var product = new Product()
            {
                MetaData = new Product.ProductMetaData()
                {
                    Description = "New insert test",
                    Name = "New insert",
                    Identification = new Product.Identification()
                    {
                        ExternalID = null,
                        BarCode = Encoding.UTF8.GetBytes("SKU-12345678")
                    }
                },
                Price = 12.50M,
                
            };
            await _connectorProducts.InsertAsync(product);
        }

        [TestMethod]
        public async Task InsertOrUpdateNewRecordAsync()
        {
            var identifier = new Identifier()
            {
                GUID = Guid.NewGuid(),
                Batch = Guid.NewGuid()
            };
            var connector = CreateConnector<Identifier>();
            await connector.InsertOrUpdateAsync(identifier);

            //check if the object exists now
            var filter = connector.CreateQuery();
            filter.Add(x => x.GUID, identifier.GUID);
            var newIdentifier = await connector.GetFirstAsync(filter);

            Assert.IsNotNull(newIdentifier);
            Assert.AreEqual(identifier.Batch, newIdentifier.Batch);
        }

        [TestMethod]
        public async Task InsertOrUpdateExistingRecordAsync()
        {
            var identifier = new Identifier()
            {
                GUID = Guid.NewGuid(),
                Batch = Guid.NewGuid()
            };
            var connector = CreateConnector<Identifier>();
            await connector.InsertAsync(identifier);

            //get the existing object
            var filter = connector.CreateQuery();
            filter.Add(x => x.GUID, identifier.GUID);
            var newIdentifier = await connector.GetFirstAsync(filter);

            //update it
            newIdentifier.Batch = Guid.NewGuid();
            await connector.InsertOrUpdateAsync(newIdentifier);

            //retrieve updated object
            var updatedIdentifier = await connector.GetFirstAsync(filter);


            Assert.IsNotNull(updatedIdentifier);
            Assert.AreEqual(newIdentifier.Batch, updatedIdentifier.Batch);
        }       

        [TestMethod]
        public async Task InsertComposityKeyAsync()
        {
            var compositeKey = new CompositeKey()
            {
                FirstID = Guid.NewGuid().GetHashCode(),
                SecondID = Guid.NewGuid().GetHashCode(),
                SomeValue = Guid.NewGuid().ToString()
            };
            var connector = CreateConnector<CompositeKey>();
            await connector.InsertAsync(compositeKey);
        }

        [TestMethod]
        public async Task UpdateComposityKey()
        {
            var compositeKey = new CompositeKey()
            {
                FirstID = 1,
                SecondID = 1,
                SomeValue = Guid.NewGuid().ToString()
            };
            var connector = CreateConnector<CompositeKey>();
            await connector.UpdateAsync(compositeKey);

            var filter = connector.CreateQuery();
            filter.Add(x => x.FirstID, 1);
            filter.Add(x => x.SecondID, 1);
            var result = await connector.GetFirstAsync(filter);

            Assert.AreEqual(compositeKey.SomeValue, result.SomeValue);
        }

        [TestMethod]
        public async Task CustomTimeOut()
        {
            var query = _connectorOrders.CreateQuery();
            query.SqlQuery = "WAITFOR DELAY '00:00:05';";
            query.CommandTimeOut = 2;

            Exception exception = null;
            try
            {
                await _connectorOrders.ExecuteNonQueryAsync(query);
            }
            catch(Exception ex)
            {
                exception = ex;
            }

            Assert.IsNotNull(exception);
            Assert.IsNotNull(exception.InnerException);
            Assert.IsInstanceOfType(exception.InnerException, typeof(Microsoft.Data.SqlClient.SqlException));
            var number = ((Microsoft.Data.SqlClient.SqlException)exception.InnerException).Errors[0].Number;
            Assert.AreEqual(-2, number);
        }

        private IConnector<T> CreateConnector<T>() where T : new()
        {
            return _serviceProvider.GetRequiredService<IConnector<T>>();
        }
    }
}
