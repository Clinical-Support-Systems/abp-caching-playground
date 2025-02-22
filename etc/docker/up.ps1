docker network create abpcachingplayground --label=abpcachingplayground
docker-compose -f docker-compose.infrastructure.yml up -d
exit $LASTEXITCODE