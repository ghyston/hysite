FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

COPY Hysite.Web/hysite.csproj ./
RUN dotnet restore

COPY Hysite.Web ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-bionic
#ARG source=./Hysite.Web/bin/Release/netcoreapp3.1/publish
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS http://*:80
EXPOSE 80
#RUN cp /app/runtimes/ubuntu.18.04-x64/native/* /app/
ENTRYPOINT ["dotnet", "hysite.dll"]