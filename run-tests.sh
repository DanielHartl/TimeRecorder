SCRIPTPATH="$( cd "$(dirname "$0")" ; pwd -P )"

dotnet test $SCRIPTPATH/TimeRecorder.sln --logger trx
RESULT_UT=$?

docker build -t server $SCRIPTPATH/server
docker rm -f server || true
docker run --name server -e ASPNETCORE_ENVIRONMENT=development -d -p 80:80 --rm -ti server
dotnet test $SCRIPTPATH/integration-tests/integration-tests.csproj --logger trx
RESULT_IT=$?

docker rm -f server || true

if [ $RESULT_UT -ne 0 ]; then
  echo Unit tests failed
  exit $RESULT_UT 
fi

if [ $RESULT_IT -ne 0 ]; then
  echo Integration tests failed
  exit $RESULT_IT
fi

exit 0
