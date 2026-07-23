FROM golang:1.25-alpine AS builder
WORKDIR /app
COPY services/media-service/go.mod services/media-service/go.sum ./
RUN go mod download
COPY services/media-service/ .
RUN CGO_ENABLED=0 GOOS=linux go build -o /app/server ./cmd/server
RUN CGO_ENABLED=0 GOOS=linux go build -o /app/migrate ./cmd/migrate

FROM alpine:3.20 AS runtime
RUN apk add --no-cache ca-certificates
WORKDIR /app
COPY --from=builder /app/server .
COPY services/media-service/migrations ./migrations
EXPOSE 8080
ENTRYPOINT ["./server"]

FROM alpine:3.20 AS migrate
RUN apk add --no-cache ca-certificates
WORKDIR /app
COPY --from=builder /app/migrate .
COPY services/media-service/migrations ./migrations
ENTRYPOINT ["./migrate"]
CMD ["up"]