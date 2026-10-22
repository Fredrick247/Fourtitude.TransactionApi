FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Fourtitude.TransactionApi.slnx ./
COPY src/Fourtitude.TransactionApi.Api/Fourtitude.TransactionApi.Api.csproj src/Fourtitude.TransactionApi.Api/
COPY tests/Fourtitude.TransactionApi.Tests/Fourtitude.TransactionApi.Tests.csproj tests/Fourtitude.TransactionApi.Tests/
RUN dotnet restore Fourtitude.TransactionApi.slnx

COPY . .
RUN dotnet test tests/Fourtitude.TransactionApi.Tests --no-restore
RUN dotnet publish src/Fourtitude.TransactionApi.Api -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

RUN mkdir -p /app/logs && chown $APP_UID /app/logs

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID

ENTRYPOINT ["dotnet", "Fourtitude.TransactionApi.Api.dll"]