docker build -f Update-npm.Dockerfile -t test-svc ..
docker run --name test-svc-app test-svc
docker cp test-svc-app:/source/ClientApp/package-lock.json ../src/Server.App/ClientApp/package-lock.json