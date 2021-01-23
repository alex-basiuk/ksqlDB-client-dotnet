using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KsqlDb.Client.IntegrationTests
{
    public class ListStreamsTests
    {
        [Fact]
        public async Task Returns_ORDERS_TOPIC_Stream_In_The_List_Of_Streams()
        {
            // Act
            var streams = await TestClient.Instance.ListStreams();

            // Assert
            Assert.Contains(streams, s => s.Name == TestClient.KnownEntities.OrdersStreamName
                                          && s.Topic == TestClient.KnownEntities.OrdersTopicName
                                          && s.KeyFormat == "KAFKA"
                                          && s.ValueFormat == "JSON"
                                          && s.IsWindowed is null);
        }

        [Fact]
        public async Task Throws_TaskCanceledException_When_Cancelled()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act / Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => TestClient.Instance.ListStreams(cts.Token));
        }
    }
}
