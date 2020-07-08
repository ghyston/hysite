FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-bionic
ARG source=./Hysite.Web/bin/Release/netcoreapp3.1/publish
WORKDIR /app
COPY $source .
ENV ASPNETCORE_URLS http://*:5000
EXPOSE 5000
RUN cp /app/runtimes/ubuntu.18.04-x64/native/* /app/
ENTRYPOINT ["dotnet", "hysite.dll"]