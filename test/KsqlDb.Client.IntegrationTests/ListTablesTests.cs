using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KsqlDb.Client.IntegrationTests
{
    public class ListTablesTests
    {
        [Fact]
        public async Task Returns_Users_Table_In_The_List_Of_Table()
        {
            // Act
            var tables = await TestClient.Instance.ListTables();

            // Assert
            Assert.Contains(tables, s => s.Name == TestClient.KnownEntities.UsersTableName
                                         && s.Topic == TestClient.KnownEntities.UsersTopicName
                                         && s.KeyFormat == "KAFKA"
                                         && s.ValueFormat == "JSON"
                                         && s.IsWindowed is false);
        }

        [Fact]
        public async Task Throws_TaskCanceledException_When_Cancelled()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act / Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => TestClient.Instance.ListTables(cts.Token));
        }
    }
}
