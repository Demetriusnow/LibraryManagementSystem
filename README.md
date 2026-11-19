# Library Management System (Web API)

Система керування бібліотекою — навчальний проєкт Web API на платформі **.NET 8 (C#)**, розроблений із застосуванням принципів **Clean Architecture**, методології **Test-Driven Development (TDD)** та інструментів **GitHub Copilot / AI Pair Programming**.

---

## 📌 Архітектура проєкту (Clean Architecture)

Рішення організовано за шарами з чітким розділенням відповідальностей та інверсією залежностей:

```text
LibraryManagementSystem/
├── Library.Core/               # Доменні моделі, DTO, переліки статусів та інтерфейси
├── Library.BLL/                # Бізнес-логіка (LoanService, BookService, AuthorService, ReservationService)
├── Library.DAL/                # Рівень доступу до даних (демонстраційні репозиторії в пам'яті)
├── Library.Tests/              # Модульні тести (xUnit, Moq, FluentAssertions)
├── Controllers/                # REST API контролери (LoansController, BooksController, AuthorsController, ReservationsController)
├── Program.cs                  # Налаштування DI-контейнера, Swagger та пайплайну
└── README.md                   # Документація проєкту
```

---

## 🚀 Методологія розробки (TDD: Red → Green → Refactor)

Уся бізнес-логіка розробляється строго за циклом TDD:
1. **Red**: Створення модульного тесту на основі Acceptance Criteria (AC) із Jira, який падає або не компілюється через відсутність реалізації
2. **Green**: Написання мінімально необхідного коду сервісу для успішного проходження тестів
3. **Refactor**: Покращення структури коду, чистка та інтеграція в DI й REST API

---

## 📋 Реалізований функціонал (Epic 1 / ЛР №2 — усі 4 User Stories)

- **1. Видача та повернення примірників книг (`LoansController` / `LoanService`)**:
  - `POST /api/loans/issue` — видача доступного примірника зі зміною статусу на `Borrowed` і терміном повернення +14 днів (`400 Bad Request: "Book copy is not available."` для зайнятих)
  - `POST /api/loans/return` — повернення книги з фіксацією `ReturnDate` та відновленням статусу примірника (`Available` або `Reserved`)
  - `GET /api/loans/reader/{readerId}` — активні позики конкретного читача

- **2. Додавання книг, авторів та облік примірників (`BooksController`, `AuthorsController`)**:
  - CRUD для `Book` (`GET`, `POST`, `PUT`, `DELETE /api/books`), `Author` (`/api/authors`) та `BookCopy` (`/api/books/{id}/copies`, `/api/books/copies`)
  - Валідація унікальності `ISBN` та інвентарного номера формату `INV-xxx`
  - Заборона створення/оновлення книги з роком видання в майбутньому
  - Прив'язка кількох фізичних примірників (`BookCopy`) до одного запису книги

- **3. Пошук та фільтрація каталогу (`BookService`)**:
  - `GET /api/books?query=...&category=...&page=1&pageSize=10` — case-insensitive пошук за назвою, автором і ISBN з фільтрацією за категорією та пагінацією
  - Відображення кількості доступних фізичних примірників (`AvailableCopiesCount`) у результатах пошуку
  - Повернення порожнього списку (`200 OK`) без збоїв, якщо нічого не знайдено

- **4. Резервування літератури та контроль прострочених повернень (`ReservationsController`, `LoanService`)**:
  - `GET /api/loans/overdue` — автоматичне маркування прострочених позик статусом `Overdue`, якщо поточна дата перевищує `DueDate`
  - `GET /api/loans/overdue-readers` — формування списку читачів із простроченими книгами
  - `POST /api/reservations`, `GET /api/reservations/book-copy/{bookCopyId}`, `DELETE /api/reservations/{id}` — керування чергою резерву на зайняті примірники
  - Автоматичне сповіщення першого читача в черзі та бронювання примірника на 48 годин (`Reserved`) після повернення книги (`GET /api/reservations/notifications`)

---

## 🛠 Інструкція із запуску

### Вимоги:
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git

### 1. Клонування репозиторію:
```bash
git clone https://github.com/Demetriusnow/LibraryManagementSystem.git
cd LibraryManagementSystem
```

### 2. Запуск тестів:
```bash
dotnet test
```

### 3. Запуск Web API:
```bash
dotnet run
```
Після запуску відкрийте Swagger UI у браузері:
`https://localhost:<port>/swagger` або `http://localhost:<port>/swagger`
