package config

import (
	"os"
)

type Config struct {
	RedisAddr string
	Port      string
}

func Load() Config {
	return Config{
		RedisAddr: os.Getenv("REDIS_ADDR"),
		Port:      os.Getenv("PORT"),
	}
}
