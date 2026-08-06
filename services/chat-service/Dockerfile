FROM golang:1.25-alpine AS builder
WORKDIR /app
COPY services/chat-service/go.mod services/chat-service/go.sum ./
RUN go mod download
COPY services/chat-service/ .
RUN CGO_ENABLED=0 GOOS=linux go build -o /app/server ./cmd/server

FROM alpine:3.20 AS runtime
RUN apk add --no-cache ca-certificates
WORKDIR /app
COPY --from=builder /app/server .
EXPOSE 8080
ENTRYPOINT ["./server"]