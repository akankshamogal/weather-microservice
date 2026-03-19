FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . .

RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app


RUN apt-get update && apt-get install -y libkrb5-3 libgssapi-krb5-2

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WeatherService.dll"]