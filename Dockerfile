# multi-stage dockerfile

# build stage/image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.slnx ./
# move csproj files to the root
COPY --parents **/*.csproj ./ 
RUN dotnet restore 
# copy everything else and build app
COPY . .
RUN dotnet publish -c Release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
# copy the build app from the build stage to the final stage
COPY --from=build /app ./

RUN find /app -type f

ENTRYPOINT ["dotnet", "WiseBet.backend.dll"]