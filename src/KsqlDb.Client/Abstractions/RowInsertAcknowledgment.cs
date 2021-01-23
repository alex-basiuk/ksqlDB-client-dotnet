using KsqlDb.Api.Client.Abstractions;

namespace KSQL.API.Client
{
    /// <summary>
    /// An acknowledgment from the ksqlDB server that a row has been successfully inserted into a ksqlDB stream.
    /// </summary>
    public class RowInsertAcknowledgment
    {
        /// <summary>
        /// The corresponding sequence number for the acknowledgment.
        /// Sequence numbers start at zero for each new <see cref="IClient.streamInserts"/> request.
        /// </summary>
        public long SequenceNumber { get; }

        public RowInsertAcknowledgment(long sequenceNumber) => SequenceNumber = sequenceNumber;
    }
}
