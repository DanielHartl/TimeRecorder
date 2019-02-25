FROM microsoft/dotnet:2.2-sdk AS builder

RUN apt-get -y update && \
    apt-get -y install build-essential && \
    curl -sL https://deb.nodesource.com/setup_10.x | bash - && \
    apt-get install -y nodejs


WORKDIR /source

COPY ./ ./
RUN dotnet publish "./src/Server.App/TimeRecorder.Server.App.csproj" --output "/dist" --configuration Release

FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY --from=builder /dist .
EXPOSE 80
ENTRYPOINT ["dotnet", "TimeRecorder.Server.App.dll"]