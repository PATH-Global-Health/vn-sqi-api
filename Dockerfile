FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["SQi/SQi/SQi.csproj", "SQi/"]
COPY ["SQi/Data/Data.csproj", "Data/"]
COPY ["SQi/Services/Services.csproj", "Services/"]
RUN dotnet restore "SQi/SQi.csproj"
COPY . .
WORKDIR "/src/SQi/SQi"
RUN dotnet build "SQi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SQi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SQi.dll"]
