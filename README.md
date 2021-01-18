# ksqlDB client for .NET
=====================================================

![Build](https://github.com/alex-basiuk/ksqlDB-client-dotnet/workflows/Build/badge.svg)


In development scenarios when server and client have a priori knowledge that both will speak HTTP/2 unencrypted, you may establish an HTTP/2 connection over cleartext by setting an AppContext switch or an environment variable (DOTNET_SYSTEM_NET_HTTP_SOCKETSHTTPHANDLER_HTTP2UNENCRYPTEDSUPPORT=1).
`AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);`
