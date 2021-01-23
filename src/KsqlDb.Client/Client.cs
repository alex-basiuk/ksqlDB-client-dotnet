using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using KSQL.API.Client;
using KsqlDb.Api.Client.Abstractions;
using KsqlDb.Api.Client.Abstractions.Objects;
using KsqlDb.Api.Client.Abstractions.QueryResults;
using KsqlDb.Api.Client.Exceptions;
using KsqlDb.Api.Client.KsqlApiV1;
using KsqlDb.Api.Client.KsqlApiV1.Responses;
using KsqlDb.Api.Client.Parsers;

namespace KsqlDb.Api.Client
{
    internal class Client : IClient
    {
        private static readonly Regex _batchRequestValidationRegex = new Regex(@"EMIT\s+CHANGES\s+^(LIMIT\s+\d+)", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Dictionary<string, object> _emptyProperties = new Dictionary<string, object>();
        private readonly KSqlDbHttpClient _httpClient;

        public Client(KSqlDbHttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public Task<StreamedQueryResult> StreamQuery(string sql, CancellationToken cancellationToken = default) => StreamQuery(sql, _emptyProperties, cancellationToken);

        public async Task<StreamedQueryResult> StreamQuery(string sql, IDictionary<string, object> properties, CancellationToken cancellationToken = default)
        {
            ThrowIfInvalidStatement(sql);
            const int bufferCapacity = 200; //TODO Make it configurable
            var channel = Channel.CreateBounded<JsonElement>(bufferCapacity);
            _ = _httpClient.PostQueryStream(channel.Writer, sql, properties, cancellationToken);

            if (!await channel.Reader.WaitToReadAsync(cancellationToken))
            {
                // We should end up here only in case of http error.
                // Observer the task and throw the underlying exception.
                await channel.Reader.Completion;
            }

            var header = await channel.Reader.ReadAsync(cancellationToken).ConfigureAwait(false); // Can throw ChannelClosedException
            string? queryId = header.TryGetProperty("queryId", out var queryIdElement) ? queryIdElement.GetString() : default;
            var parser = new QueryResultRowParser(header);
            var rows = channel.Reader.ReadAllAsync(cancellationToken).Select(parser.Parse);
            return new StreamedQueryResult(parser.ColumnNamesAndTypes, rows, queryId);
        }

        public Task<BatchedQueryResults> BatchQuery(string sql, CancellationToken cancellationToken = default) => BatchQuery(sql, _emptyProperties, cancellationToken);

        public async Task<BatchedQueryResults> BatchQuery(string sql, IDictionary<string, object> properties, CancellationToken cancellationToken = default)
        {
            if (!_batchRequestValidationRegex.Match(sql).Success) throw new ArgumentException("The SQL statement must be either a pull query (without EMIT CHANGES clause) or a limiting push query (with EMIT CHANGES LIMIT {rows_number}).", nameof(sql));
            var streamedResult = await StreamQuery(sql, properties, cancellationToken);
            var rows = await streamedResult.Rows.ToArrayAsync(cancellationToken).ConfigureAwait(false);
            return new BatchedQueryResults(rows, streamedResult.QueryId);
        }

        public Task TerminatePushQuery(string queryId, CancellationToken cancellationToken = default) => _httpClient.PostKqsl($"TERMINATE {queryId}", cancellationToken: cancellationToken);

        public async Task<RowInsertAcknowledgment> InsertRow(string streamName, KSqlObject row, CancellationToken cancellationToken = default)
        {
            var channel = Channel.CreateBounded<KSqlObject>(1);
            channel.Writer.TryWrite(row);
            channel.Writer.Complete();
            var acks = StreamInserts(streamName, channel.Reader.ReadAllAsync(cancellationToken), cancellationToken).ConfigureAwait(true);
            await foreach (var ack in acks)
            {
                return ack;
            }

            throw new KsqlDbException("An acknowledgement hasn't been received");
        }

        public async IAsyncEnumerable<RowInsertAcknowledgment> StreamInserts(string streamName, IAsyncEnumerable<KSqlObject> rows, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            const int bufferCapacity = 200; //TODO Make it configurable
            var channel = Channel.CreateBounded<InsertStreamAckResponse>(bufferCapacity);
            _ = _httpClient.PostInsertStream(streamName, rows, channel.Writer, cancellationToken);

            while (await channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                while (channel.Reader.TryRead(out var item))
                {
                    if (string.Equals(item.Status, "ok", StringComparison.OrdinalIgnoreCase))
                    {
                        yield return new RowInsertAcknowledgment(item.Seq);
                    }
                    else if (string.Equals(item.Status, "error", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new KsqlDbException("Received an error while trying to insert into stream.")
                        {
                            Body = new ErrorDetails { ErrorCode = item.ErrorCode ?? default, Message = item.Message }
                        };
                    }
                    else
                    {
                        throw new KsqlDbException($"Received an unrecognized status while trying to insert into stream: {item.Status}.");
                    }
                }
            }

            await channel.Reader.Completion;
        }

        public Task<ExecuteStatementResult> ExecuteStatement(string sql, CancellationToken cancellationToken = default) => ExecuteStatement(sql, _emptyProperties, cancellationToken);

        public async Task<ExecuteStatementResult> ExecuteStatement(string sql, IDictionary<string, object> properties, CancellationToken cancellationToken = default)
        {
            var responses = await _httpClient.PostKqsl(sql, properties, cancellationToken: cancellationToken).ConfigureAwait(false);
            var response = EnsureSingleEntityResponse(responses);
            return new ExecuteStatementResult(response.CommandId);
        }

        public async Task<IReadOnlyCollection<StreamOrTableInfo>> ListStreams(CancellationToken cancellationToken = default)
        {
            static StreamOrTableInfo Map(KsqlResponse.StreamOrTableInfo stream) => new StreamOrTableInfo(stream.Name, stream.Topic, "KAFKA", stream.Format);
            return await ListEntities("LIST STREAMS;", r => r.Streams, Map, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<StreamOrTableInfo>> ListTables(CancellationToken cancellationToken = default)
        {
            //TODO REST Response has only 1 format field.
            static StreamOrTableInfo Map(KsqlResponse.StreamOrTableInfo table) => new StreamOrTableInfo(table.Name, table.Topic, "KAFKA", table.Format, table.IsWindowed);
            return await ListEntities("LIST TABLES;", r => r.Tables, Map, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<TopicInfo>> ListTopics(CancellationToken cancellationToken = default)
        {
            static TopicInfo Map(KsqlResponse.TopicInfo topic) => new TopicInfo(topic.Name, topic.ReplicaInfo);
            return await ListEntities("LIST TOPICS;", r => r.Topics, Map, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<QueryInfo>> ListQueries(CancellationToken cancellationToken = default)
        {
            // TODO REST response doesn't match Java client
            static QueryInfo Map(KsqlResponse.QueryInfo query) => new QueryInfo(query.Sinks == null ? QueryType.Push : QueryType.Persistent, query.Id, query.QueryString, null);
            return await ListEntities("LIST QUERIES;", r => r.Queries, Map, cancellationToken).ConfigureAwait(false);
        }

        public Task<SourceDescription> DescribeSource(string sourceName, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();

        private async Task<IReadOnlyCollection<TOutput>> ListEntities<TInput, TOutput>(string sql, Func<KsqlResponse, TInput[]?> getEntityArray, Func<TInput, TOutput> mapInputToOutput, CancellationToken cancellationToken = default)
        {
            var responses = await _httpClient.PostKqsl(sql, cancellationToken: cancellationToken).ConfigureAwait(false);
            var response = EnsureSingleEntityResponse(responses);
            var entitiesArray = getEntityArray(response);
            if (entitiesArray == null || entitiesArray.Length == 0) return Array.Empty<TOutput>();

            var result = new TOutput[entitiesArray.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = mapInputToOutput(entitiesArray[i]);
            }
            return result;
        }

        private static KsqlResponse EnsureSingleEntityResponse(KsqlResponse[]? responses) =>
            responses?.SingleOrDefault() ?? throw new KsqlDbException($"Unexpected number of entities in server response: {responses?.Length ?? 0}. Expected 1.");

        private static void ThrowIfInvalidStatement(string sql)
        {
            if (sql is null) throw new ArgumentNullException(nameof(sql));
            var trimmedSql = sql.AsSpan().Trim();
            if (trimmedSql[^1] != ';') throw new ArgumentException($"Missing semicolon in the SQL statement: {sql}", nameof(sql));
            if (trimmedSql.IndexOf(';') != trimmedSql.Length - 1) throw new ArgumentException($"Only one KSQL statement can be executed at a time. The supplied statement: {sql}");
        }
    }
}
