# Base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./JubiAPI/*.csproj ./JubiAPI/
COPY ./DiscordBot/*.csproj ./DiscordBot/
RUN dotnet restore ./JubiAPI/JubiAPI.csproj
COPY . .
#WORKDIR "src/JubiAPI"
RUN dotnet build "JubiAPI/JubiAPI.csproj" -c Release -o /app/build -v d

# publish image
FROM build as publish
RUN dotnet publish "JubiAPI/JubiAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# final image
FROM base as final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JubiAPI.dll"]
