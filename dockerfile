
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only project files first (for better build caching)
COPY *.csproj .
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false


# ============================
# Stage 2: Runtime image
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Set environment to production
ENV ASPNETCORE_ENVIRONMENT=production
ENV ASPNETCORE_URLS=http://+:5000

# Expose port
EXPOSE 5000

# Run the app
ENTRYPOINT ["dotnet", "IEEE_Application.dll"]
