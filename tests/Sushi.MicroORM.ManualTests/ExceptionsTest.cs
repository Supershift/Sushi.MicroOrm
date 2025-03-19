using Microsoft.Extensions.DependencyInjection;
using Sushi.MicroORM.Exceptions;
using Sushi.MicroORM.ManualTests.DAL;

namespace Sushi.MicroORM.ManualTests
{
    [Collection("Database collection")]
    public class ExceptionsTest
    {
        private readonly ServiceProvider _serviceProvider;

        public ExceptionsTest(DbFixture fixture)
        {
            // build provider
            _serviceProvider = fixture.Services;
        }

        [Fact]
        public async Task UniqueIndexViolation()
        {
            // arrange
            var connector = _serviceProvider.GetRequiredService<IConnector<UniqueValue>>();
            var uniqueValue = new UniqueValue(Guid.NewGuid(), 1234);

            // act
            var act = async () => await connector.InsertAsync(uniqueValue);

            // assert
            await Assert.ThrowsAsync<UniqueIndexViolationException>(act);
        }

        [Fact]
        public async Task PrimaryKeyConstraintViolation()
        {
            // arrange
            var connector = _serviceProvider.GetRequiredService<IConnector<UniqueValue>>();
            var uniqueValue = new UniqueValue(new Guid("813d3cfd-b582-4ee1-9d2d-bc7e58e456b4"), 574574);

            // act
            var act = async () => await connector.InsertAsync(uniqueValue);

            // assert            
            await Assert.ThrowsAsync<UniqueConstraintViolationException>(act);
        }

        [Fact]
        public async Task ConstraintViolation()
        {
            // arrange
            var connector = _serviceProvider.GetRequiredService<IConnector<Parent>>();
            var query = connector.CreateQuery();
            query.Add(x => x.Id, 1);

            // act
            var act = async () => await connector.DeleteAsync(query);

            // assert            
            await Assert.ThrowsAsync<ConstraintViolationException>(act);
        }

        [Fact]
        public async Task BulkInsert_UniqueIndexViolation()
        {
            // arrange
            var connector = _serviceProvider.GetRequiredService<IConnector<UniqueValue>>();
            var uniqueValue = new UniqueValue(Guid.NewGuid(), 1234);

            // act
            var act = async () => await connector.BulkInsertAsync(new[] { uniqueValue });

            // assert
            await Assert.ThrowsAsync<UniqueIndexViolationException>(act);
        }
    }
}
