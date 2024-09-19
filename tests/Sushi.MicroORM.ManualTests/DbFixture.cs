using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SqlServer.Dac;
using Testcontainers.MsSql;

namespace Sushi.MicroORM.ManualTests;

public class DbFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer;
    public ServiceProvider Services { get; private set; } = null!;

    public DbFixture()
    {
        _msSqlContainer = new MsSqlBuilder().Build();
    }

    public async Task DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        // setup databases
        await _msSqlContainer.StartAsync();
        return;
        var connectionString = _msSqlContainer.GetConnectionString();
        var dacService = new DacServices(connectionString);

        // import bacpacs
        var databaseNames = new List<string> { "TestDatabase", "Customers", "Addresses" };
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        foreach (var databaseName in databaseNames)
        {
            var package = BacPackage.Load($"Databases/{databaseName}.bacpac");
            dacService.ImportBacpac(package, databaseName, cts.Token);
        }

        // create service collection
        var serviceCollection = new ServiceCollection();

        // add microorm
        AddMicroOrm(serviceCollection, connectionString);

        // build service provider
        Services = serviceCollection.BuildServiceProvider();
    }

    private void AddMicroOrm(ServiceCollection serviceCollection, string connectionString)
    {
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
        // primary
        builder.InitialCatalog = "TestDatabase";
        var primaryConnectionString = new SqlServerConnectionString(builder.ToString(), true);

        // customer
        builder.InitialCatalog = "Customers";
        var customersConnectionString = new SqlServerConnectionString(builder.ToString(), true);

        // address
        builder.InitialCatalog = "Addresses";
        var addressesConnectionString = new SqlServerConnectionString(builder.ToString(), true);

        // add micro orm
        serviceCollection.AddMicroORM(
            primaryConnectionString,
            c =>
            {
                c.ConnectionStringProvider.AddMappedConnectionString(
                    typeof(DAL.Customers.Customer),
                    customersConnectionString
                );
                c.ConnectionStringProvider.AddMappedConnectionString(
                    typeof(DAL.Customers.Address),
                    addressesConnectionString
                );
            }
        );
    }

    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DbFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    [Collection("Database collection")]
    public class DatabaseInitialization
    {
        public DatabaseInitialization(DbFixture fixture) { }

        [Fact]
        public void InitializeDatabase()
        {
            // this method exists so that the fixture is initialized
        }
    }
}
