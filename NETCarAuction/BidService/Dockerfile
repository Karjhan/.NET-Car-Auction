FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
EXPOSE 80
# COPY ALL .CSPROJ FILES AND RESTORE AS DISTINCT LAYERS
# USE SAME COPY COMMAND FOR EVERY DOCKERFILE IN THE PROJECT TO TAKE ADVANTAGE OF DOCKER CACHING
COPY NETCarAuction.sln NETCarAuction.sln
COPY /AuctionService/AuctionService.csproj src/AuctionService/AuctionService.csproj
COPY /SearchService/SearchService.csproj src/SearchService/SearchService.csproj
COPY /GatewayService/GatewayService.csproj src/GatewayService/GatewayService.csproj
COPY /NotificationService/NotificationService.csproj src/NotificationService/NotificationService.csproj
COPY /BidService/BidService.csproj src/BidService/BidService.csproj
COPY /Contracts/Contracts.csproj src/Contracts/Contracts.csproj
COPY /IdentityService/IdentityService.csproj src/IdentityService/IdentityService.csproj

# RESTORE PACKAGE DEPENDECIES
RUN dotnet restore  src/AuctionService/AuctionService.csproj
RUN dotnet restore src/SearchService/SearchService.csproj
RUN dotnet restore  src/GatewayService/GatewayService.csproj
RUN dotnet restore  src/BidService/BidService.csproj
RUN dotnet restore  src/Contracts/Contracts.csproj
RUN dotnet restore  src/IdentityService/IdentityService.csproj
RUN dotnet restore  src/NotificationService/NotificationService.csproj
# RUN dotnet restore NETCarAuction/NETCarAuction.sln

# COPY THE APP FOLDERS OVER
COPY /BidService src/BidService
COPY /Contracts src/Contracts
WORKDIR /app/src/BidService
RUN dotnet publish "BidService.csproj" -c Release -o /app/src/out

# BUILD RUNTIME IMAGE
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
COPY --from=build /app/src/out . 
ENTRYPOINT [ "dotnet", "BidService.dll" ]
