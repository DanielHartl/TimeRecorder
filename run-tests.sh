cd server
dotnet test --logger trx
RESUT=$?

docker build -t server .
docker rm -f server || true
docker run --name server -e ASPNETCORE_ENVIRONMENT=development -d -p 80:80 --rm -ti server
cd ../integration-tests
dotnet test --logger trx
RESIT=$?

docker rm -f server || true

if [ $RESUT -ne 0 ]; then
  echo Unit tests failed
  exit $RESUT 
fi

if [ $RESIT -ne 0 ]; then
  echo Integration tests failed
  exit $RESIT
fi

exit 0
