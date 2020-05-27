using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sushi.MicroORM.Tests.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sushi.MicroORM.Tests
{
    [TestClass]
    public class ConnectorTest
    {
        [AssemblyInitialize]
        public static void Init(TestContext context)
        {
            string settingsFile = System.IO.Directory.GetCurrentDirectory() + "\\appsettings.json";

            if (System.IO.File.Exists(settingsFile))
            {
                //configure using appsettings.json (local testing)
                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().AddJsonFile(settingsFile);

                var configuration = configurationBuilder.Build();

                string connectionString = configuration.GetConnectionString("TestDatabase");                
                DatabaseConfiguration.SetDefaultConnectionString(connectionString);


                var connectionString2 = configuration.GetConnectionString("Customers");
                DatabaseConfiguration.AddMappedConnectionString("Sushi.MicroORM.Tests.DAL.Customers", connectionString2);

                var connectionString3 = configuration.GetConnectionString("Addresses");
                DatabaseConfiguration.AddMappedConnectionString(typeof(DAL.Customers.Address), connectionString3);
            }
            else
            {
                //configure using environment variables (build pipeline)
                string connectionString = Environment.GetEnvironmentVariable("TestDatabase");
                DatabaseConfiguration.SetDefaultConnectionString(connectionString);

                var connectionString2 = Environment.GetEnvironmentVariable("Customers");
                DatabaseConfiguration.AddMappedConnectionString("Sushi.MicroORM.Tests.DAL.Customers", connectionString2);

                var connectionString3 = Environment.GetEnvironmentVariable("Addresses");
                DatabaseConfiguration.AddMappedConnectionString(typeof(DAL.Customers.Address), connectionString3);
            }
        }

        [TestMethod]
        public void FetchSingleByID()
        {
            var ConnectorOrders = new Connector<Order>();
            int id = 1;

            var order = ConnectorOrders.FetchSingle(id);

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(order, Newtonsoft.Json.Formatting.Indented));

            Assert.IsTrue(order?.ID == id);
        }

        [TestMethod]
        public void FetchSingleNotExistingByID()
        {
            var ConnectorOrders = new Connector<Order>()
            {
                 FetchSingleMode = FetchSingleMode.ReturnDefaultWhenNotFound
            };
            int id = -1;

            var order = ConnectorOrders.FetchSingle(id);

            Assert.IsNull(order);
        }

        [TestMethod]
        public void FetchSingleNotExistingByIDNewInstance()
        {
            var ConnectorOrders = new Connector<Order>()
            {
                FetchSingleMode = FetchSingleMode.ReturnNewObjectWhenNotFound
            };
            int id = -1;

            var order = ConnectorOrders.FetchSingle(id);

            Assert.IsNotNull(order);
            Assert.AreEqual(0, order.ID);
        }

        [TestMethod]
        public void FetchSingleByIDWithCustomTimeout()
        {
            int id = 1;

            var ConnectorOrders = new Connector<Order>();
            var order = ConnectorOrders.FetchSingle(id);

            Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");

            Assert.IsTrue(order?.ID == id);
        }

        [TestMethod]
        public void FetchSingleByFilter()
        {
            int id = 2;

            var ConnectorOrders = new Connector<Order>();
            var filter = ConnectorOrders.CreateDataFilter();
            filter.Add(x => x.ID, id);

            var order = ConnectorOrders.FetchSingle(filter);

            Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");

            Assert.IsTrue(order?.ID == id);
        }

        [TestMethod]
        public void FetchSingleBySql()
        {
            int id = 3;            

            string sql = $"SELECT Order_Key, Order_Created FROM cat_Orders WHERE Order_Key = {id}"; //this is BAD PRACTICE! always use parameters

            var ConnectorOrders = new Connector<Order>();
            var order = ConnectorOrders.FetchSingle(sql);

            Console.WriteLine($"{order.ID} - {order.Created} - {order.Created2} - {order.CustomerID}");

            Assert.IsTrue(order?.ID == id);
        }

        [TestMethod]
        public void FetchSingleBySqlAndFilter()
        {
            int id = 3;

            

            string sql = $"SELECT * FROM cat_Orders WHERE Order_Key = @orderID"; //this is BAD PRACTICE! always use parameters

            var ConnectorOrders = new Connector<Order>();
            var filter = ConnectorOrders.CreateDataFilter();
            filter.AddParameter("@orderID", id);

            var order = ConnectorOrders.FetchSingle(sql, filter);

            Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");

            Assert.IsTrue(order?.ID == id);
        }

        [TestMethod]
        public void FetchSingleMultilevel()
        {
            int productID = 1;
            var product = Product.FetchSingle(productID);
            
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(product, Newtonsoft.Json.Formatting.Indented));

            Assert.IsTrue(product.MetaData.Identification.GUID != Guid.Empty);
        }

        [TestMethod]
        public void FetchAll()
        {
            var ConnectorOrders = new Connector<Order>();

            var request = new DataFilter<Order>();
            var orders = ConnectorOrders.FetchAll(request);
            foreach(var order in orders)
            {
                Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");
            }

            Assert.IsTrue(orders.Count > 0);            
        }

        [TestMethod]
        public void FetchAllMaxResults()
        {
            int maxResults = 2;

            var connector = new Connector<Order>();            
            var filter = connector.CreateDataFilter();
            filter.MaxResults = maxResults;
            var orders = connector.FetchAll(filter);

            Assert.AreEqual(maxResults, orders.Count);
        }

        [TestMethod]
        public void FetchAllInvalidMap()
        {
            var connector = new Connector<Order>(new Order.InvalidOrderMap());
            var request = connector.CreateDataFilter();
            try
            {
                var orders = connector.FetchAll(request);
                Assert.Fail();
            }
            catch(Exception ex)
            {
                if (!ex.Message.Contains("no table"))
                    Assert.Fail();
            }
        }

        [TestMethod]
        public void FetchAllBySql()
        {
            var ConnectorOrders = new Connector<Order>();
            string sql = "SELECT TOP(3) * FROM cat_Orders";
            var orders = ConnectorOrders.FetchAll(sql);
            foreach (var order in orders)
            {
                Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");
            }

            Assert.IsTrue(orders.Count > 0);
        }

        [TestMethod]
        public void FetchAllWithJoin()
        {
            int customerID = 1;

            var bookings = BookedRoom.FetchAll(customerID);

            Console.Write(Newtonsoft.Json.JsonConvert.SerializeObject(bookings, Newtonsoft.Json.Formatting.Indented));
            Assert.IsTrue(bookings.Count > 0);
        }

        [TestMethod]
        public void FetchAllOrderedBy()
        {
            var ConnectorOrders = new Connector<Order>();

            var filter = new DataFilter<Order>();
            filter.AddOrder(x => x.ID, SortOrder.DESC);
            filter.MaxResults = 2;
            var orders = ConnectorOrders.FetchAll(filter);
            
            foreach (var order in orders)
            {
                Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");
            }

            Assert.AreEqual(2, orders.Count);
            Assert.IsTrue(orders[0].ID > orders[1].ID);
        }

        [TestMethod]
        public void FetchPaging()
        {
            var ConnectorOrders = new Connector<Order>();

            var request = new DataFilter<Order>();
            request.Paging = new PagingData()
            {
                NumberOfRows = 5,
                PageIndex = 2
            };
            var orders = ConnectorOrders.FetchAll(request);
            foreach (var order in orders)
            {
                Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");
            }
            Console.WriteLine("Total number of rows: " + request.Paging.TotalNumberOfRows);

            Assert.IsTrue(orders.Count == request.Paging.NumberOfRows);
            Assert.IsTrue(request.Paging.TotalNumberOfRows.HasValue);
        }

        [TestMethod]
        public void FetchWhereInInt()
        {
            var ConnectorOrders = new Connector<Order>();
            var request = new DataFilter<Order>();
            //request.WhereClause.Add(new DatabaseDataValueColumn("Order_Key", System.Data.SqlDbType.Int, new int[] { 1, 2,3 }, ComparisonOperator.In));
            request.Add(x => x.ID, new int[] { 1, 2, 3 }, ComparisonOperator.In);
            var orders = ConnectorOrders.FetchAll(request);
            foreach (var order in orders)
            {
                Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");
            }

            Assert.AreEqual(3, orders.Count);
        }

        [TestMethod]
        public void FetchWhereInEmptyEnumerable()
        {
            var ConnectorOrders = new Connector<Order>();
            var request = new DataFilter<Order>();
            //request.WhereClause.Add(new DatabaseDataValueColumn("Order_Key", System.Data.SqlDbType.Int, new int[] { 1, 2,3 }, ComparisonOperator.In));
            request.Add(x => x.ID, new int[] { }, ComparisonOperator.In);
            var orders = ConnectorOrders.FetchAll(request);
            foreach (var order in orders)
            {
                Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");
            }

            Assert.AreEqual(0, orders.Count);
        }

        [TestMethod]
        public void FetchWhereInString()
        {
            var ConnectorProducts = new Connector<Product>();

            var request = new DataFilter<Product>();
            var names = new string[]
            {
                "TV",
                "Laser Mouse",
                "E-reader"
            };

            request.Add(x => x.MetaData.Name, names, ComparisonOperator.In);
            var products = ConnectorProducts.FetchAll(request);
            foreach (var product in products)
            {
                Console.WriteLine($"{product.ID} - {product.MetaData.Name}");
            }

            Assert.IsTrue(products.Count == 3);
        }

        [TestMethod]
        public void FetchWhereGreaterThanString()
        {
            var ConnectorProducts = new Connector<Product>();

            var filter = ConnectorProducts.CreateDataFilter();


            filter.Add(x => x.MetaData.Description, "", ComparisonOperator.GreaterThan);
            var products = ConnectorProducts.FetchAll(filter);
            foreach (var product in products)
            {
                Console.WriteLine($"{product.ID} - {product.MetaData.Name} - {product.MetaData.ProductTypeID}");
            }

            Assert.IsTrue(products.Count > 0);
        }

        [TestMethod]
        public void FetchWhereCustomSql()
        {
            var connector = new Connector<Product>();

            var filter = connector.CreateDataFilter();
            filter.AddSql("LEN(Product_Name) > @length");            
            filter.AddParameter("@length", 12);
            filter.Add(x => x.Price, 1, ComparisonOperator.GreaterThanOrEquals);
            var products = connector.FetchAll(filter);
            foreach (var product in products)
            {
                Console.WriteLine($"{product.ID} - {product.MetaData.Name} - {product.Price}");
            }
            Assert.IsTrue(products.Count > 0);
        }

        [TestMethod]
        public void FetchWhereCustomSqlNullableParameter()
        {
            var connector = new Connector<Product>();

            var filter = connector.CreateDataFilter();
            filter.AddSql("Product_ProductTypeID = @productTypeID");
            int? productTypeID = null;
            filter.AddParameter("@productTypeID", productTypeID);
            
            var products = connector.FetchAll(filter);
            foreach (var product in products)
            {
                Console.WriteLine($"{product.ID} - {product.MetaData.Name} - {product.Price}");
            }
            Assert.IsTrue(products.Count == 0);
        }

        [TestMethod]
        public void FetchNotExisting()
        {
            var ConnectorOrders = new Connector<Order>();
            var order = ConnectorOrders.FetchSingle(-1);
            Assert.IsTrue(order == null);
        }

        [TestMethod]
        public void ExecuteNonQuery()
        {
            var ConnectorProducts = new Connector<Product>();

            string name = DateTime.UtcNow.Ticks.ToString();
            int productID = 1;
            var query = @"
UPDATE cat_Products
SET Product_Name = @name
WHERE Product_Key = @productID";
            var filter = new DataFilter<Product>();
            filter.AddParameter(@"name", System.Data.SqlDbType.VarChar, name);
            filter.AddParameter("@productID", System.Data.SqlDbType.Int, productID);
            ConnectorProducts.CommandTimeout = 1;
            ConnectorProducts.ExecuteNonQuery(query, filter);

            //check if name was updated
            var product = ConnectorProducts.FetchSingle(productID);
            Assert.AreEqual(name, product.MetaData.Name);
        }

        [TestMethod]
        public void ExecuteScalar()
        {
            var ConnectorProducts = new Connector<Product>();
            string name = DateTime.UtcNow.Ticks.ToString();
            int productID = 1;
            var query = @"
SELECT COUNT(*)
FROM cat_Products
WHERE Product_Key = @productID";
            var filter = new DataFilter<Product>();
            filter.AddParameter("@productID", System.Data.SqlDbType.Int, productID);
            var count = ConnectorProducts.ExecuteScalar<int>(query, filter);
            Console.WriteLine(count);
            Assert.AreEqual(1, count);            
        }

        [TestMethod]
        public void ExecuteSet()
        {
            var ConnectorProducts = new Connector<Product>();
            string name = DateTime.UtcNow.Ticks.ToString();            
            var query = @"
SELECT DISTINCT(Product_ProductTypeID)
FROM cat_Products";
                        
            var productTypes = ConnectorProducts.ExecuteSet<Product.ProducType?>(query);

            foreach (var productType in productTypes)
            {
                Console.WriteLine(productType == null ? "NULL" : productType.ToString());
            }

            Assert.AreEqual(5, productTypes.Count);
        }

        [TestMethod]
        public void ExecuteSetWithFilter()
        {
            var ConnectorProducts = new Connector<Product>();
            string name = DateTime.UtcNow.Ticks.ToString();
            int productID = 1;
            var query = @"
SELECT DISTINCT(Product_ProductTypeID)
FROM cat_Products
WHERE Product_Key > @productID";
            var filter = new DataFilter<Product>();
            filter.AddParameter("@productID", System.Data.SqlDbType.Int, productID);
            var productTypes = ConnectorProducts.ExecuteSet<Product.ProducType?>(query, filter);
            
            foreach(var productType in productTypes)
            {
                Console.WriteLine(productType == null ? "NULL" : productType.ToString());
            }
            Assert.AreEqual(4, productTypes.Count);
        }

        [TestMethod]
        public void SaveNew()
        {
            var ConnectorOrders = new Connector<Order>();
            var order = new Order()
            {
                CustomerID = 1,
                Created = DateTime.UtcNow,
                DeliveryTime = new TimeSpan(3, 20, 35),
                DeliveryTime2 = DateTime.UtcNow.TimeOfDay,
                Amount = 15.97M,
                Measurement = 100.46
            };
            ConnectorOrders.Save(order);

            Assert.IsTrue(order.ID > 0);
        }

        [TestMethod]
        public void SaveExisting()
        {
            var ConnectorOrders = new Connector<Order>();

            //save the order
            var order = ConnectorOrders.FetchSingle(20);
            
            
            string newComments = DateTime.UtcNow.ToString();

            order.Comments = newComments;
            order.DeliveryTime = null;
            order.DeliveryTime2 = DateTime.UtcNow.TimeOfDay;
            ConnectorOrders.Save(order);

            //retrieve order again from database
            order = ConnectorOrders.FetchSingle(20);


            Assert.AreEqual(newComments, order.Comments);
        }

        [TestMethod]
        public void Insert()
        {
            var connector = new Connector<Product>();
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
            connector.Insert(product);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(product, Newtonsoft.Json.Formatting.Indented));
            Assert.AreNotEqual(0, product.ID);
        }

        [TestMethod]
        public void InsertOrUpdateNewRecord()
        {
            var identifier = new Identifier()
            {
                GUID = Guid.NewGuid(),
                Batch = Guid.NewGuid()
            };
            var connector = new Connector<Identifier>();
            connector.InsertOrUpdate(identifier);

            //check if the object exists now
            var filter = connector.CreateDataFilter();            
            filter.Add(x => x.GUID, identifier.GUID);
            var newIdentifier = connector.FetchSingle(filter);

            Assert.IsNotNull(newIdentifier);
            Assert.AreEqual(identifier.Batch, newIdentifier.Batch);
        }

        [TestMethod]
        public void InsertOrUpdateExistingRecord()
        {
            var identifier = new Identifier()
            {
                GUID = Guid.NewGuid(),
                Batch = Guid.NewGuid()
            };
            var connector = new Connector<Identifier>();
            connector.Insert(identifier);

            //get the existing object
            var filter = connector.CreateDataFilter();
            filter.Add(x => x.GUID, identifier.GUID);
            var newIdentifier = connector.FetchSingle(filter);

            //update it
            newIdentifier.Batch = Guid.NewGuid();
            connector.InsertOrUpdate(newIdentifier);

            //retrieve updated object
            var updatedIdentifier = connector.FetchSingle(filter);


            Assert.IsNotNull(updatedIdentifier);
            Assert.AreEqual(newIdentifier.Batch, updatedIdentifier.Batch);
        }

        [TestMethod]
        public void Insert_GuidEmpty()
        {
            var ConnectorProducts = new Connector<Product>();
            var product = new Product()
            {
                MetaData = new Product.ProductMetaData()
                {
                    Description = "New insert test",
                    Name = "New insert",
                    Identification = new Product.Identification()
                    {
                        ExternalID = null,
                        BarCode = Encoding.UTF8.GetBytes("SKU-12345678"),
                        GUID = Guid.Empty
                    }
                },
                Price = 12.50M,
                
            };
            ConnectorProducts.Insert(product);
        }

        [TestMethod]
        public void InsertAssignedKey()
        {
            var assigned = new Identifier()
            {
                GUID = Guid.NewGuid(),
                Batch = Guid.NewGuid()
            };
            var connector = new Connector<Identifier>();
            connector.Insert(assigned);
        }

        [TestMethod]
        public void InsertComposityKey()
        {
            var compositeKey = new CompositeKey()
            {
                FirstID = Guid.NewGuid().GetHashCode(),
                SecondID = Guid.NewGuid().GetHashCode(),
                SomeValue = Guid.NewGuid().ToString()
            };
            var connector = new Connector<CompositeKey>();
            connector.Insert(compositeKey);            
        }

        [TestMethod]
        public void UpdateComposityKey()
        {
            var compositeKey = new CompositeKey()
            {
                FirstID = 1,
                SecondID = 1,
                SomeValue = Guid.NewGuid().ToString()
            };
            var connector = new Connector<CompositeKey>();
            connector.Update(compositeKey);

            var filter = connector.CreateDataFilter();
            filter.Add(x => x.FirstID, 1);
            filter.Add(x => x.SecondID, 1);
            var result = connector.FetchSingle(filter);

            Assert.AreEqual(compositeKey.SomeValue, result.SomeValue);
        }

        [TestMethod]
        public void Delete()
        {
            var connector = new Connector<Product>();
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
            //insert
            connector.Insert(product);

            //delete
            connector.Delete(product);

            //check if really deleted
            var deletedProduct = connector.FetchSingle(product.ID);
            Assert.IsNull(deletedProduct);
        }

        [TestMethod]
        public void BulkInsertAutoIncrement()
        {
            var ConnectorOrders = new Connector<Order>();

            var orders = new List<Order>();

            int numberOfRows = 100;

            //assign unique ID to orders so we can check if all items were inserterd
            var uniqueID = Guid.NewGuid().ToString();
            for (int i = 0; i<numberOfRows;i++)
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

            ConnectorOrders.BulkInsert(orders);

            //retrieve orders
            var filter = ConnectorOrders.CreateDataFilter();
            filter.Add(x => x.Comments, uniqueID);
            var retrievedOrders = ConnectorOrders.FetchAll(filter);
            Assert.AreEqual(numberOfRows, retrievedOrders.Count);
        }

        [TestMethod]
        public void BulkInsertIdentityInsert()
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

            var connector = new Connector<Identifier>();
            connector.BulkInsert(identifiers, true);

            //retrieve 
            var filter = connector.CreateDataFilter();
            filter.Add(x => x.Batch, uniqueID);
            var retrievedRows = connector.FetchAll(filter);
            Assert.AreEqual(numberOfRows, retrievedRows.Count);
        }

        [TestMethod]
        public void BulkInsertCompositeKey()
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

            var connector = new Connector<CompositeKey>();
            connector.BulkInsert(rows);            
        }

        [TestMethod]
        public void FetchWithTableValuedParameter()
        {
            var sproc = "EXEC sp_GetOrders @customerIDs";

            var customerTable = new System.Data.DataTable();
            customerTable.Columns.Add();
            customerTable.Rows.Add(98);
            customerTable.Rows.Add(99);

            var connector = new Connector<Order>();
            var filter = connector.CreateDataFilter();
            filter.AddParameter("@customerIDs", customerTable, "cat_CustomerTableType");
            var orders = connector.FetchAll(sproc, filter);

            var count98 = orders.Count(x => x.CustomerID == 98);
            var count99 = orders.Count(x => x.CustomerID == 99);

            Assert.AreEqual(orders.Count, count98 + count99);
        }

        [TestMethod]
        public void FetchFromTableValuedFunction()
        {
            var connector = new Connector<AvailableRoom>();
            var filter = connector.CreateDataFilter();
            filter.AddParameter("@startDate", new DateTime(2019, 1, 1));
            filter.AddParameter("@endDate", new DateTime(2019, 1, 12));
            var rooms = connector.FetchAll(filter);

            foreach (var room in rooms)
            {
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(room));
            }

            Assert.IsTrue(rooms.Count > 0);
        }

        [TestMethod]
        public void FetchFromTableValuedFunctionWithCondition()
        {
            var connector = new Connector<AvailableRoom>();
            var filter = connector.CreateDataFilter();
            filter.AddParameter("@startDate", new DateTime(2019, 1, 1));
            filter.AddParameter("@endDate", new DateTime(2019, 1, 12));
            filter.Add(x => x.Type, 2);
            var rooms = connector.FetchAll(filter);

            foreach (var room in rooms)
            {
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(room));
            }

            Assert.IsTrue(rooms.Count > 0);
        }

        [TestMethod]
        public void Log()
        {
            var ConnectorOrders = new Connector<Order>();
            int id = 1;

            string logMessage = null;

            try
            {
                DatabaseConfiguration.Log = (string s) => { logMessage = s; };

                var order = ConnectorOrders.FetchSingle(id);



                Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");

                Console.WriteLine(logMessage);
            }
            finally
            {
                DatabaseConfiguration.Log = null;
            }
            Assert.IsTrue(!string.IsNullOrWhiteSpace(logMessage));
        }
    }
}
