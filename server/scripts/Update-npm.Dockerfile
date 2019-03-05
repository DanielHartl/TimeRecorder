FROM microsoft/dotnet:2.2-sdk AS builder

RUN apt-get -y update && \
    apt-get -y install build-essential && \
    curl -sL https://deb.nodesource.com/setup_10.x | bash - && \
    apt-get install -y nodejs


WORKDIR /source

COPY src/Server.App/ ./
RUN cd /source/ClientApp && npm install
RUN cd /source/ClientApp && npm audit fix