FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["MP4ToWebMBot/MP4ToWebMBot.csproj", "MP4ToWebMBot/"]
RUN dotnet restore "MP4ToWebMBot/MP4ToWebMBot.csproj"
COPY . .
WORKDIR "/src/MP4ToWebMBot"
RUN dotnet build "MP4ToWebMBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MP4ToWebMBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=mwader/static-ffmpeg:4.2.1 /ffmpeg /ffprobe /usr/local/bin/
ENTRYPOINT ["dotnet", "MP4ToWebMBot.dll"]
