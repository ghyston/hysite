FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY src ./src
COPY tests ./tests
COPY hysite.sln ./hysite.sln
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0

ARG HYSITE_VERSION="latest"
ENV HYSITE_VERSION=$HYSITE_VERSION

ARG READER_TOKEN
ENV READER_TOKEN=$READER_TOKEN

WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS https://+;http://+
ENV ASPNETCORE_HTTPS_PORT 443
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "hysite.dll"]