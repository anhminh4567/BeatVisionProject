#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
ENV TZ=Asia/Ho_Chi_Minh
RUN apt-get update && apt-get install -y tzdata
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
#ARG BUILD_CONFIGURATION=Debug
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BeatVisionProject/BeatVisionProject.csproj", "BeatVisionProject/"]
COPY ["Services/Services.csproj", "Services/"]
COPY ["Repository/Repository.csproj", "Repository/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "./BeatVisionProject/BeatVisionProject.csproj"
COPY . .
WORKDIR "/src/BeatVisionProject"
RUN dotnet build "./BeatVisionProject.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BeatVisionProject.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY BeatVisionProject/wwwroot /app/wwwroot
ENTRYPOINT ["dotnet", "BeatVisionProject.dll"]