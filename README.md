# ksqlDB client for .NET 
![Build](https://github.com/alex-basiuk/ksqlDB-client-dotnet/workflows/Build/badge.svg)


A lightweight .NET client that enables sending requests easily to a ksqlDB server from within your .NET app, as an alternative to using [REST API](https://docs.ksqldb.io/en/latest/developer-guide/api/).

## Project status: Incubating

The public interface is mostly defined. There might be some refinement but major changes are not expected.

## Planned features and progress
| Feature | Progress |
|---------|----------|
| Listing existing streams, tables, topics, and queries | Done |
| Creation and deletion of streams and table | Done |
| Push and pull queries | Almost done |
| Inserting new rows of data into existing ksqlDB streams | Almost done |
| Terminating push and persistent queries | Pending |
| DI Integration | Pending |
| Various authentication methods | Pending |
| Retry policies | Pending |

## Target frameworks
.NET 5 and .NET Core 3.1 initially. Others might be considered at later stages. 
