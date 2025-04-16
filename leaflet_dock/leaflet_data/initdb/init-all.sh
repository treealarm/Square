#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

echo "⏳ Создание БД MapStore, если не существует..."

DB_EXISTS=$(psql -U "$POSTGRES_USER" -tAc "SELECT 1 FROM pg_database WHERE datname = 'MapStore'")

if [ "$DB_EXISTS" != "1" ]; then
  echo "📦 Создаём базу данных MapStore..."
  createdb -U "$POSTGRES_USER" -O "$POSTGRES_USER" MapStore
else
  echo "✅ База MapStore уже существует."
fi


echo "📥 Импорт структуры в MapStore из main.sql..."
cd "$SCRIPT_DIR/sql_scripts"
psql -v ON_ERROR_STOP=1 -U "$POSTGRES_USER" -f main.sql

echo "✅ Импорт завершён!"
