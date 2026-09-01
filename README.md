# Ecommerce Backend

This repository contains the backend services for the ecommerce platform, built with a microservices architecture. Each service owns its own database and communicates with other services through gRPC (synchronous) and Kafka (asynchronous) messaging.

## Prerequisites

Before running this project, make sure you have the following installed on your machine:

- Docker Desktop (includes Docker Engine and Docker Compose)
- Git

You do not need to install .NET, Go, Node.js, PostgreSQL, MongoDB, Redis, or any other individual dependency on your host machine. Everything runs inside containers.

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/nguyenanhtu205/ecommerce-backend.git
cd ecommerce-backend
```

### 2. Configure environment variables

Copy the example environment file and adjust the values if needed:

```bash
cp .env.example .env
```

Open the `.env` file and configure any required environment variables (for example, SMTP credentials for the notification service). For VNPay, no configuration changes are required at this time, as the payment integration has not been implemented yet. You can leave the VNPay values unchanged from `.env.example`.


### 3. Start all services

Build and start every service, database, and supporting infrastructure (Kafka, Redis, MongoDB, PostgreSQL, MinIO, Elasticsearch, Kong, KrakenD) with a single command:

```bash
docker compose up --build
```

The first run may take a few minutes since it needs to build every service image and download the base images for the infrastructure components.

To run the stack in the background, add the `-d` flag:

```bash
docker compose up --build -d
```

### 4. Verify the services are running

Once all containers are up, you can check their status with:

```bash
docker compose ps
```

Each service should be reachable through the Kong API Gateway. Refer to the `gateway/kong/kong.yml` file for the exposed routes, or check each service's port mapping in `docker-compose.yml` for direct access during development.

### 5. Stopping the stack

To stop all running containers:

```bash
docker compose down
```

To stop the containers and also remove all persisted data (databases, message queues, uploaded files):

```bash
docker compose down -v
```

## Rebuilding a Single Service

If you make changes to a specific service and want to rebuild only that one instead of the whole stack:

```bash
docker compose up --build <service-name>
```

For example:

```bash
docker compose up --build order-service
```

## Troubleshooting

- If a service fails to start because a dependency is not ready yet, simply run `docker compose up --build` again; most services will retry their connection automatically.
- If you change the database schema for a service, you may need to remove its volume and restart so migrations run cleanly: `docker compose down -v` followed by `docker compose up --build`.
- If port conflicts occur, check `docker-compose.yml` for the port mappings and make sure nothing else on your machine is using the same ports.
