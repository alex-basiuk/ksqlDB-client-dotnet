using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KsqlDb.Client.IntegrationTests
{
    public class ListTopicsTests
    {
        [Fact]
        public async Task Returns_Order_Topic_In_The_List_Of_Topics()
        {
            // Act
            var topics = await TestClient.Instance.ListTopics();

            // Assert
            Assert.Contains(topics, t => t.Topic == TestClient.KnownEntities.OrdersTopicName
                                         && t.NumberOfPartitions == 1
                                         && t.ReplicasPerPartition.SequenceEqual(new[] {1}));
        }

        [Fact]
        public async Task Throws_TaskCanceledException_When_Cancelled()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act / Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => TestClient.Instance.ListTopics(cts.Token));
        }
    }
}
