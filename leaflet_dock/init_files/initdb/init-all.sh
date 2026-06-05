#!/bin/bash
set -e

# BASH_SOURCE, а не $0: postgres-образ подключает init-скрипты через `source`,
# поэтому $0 указывает на сам docker-entrypoint, а не на этот файл.
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "⏳ Создание БД mapstore, если не существует..."

DB_EXISTS=$(psql -U "$POSTGRES_USER" -tAc "SELECT 1 FROM pg_database WHERE datname = 'myrealm'")

if [ "$DB_EXISTS" != "1" ]; then
  echo "📦 Создаём базу данных mapstore..."
  createdb -U "$POSTGRES_USER" -O "$POSTGRES_USER" myrealm
else
  echo "✅ База myrealm уже существует."
fi


echo "📥 Импорт структуры в mapstore из main.sql..."
cd "$SCRIPT_DIR/sql_scripts"
psql -v ON_ERROR_STOP=1 -U "$POSTGRES_USER" -f main.sql

echo "✅ Импорт завершён!"
