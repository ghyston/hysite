FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

COPY Hysite.Web/hysite.csproj ./
RUN dotnet restore

COPY Hysite.Web ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:5.0-focal

ARG HYSITE_VERSION="latest"
ENV HYSITE_VERSION=$HYSITE_VERSION

ARG PFX_PASSWORD
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=${PFX_PASSWORD}
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/app/cert/certificate.pfx

WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS https://+;http://+
ENV ASPNETCORE_HTTPS_PORT 443
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "hysite.dll"]