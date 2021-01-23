$ksq_endpoint="http://localhost:8088/ksql"
$create_stream_sql="CREATE STREAM orders_stream (ordertime BIGINT, orderid INT, itemid VARCHAR, orderunits DOUBLE, address STRUCT<city VARCHAR, state VARCHAR, zipcode BIGINT>) WITH (KAFKA_TOPIC='orders_topic', VALUE_FORMAT='JSON');"
$create_table_sql="CREATE TABLE users_table (registertime BIGINT, gender VARCHAR, regionid VARCHAR, userid VARCHAR PRIMARY KEY, interests ARRAY<STRING>, contactInfo MAP<STRING, STRING>) WITH (kafka_topic='users_topic', value_format='JSON');"

$create_stream_body = @{
 "ksql"=$create_stream_sql
} | ConvertTo-Json

$create_table_body = @{
 "ksql"=$create_table_sql
} | ConvertTo-Json

$header = @{
 "Accept"="application/vnd.ksql.v1+json"
 "Content-Type"="application/json"
}

do
{  
  $response = Invoke-WebRequest -SkipHttpErrorCheck -Uri $ksq_endpoint -Method POST -Headers $header -Body $create_stream_body
  if ($response.statusCode -ne 200)
  {
    Write-Host "ksqlDb is not ready yet: $response"
    Start-Sleep -s 5
  }  
} while($response.statusCode -ne 200) # We need to wait until the ksqldb server is up and running and the respective topics are created

Invoke-WebRequest -Uri $ksq_endpoint -Method POST -Headers $header -Body $create_table_body