# Library Management System (Web API)

Система керування бібліотекою — навчальний проєкт Web API на платформі **.NET 8 (C#)**, розроблений із застосуванням принципів **Clean Architecture**, методології **Test-Driven Development (TDD)** та інструментів **GitHub Copilot / AI Pair Programming**.

---

## 📌 Архітектура проєкту (Clean Architecture)

Рішення організовано за шарами з чітким розділенням відповідальностей та інверсією залежностей:

```text
LibraryManagementSystem/
├── Library.Core/               # Доменні моделі (Book, Author, BookCopy, Loan) та інтерфейси репозиторіїв
├── Library.BLL/                # Бізнес-логіка (LoanService, BookService, валідація, правила роботи з книгами)
├── Library.DAL/                # Рівень доступу до даних (демонстраційні репозиторії в пам'яті)
├── Library.Tests/              # Модульні тести (xUnit, Moq, FluentAssertions)
├── Controllers/                # REST API контролери (LoansController, BooksController)
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

## 📋 Реалізований функціонал (Epic 1 / ЛР №2)

- **Видача примірника книги читачеві (`POST /api/loans/issue`)**:
  - Перевірка доступності примірника книги (`BookCopyStatus.Available`)
  - Створення нового запису позики (`Loan`) із фіксацією дати видачі (`BorrowDate`) та планової дати повернення (`DueDate`)
  - Оновлення статусу примірника на зайнятий (`BookCopyStatus.Borrowed`)
  - Захист від повторної видачі зайнятої книги (повернення `400 Bad Request: "Book copy is not available."`)
- **Повернення примірника книги (`POST /api/loans/return`)**:
  - Перевірка існування та активності позики із фіксацією дати повернення (`ReturnDate`)
  - Відновлення статусу примірника на доступний (`BookCopyStatus.Available`)
  - Захист від повторного повернення або неіснуючої позики (`400 Bad Request`)
- **Активні позики читача (`GET /api/loans/reader/{readerId}`)**:
  - Перегляд усіх неповернутих позик конкретного читача
- **Каталог книг та примірників (`BooksController`)**:
  - `GET /api/books` — перегляд та пошук книг за назвою, автором або ISBN (`?query=...`)
  - `GET /api/books/{id}` — отримання книги за ID (`404 Not Found` якщо не знайдено)
  - `POST /api/books` — додавання нової книги до каталогу з валідацією (`201 Created`)
  - `GET /api/books/copies` — перегляд усіх примірників книг та їхніх статусів
- **Тестування**:
  - Повне покриття бізнес-логіки та контролерів юніт-тестами (30/30 тестів проходять успішно)

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
