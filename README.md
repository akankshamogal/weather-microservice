# Weather Microservice

## Overview
A .NET microservice that fetches real-time weather data for 10 cities and stores it in PostgreSQL.

## Features
- Parallel API calls using Task.WhenAll
- Retry mechanism (3 attempts)
- PostgreSQL data storage
- Idempotency (no duplicate records)
- Dockerized application
- Environment variable based configuration

## Tech Stack
- .NET 8 (C#)
- PostgreSQL
- Docker

## Run

docker build -t weather-app .
docker run --rm -e DB_CONNECTION="Host=host.docker.internal;Username=postgres;Password=postgres;Database=weather" weather-app