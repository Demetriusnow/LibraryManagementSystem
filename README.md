# Library Management System (Web API)

Система керування бібліотекою — навчальний проєкт Web API на платформі **.NET 8 (C#)**, розроблений із застосуванням принципів **Clean Architecture**, методології **Test-Driven Development (TDD)** та контейнеризації **Docker / Docker Compose + PostgreSQL + PgAdmin**.

---

## 📌 Архітектура проєкту (Clean Architecture)

Рішення організовано за шарами з чітким розділенням відповідальностей та інверсією залежностей:

```text
LibraryManagementSystem/
├── Library.Core/               # Доменні моделі, DTO, переліки статусів та інтерфейси
├── Library.BLL/                # Бізнес-логіка (LoanService, BookService, AuthorService, ReservationService)
├── Library.DAL/                # Рівень доступу до даних (EF Core PostgreSQL, міграції, DbSeeder та In-Memory репозиторії)
├── Library.Tests/              # Модульні та інтеграційні тести (xUnit, Moq, FluentAssertions, EF InMemory)
├── Controllers/                # REST API контролери (LoansController, BooksController, AuthorsController, ReservationsController)
├── Dockerfile                  # Багатоетапна збірка Web API контейнера
├── docker-compose.yaml         # Оркестрація Web API, PostgreSQL та PgAdmin
├── .env                        # Параметри підключення до БД та облікові дані для сервісів
├── Program.cs                  # Налаштування DI-контейнера, EF Core PostgreSQL, міграцій і Swagger
└── README.md                   # Документація проєкту
```

---

## 🚀 Методологія розробки (TDD: Red → Green → Refactor)

Уся бізнес-логіка розробляється строго за циклом TDD:
1. **Red**: Створення модульного тесту на основі Acceptance Criteria (AC) із Jira, який падає або не компілюється через відсутність реалізації
2. **Green**: Написання мінімально необхідного коду сервісу для успішного проходження тестів
3. **Refactor**: Покращення структури коду, чистка та інтеграція в DI й REST API

---

## 📋 Реалізований функціонал (ЛР №2 + ЛР №3)

- **Видача та повернення примірників книг (`LoansController` / `LoanService`)**:
  - `POST /api/loans/issue` — видача доступного примірника зі зміною статусу на `Borrowed` і терміном повернення +14 днів (`400 Bad Request: "Book copy is not available."` для зайнятих)
  - `POST /api/loans/return` — повернення книги з фіксацією `ReturnDate` та відновленням статусу примірника (`Available` або `Reserved`)
  - `GET /api/loans/reader/{readerId}` — активні позики конкретного читача

- **Додавання книг, авторів та облік примірників (`BooksController`, `AuthorsController`)**:
  - CRUD для `Book` (`GET`, `POST`, `PUT`, `DELETE /api/books`), `Author` (`/api/authors`) та `BookCopy` (`/api/books/{id}/copies`, `/api/books/copies`)
  - Валідація унікальності `ISBN` та інвентарного номера формату `INV-xxx`
  - Заборона створення/оновлення книги з роком видання в майбутньому
  - Прив'язка кількох фізичних примірників (`BookCopy`) до одного запису книги

- **Пошук та фільтрація каталогу (`BookService`)**:
  - `GET /api/books?query=...&category=...&page=1&pageSize=10` — case-insensitive пошук за назвою, автором і ISBN з фільтрацією за категорією та пагінацією
  - Відображення кількості доступних фізичних примірників (`AvailableCopiesCount`) у результатах пошуку
  - Повернення порожнього списку (`200 OK`) без збоїв, якщо нічого не знайдено

- **Резервування літератури та контроль прострочених повернень (`ReservationsController`, `LoanService`)**:
  - `GET /api/loans/overdue` — автоматичне маркування прострочених позик статусом `Overdue`, якщо поточна дата перевищує `DueDate`
  - `GET /api/loans/overdue-readers` — формування списку читачів із простроченими книгами
  - `POST /api/reservations`, `GET /api/reservations/book-copy/{bookCopyId}`, `DELETE /api/reservations/{id}` — керування чергою резерву на зайняті примірники
  - Автоматичне сповіщення першого читача в черзі та бронювання примірника на 48 годин (`Reserved`) після повернення книги (`GET /api/reservations/notifications`)

- **Контейнеризація та збереження даних у PostgreSQL (ЛР №3)**:
  - Автоматичне застосування EF Core міграцій (`MigrateAsync`) та початковий посів даних (`DbSeeder`) при старті API
  - Ізольована мережа `library-network` для взаємодії контейнерів `api`, `postgres` та `pgadmin`
  - Docker Volume `postgres_data` для гарантованого збереження даних після перезапуску контейнерів
  - Налаштування параметрів підключення та паролів через `.env`

---

## 🐳 Запуск у Docker Compose (ЛР №3)

### Вимоги:
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (із підтримкою `docker compose`)

### 1. Збірка та запуск усіх 3 контейнерів (`api`, `postgres`, `pgadmin`):
```bash
docker compose up -d --build
```

### 2. Доступ через браузер:
- **Swagger Web API:** `http://localhost:8080/swagger`
- **PgAdmin:** `http://localhost:5050`
  - Логін / пароль для входу в PgAdmin беруться з `.env` (`admin@library.com` / `admin_pass`)
  - Підключення до бази даних у PgAdmin (*Register → Server*):
    - **Host name/address:** `postgres`
    - **Port:** `5432`
    - **Maintenance database:** `library_db`
    - **Username / Password:** `library_user` / `library_pass`

### 3. Перевірка збереження даних після перезапуску:
```bash
docker compose restart
```
або повний перепуск контейнерів (зі збереженням Docker Volume):
```bash
docker compose down
docker compose up -d
```

---

## 🛠 Локальний запуск та тестування (.NET 8 SDK)

### Запуск модульних тестів:
```bash
dotnet test
```

### Локальний запуск Web API (потребує запущеного PostgreSQL за рядком з `appsettings.json`):
```bash
dotnet run
```
