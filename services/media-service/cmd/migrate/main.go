package main

import (
	"context"
	"database/sql"
	"log"
	"os"
	"time"

	_ "github.com/jackc/pgx/v5/stdlib"
	"github.com/pressly/goose/v3"

	"media-service/config"
)

const migrationsDir = "migrations"

func main() {
	if len(os.Args) < 2 {
		log.Fatal("usage: migrate <up|down|status|redo> [args...]")
	}
	command := os.Args[1]

	cfg := config.Load()

	db, err := sql.Open("pgx", cfg.DatabaseURL)
	if err != nil {
		log.Fatalf("connect postgres: %v", err)
	}
	defer func(db *sql.DB) {
		err := db.Close()
		if err != nil {

		}
	}(db)

	if err := waitForDB(db, 15, 2*time.Second); err != nil {
		log.Fatalf("postgres not reachable: %v", err)
	}

	if err := goose.SetDialect("postgres"); err != nil {
		log.Fatalf("set dialect: %v", err)
	}

	if err := goose.RunContext(context.Background(), command, db, migrationsDir, os.Args[2:]...); err != nil {
		log.Fatalf("goose %s: %v", command, err)
	}
}

func waitForDB(db *sql.DB, maxAttempts int, delay time.Duration) error {
	var err error
	for i := 1; i <= maxAttempts; i++ {
		if err = db.Ping(); err == nil {
			return nil
		}
		log.Printf("waiting for postgres (attempt %d/%d): %v", i, maxAttempts, err)
		time.Sleep(delay)
	}
	return err
}
