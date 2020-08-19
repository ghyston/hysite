FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-bionic
ARG source=./out
WORKDIR /app
COPY $source .
ENV ASPNETCORE_URLS http://*:80
EXPOSE 80
RUN cp /out/* /app/
ENTRYPOINT ["dotnet", "hysite.dll"]