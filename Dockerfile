FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /cv-management-app

COPY ["cv-management-app.csproj", "./"]
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /cv-management-app
COPY --from=build /cv-management-app/out . 
EXPOSE 8080
ENTRYPOINT ["dotnet", "cv-management-app.dll"]