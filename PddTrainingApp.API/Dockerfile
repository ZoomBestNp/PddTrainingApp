FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["PddTrainingApp.API/PddTrainingApp.API.csproj", "PddTrainingApp.API/"]
COPY ["PddTrainingApp.Models/PddTrainingApp.Models.csproj", "PddTrainingApp.Models/"]
RUN dotnet restore "./PddTrainingApp.API/PddTrainingApp.API.csproj"

COPY . .
WORKDIR /src/PddTrainingApp.API
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV LC_ALL=en_US.UTF-8
ENV LANG=en_US.UTF-8

EXPOSE 8080

ENTRYPOINT ["dotnet", "PddTrainingApp.API.dll"]