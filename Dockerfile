FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

#COPY Hysite.Web/hysite.csproj ./
#RUN dotnet restore

COPY Hysite.Web ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-bionic

ARG version="unknown"
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS http://*:80
ENV HYSITE_VERSION $version
EXPOSE 80

ENTRYPOINT ["dotnet", "hysite.dll"]