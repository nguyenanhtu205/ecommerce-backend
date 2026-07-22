## Run Database Migration

Execute the following command from the project root directory:

```bash
docker exec -i ecommerce-backend-postgres-1 \
psql -U root -d notification_db \
< services/notification-service/migrations/0001_init.sql
```