FROM mcr.microsoft.com/dotnet/aspnet:8.0.18 AS base
WORKDIR /app
EXPOSE 5130 
ENV ASPNETCORE_URLS=http://+:5130
ENV TZ=Europe/Zagreb
RUN apt-get update && apt-get install -y tzdata && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0.412 AS build
WORKDIR /src
COPY . .

FROM build AS publish
RUN dotnet publish "Startupba.WebAPI/Startupba.WebAPI.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
COPY Startupba.WebAPI/Assets ./Assets

ENTRYPOINT ["dotnet", "Startupba.WebAPI.dll"]
