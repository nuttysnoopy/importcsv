echo "Starting API..."
#!/bin/bash
set -e

echo "Waiting for sqlserver:1433..."
for i in {1..60}; do
	if bash -c "echo > /dev/tcp/sqlserver/1433" >/dev/null 2>&1; then
		echo "sqlserver is reachable"
		break
	fi
	echo "waiting for sqlserver... ($i)"
	sleep 1
done

if [ $# -gt 0 ]; then
	echo "Executing: $@"
	exec "$@"
else
	echo "Starting API..."
	exec dotnet CsvImportApp.dll
fi
