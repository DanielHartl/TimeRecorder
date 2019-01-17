FROM microsoft/dotnet:2.2-sdk AS builder

RUN curl -sL https://deb.nodesource.com/setup_10.x |  bash -
RUN apt-get install -y nodejs

WORKDIR /source

COPY ./ ./
RUN dotnet test
RUN dotnet publish "./src/Server.App/ActivityTracker.Server.App.csproj" --output "/dist" --configuration Release

FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY --from=builder /dist .
EXPOSE 80
ENTRYPOINT ["dotnet", "ActivityTracker.Server.App.dll"]