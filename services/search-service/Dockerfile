FROM golang:1.25-alpine AS builder
WORKDIR /app
COPY services/search-service/go.mod services/search-service/go.sum ./
RUN go mod download
COPY services/search-service/ .
RUN CGO_ENABLED=0 GOOS=linux go build -o /app/server ./cmd/server

FROM alpine:3.20 AS runtime
RUN apk add --no-cache ca-certificates
WORKDIR /app
COPY --from=builder /app/server .
EXPOSE 8080
ENTRYPOINT ["./server"]