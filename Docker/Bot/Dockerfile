# Use official .NET SDK image to build the bot
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set working directory inside the container
WORKDIR /app

# Clone the bot repository
RUN apt-get update && apt-get install -y git && \
    git clone https://github.com/Koin101/Dank-Discord-Bot.git .

# Restore dependencies and build the project
RUN dotnet restore
RUN dotnet build "Discord Bot.csproj" -c Release -o /app/build --no-restore 

FROM build AS publish
RUN dotnet publish "Discord Bot.csproj" -c Release -o /app/publish

# Use a runtime image to run the bot
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set working directory inside the container
WORKDIR /app

# Copy the built bot from the previous stage
COPY --from=publish /app/publish .

# Run the bot
CMD ["dotnet", "Discord Bot.dll"]
