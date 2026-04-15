# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /build

# Copy solution & projects
COPY Backend/CinemaBookingSystem.sln .
COPY Backend/CinemaBooking.Domain/ CinemaBooking.Domain/
COPY Backend/CinemaBooking.Application/ CinemaBooking.Application/
COPY Backend/CinemaBooking.Infrastructure/ CinemaBooking.Infrastructure/
COPY Backend/CinemaBooking.API/ CinemaBooking.API/

# Restore & build
RUN dotnet restore
RUN dotnet build -c Release --no-restore

# Stage 2: Publish
FROM builder AS publisher

WORKDIR /build

RUN dotnet publish CinemaBooking.API/CinemaBooking.API.csproj \
    -c Release \
    -o /publish \
    --no-build \
    --no-restore

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=publisher /publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

EXPOSE 5000

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "CinemaBooking.API.dll"]
