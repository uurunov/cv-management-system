FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /cv-management-app
COPY ["cv-management-app.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o out

FROM node:24-alpine AS client-build
WORKDIR /client
COPY ["client/package.json", "client/package-lock.json", "./"]
RUN npm ci
COPY client/. .
RUN npm run build

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /cv-management-app
COPY --from=build /cv-management-app/out . 
COPY --from=client-build /client/dist/client/browser ./wwwroot
EXPOSE 10000
ENTRYPOINT ["dotnet", "cv-management-app.dll"]