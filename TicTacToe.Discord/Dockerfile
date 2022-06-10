#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["TicTacToe.Discord/TicTacToe.Discord.csproj", "TicTacToe.Discord/"]
COPY ["TicTacToe.Game/TicTacToe.Game.csproj", "TicTacToe.Game/"]
RUN dotnet restore "TicTacToe.Discord/TicTacToe.Discord.csproj"
COPY . .
WORKDIR "/src/TicTacToe.Discord"
RUN dotnet build "TicTacToe.Discord.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TicTacToe.Discord.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TicTacToe.Discord.dll"]