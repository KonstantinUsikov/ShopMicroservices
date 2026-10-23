# ShopMicroservices

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)
![Docker](https://img.shields.io/badge/Docker_Compose-2496ED?logo=docker&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?logo=postgresql&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?logo=microsoftsqlserver&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-003B57?logo=sqlite&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-DC382D?logo=redis&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-FF6600?logo=rabbitmq&logoColor=white)
![gRPC](https://img.shields.io/badge/gRPC-244c5a?logo=grpc&logoColor=white)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)

Backend интернет-магазина на микросервисной архитектуре: .NET, Docker, gRPC, RabbitMQ, YARP.

## Содержание

- [О проекте](#о-проекте)
- [Сервисы](#сервисы)
- [Технологический стек](#технологический-стек)
- [Архитектурные решения и паттерны](#архитектурные-решения-и-паттерны)
- [Структура репозитория](#структура-репозитория)
- [Быстрый старт (Docker)](#быстрый-старт-docker)
- [Локальный запуск без Docker](#локальный-запуск-без-docker)
- [Примеры запросов](#примеры-запросов)
- [Конфигурация](#конфигурация)

## О проекте

ShopMicroservices — учебный проект, в котором бэкенд интернет-магазина разделён на независимые сервисы: каталог товаров, корзина, скидки и заказы. У каждого сервиса своя база данных и свой архитектурный стиль, а наружу они выставлены через единый API Gateway.

**Что показывает проект:**

- 4 микросервиса и API Gateway, поднимаются одной командой `docker compose up`;
- три вида взаимодействия: **REST** (клиент → gateway → сервисы), **gRPC** (Basket → Discount) и **асинхронные события** через RabbitMQ (Basket → Ordering);
- **Database per Service**: PostgreSQL как документная БД (Marten), SQL Server и SQLite (EF Core), Redis как распределённый кэш;
- разные подходы к организации кода: **Vertical Slice Architecture** (Catalog, Basket) и **Clean Architecture + DDD** (Ordering);
- **CQRS** на MediatR с pipeline behaviors для валидации и логирования;
- доменные и интеграционные события, feature flags, health checks, rate limiting, глобальная обработка ошибок в формате ProblemDetails.

## Сервисы

| Сервис | Назначение | Хранилище | Архитектура | Порт (Docker) |
|---|---|---|---|---|
| **Catalog.API** | CRUD товаров, пагинация, фильтр по категории | PostgreSQL (Marten, документная модель) | Vertical Slice, CQRS | `6000` |
| **Basket.API** | Корзина пользователя, применение скидок, checkout | PostgreSQL (Marten) + Redis | Vertical Slice, CQRS | `6001` |
| **Discount.Grpc** | CRUD купонов, расчёт скидки по товару | SQLite (EF Core) | gRPC-сервис | `6002` |
| **Ordering.API** | Создание и управление заказами | SQL Server (EF Core) | Clean Architecture, DDD, CQRS | `6003` |
| **YarpApiGateway** | Единая точка входа, маршрутизация, rate limiting | — | Reverse proxy | `6004` |

<details>
<summary><b>Эндпоинты сервисов</b></summary>

**Catalog.API** — через gateway: `/catalog-service/...`

| Метод | Путь | Описание |
|---|---|---|
| `GET` | `/products?pageNumber=1&pageSize=10` | Список товаров с пагинацией |
| `GET` | `/products/{id}` | Товар по Id |
| `GET` | `/products/category/{category}` | Товары по категории |
| `POST` | `/products` | Создать товар |
| `PUT` | `/products` | Обновить товар |
| `DELETE` | `/products/{id}` | Удалить товар |
| `GET` | `/health` | Health check (PostgreSQL) |

**Basket.API** — через gateway: `/basket-service/...`

| Метод | Путь | Описание |
|---|---|---|
| `GET` | `/basket/{username}` | Корзина пользователя |
| `POST` | `/basket` | Сохранить корзину (со скидками из Discount.Grpc) |
| `DELETE` | `/basket/{username}` | Удалить корзину |
| `POST` | `/basket/checkout` | Оформить заказ: публикует `ShoppingCartCheckoutEvent` |
| `GET` | `/health` | Health check (PostgreSQL, Redis) |

**Ordering.API** — через gateway: `/ordering-service/...`

| Метод | Путь | Описание |
|---|---|---|
| `GET` | `/orders?pageIndex=0&pageSize=10` | Список заказов с пагинацией |
| `GET` | `/orders/{orderName}` | Поиск заказов по имени |
| `GET` | `/orders/customer/{customerId}` | Заказы покупателя |
| `POST` | `/orders` | Создать заказ |
| `PUT` | `/orders` | Обновить заказ |
| `DELETE` | `/orders/{id}` | Удалить заказ |
| `GET` | `/health` | Health check (SQL Server) |

**Discount.Grpc** — контракт [`discount.proto`](shop-microservices/Services/Discount/Discount.Grpc/Protos/discount.proto):
`GetDiscount`, `CreateDiscount`, `UpdateDiscount`, `DeleteDiscount`.

</details>

## Технологический стек

| Область | Технологии |
|---|---|
| Платформа | .NET 8, C#, ASP.NET Core Minimal API |
| API | Carter, gRPC (Grpc.AspNetCore, Protobuf), YARP Reverse Proxy |
| CQRS и валидация | MediatR, FluentValidation, Mapster |
| Данные | Marten (PostgreSQL как document DB), Entity Framework Core 9 (SQL Server, SQLite), Redis (`IDistributedCache`) |
| Messaging | RabbitMQ, MassTransit |
| Инфраструктура | Docker, Docker Compose, ASP.NET Core Health Checks, Microsoft.FeatureManagement, Scrutor |

## Архитектурные решения и паттерны

- **Vertical Slice Architecture** (Catalog, Basket): каждая фича лежит в своей папке, рядом endpoint, команда/запрос, валидатор и обработчик, например [`Products/CreateProduct`](shop-microservices/Services/Catalog/Catalog.API/Products/CreateProduct).
- **Clean Architecture** (Ordering): слои `Domain` → `Application` → `Infrastructure` → `API`, зависимости направлены внутрь.
- **DDD** ([`Ordering.Domain`](shop-microservices/Services/Ordering/Ordering.Domain)): rich domain model, агрегат `Order` с инвариантами, Value Objects (`Address`, `Payment`, `OrderName`), strongly-typed IDs (`OrderId`, `CustomerId`, `ProductId`).
- **CQRS + MediatR**: общие абстракции `ICommand` / `IQuery` в [`BuildingBlocks/CQRS`](shop-microservices/BuildingBlocks/BuildingBlocks/CQRS).
- **Pipeline Behaviors**: [`ValidationBehavior`](shop-microservices/BuildingBlocks/BuildingBlocks/Behaviors/ValidationBehavior.cs) прогоняет команды через FluentValidation, [`LoggingBehavior`](shop-microservices/BuildingBlocks/BuildingBlocks/Behaviors/LoggingBehavior.cs) логирует запросы и предупреждает о тех, что выполняются дольше 3 секунд.
- **Доменные события**: агрегат накапливает события, а [`DispatchDomainEventsInterceptor`](shop-microservices/Services/Ordering/Ordering.Infrastructure/Data/Interceptors/DispatchDomainEventsInterceptor.cs) (EF Core `SaveChangesInterceptor`) публикует их через MediatR во время сохранения.
- **Интеграционные события**: общий контракт и подключение MassTransit/RabbitMQ вынесены в [`BuildingBlocks.Messaging`](shop-microservices/BuildingBlocks/BuildingBlocks.Messaging).
- **Decorator + Cache-Aside**: [`CachedShoppingCartRepository`](shop-microservices/Services/Basket/Basket.API/Data/CachedShoppingCartRepository.cs) оборачивает репозиторий корзины кэшем Redis и регистрируется через Scrutor `Decorate`.
- **Синхронное межсервисное взаимодействие по gRPC**: Basket получает клиент из общего `.proto`-контракта Discount.
- **Глобальная обработка ошибок**: [`CustomExceptionHandler`](shop-microservices/BuildingBlocks/BuildingBlocks/Exceptions/Handler/CustomExceptionHandler.cs) (`IExceptionHandler`) приводит исключения к `ProblemDetails` с нужным HTTP-статусом, `traceId` и списком ошибок валидации.
- **Feature Flags**: публикация события о созданном заказе включается флагом `OrderFulfillment`.
- **API Gateway**: YARP с маршрутами по префиксам и fixed-window rate limiter на Ordering.
- **Health Checks**: `/health` в Catalog, Basket и Ordering проверяет зависимости (PostgreSQL, Redis, SQL Server).
- **Автомиграции и сидинг**: Discount и Ordering применяют EF Core-миграции на старте, Catalog и Ordering заполняют БД тестовыми данными в окружении `Development`.

## Структура репозитория

```text
ShopMicroservices/
└── shop-microservices/
    ├── ApiGateways/
    │   └── YarpApiGateway/            # YARP reverse proxy + rate limiting
    ├── BuildingBlocks/
    │   ├── BuildingBlocks/            # CQRS-абстракции, behaviors, исключения, пагинация
    │   └── BuildingBlocks.Messaging/  # интеграционные события, настройка MassTransit
    ├── Services/
    │   ├── Catalog/Catalog.API/       # каталог товаров (Marten)
    │   ├── Basket/Basket.API/         # корзина (Marten + Redis, gRPC-клиент)
    │   ├── Discount/Discount.Grpc/    # скидки (gRPC + SQLite)
    │   └── Ordering/
    │       ├── Ordering.API/            # endpoints (Carter), health checks
    │       ├── Ordering.Application/    # команды, запросы, обработчики событий
    │       ├── Ordering.Domain/         # агрегаты, value objects, доменные события
    │       └── Ordering.Infrastructure/ # EF Core, миграции, interceptors, сидинг
    ├── compose.yaml
    └── shop-microservices.sln
```

## Быстрый старт (Docker)

### Требования

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET SDK](https://dotnet.microsoft.com/download) (нужен для генерации dev-сертификата)

### 1. Dev-сертификат для HTTPS

Контейнеры поднимают HTTPS-эндпоинт с сертификатом `${APPDATA}/ASP.NET/Https/cert.pfx` и паролем `mypassword` (см. `compose.yaml`). Без этого файла сервисы не стартуют.

**Windows (PowerShell):**

```powershell
dotnet dev-certs https -ep "$env:APPDATA\ASP.NET\Https\cert.pfx" -p mypassword
dotnet dev-certs https --trust
```

**Linux / macOS:** переменной `APPDATA` там нет, поэтому задайте её сами. Она должна быть выставлена и в том терминале, где запускается `docker compose`.

```bash
export APPDATA="$HOME/.appdata"
dotnet dev-certs https -ep "$APPDATA/ASP.NET/Https/cert.pfx" -p mypassword
```

### 2. Запуск

```bash
git clone https://github.com/KonstantinUsikov/ShopMicroservices.git
cd ShopMicroservices/shop-microservices
docker compose up -d --build
docker compose ps
```

### Адреса

| Компонент | Адрес | Примечание |
|---|---|---|
| **API Gateway** | http://localhost:6004 (https: 6064) | основная точка входа |
| Catalog.API | http://localhost:6000 (https: 6060) | |
| Basket.API | http://localhost:6001 (https: 6061) | |
| Discount.Grpc | localhost:6002 (https: 6062) | HTTP/2, gRPC |
| Ordering.API | http://localhost:6003 (https: 6063) | |
| RabbitMQ Management | http://localhost:15672 | `guest` / `guest` |
| PostgreSQL (Catalog) | localhost:15432 | `postgres` / `postgres` |
| PostgreSQL (Basket) | localhost:15433 | `postgres` / `postgres` |
| Redis | localhost:16379 | |
| SQL Server | localhost:11433 | `sa` / `SwN12345678` |

## Локальный запуск без Docker

Можно поднять в Docker только инфраструктуру, а сервисы запускать из IDE или через `dotnet run`. Понадобятся .NET 8 SDK и .NET 10 SDK (Basket.API собран под `net10.0`). Строки подключения в `appsettings.json` уже указывают на проброшенные порты localhost.

```bash
cd shop-microservices
docker compose up -d catalogdb basketdb distributedcache orderdb messagebroker

dotnet run --project Services/Catalog/Catalog.API --launch-profile https
dotnet run --project Services/Discount/Discount.Grpc --launch-profile https
dotnet run --project Services/Basket/Basket.API --launch-profile https
dotnet run --project Services/Ordering/Ordering.API --launch-profile https
```

| Сервис | HTTP | HTTPS |
|---|---|---|
| Catalog.API | 5000 | 5050 |
| Basket.API | 5001 | 5051 |
| Discount.Grpc | 5002 | 5052 |
| Ordering.API | 5003 | 5053 |
| YarpApiGateway | 5004 | 5054 |

## Примеры запросов

Все запросы идут через API Gateway. Примеры написаны для bash (Linux, macOS, Git Bash на Windows).

```bash
GATEWAY=http://localhost:6004
```

**Каталог:**

```bash
curl "$GATEWAY/catalog-service/products?pageNumber=1&pageSize=5"
curl "$GATEWAY/catalog-service/products/category/Smart%20Phone"
```

**Корзина.** Скидка подтягивается из Discount.Grpc: на `IPhone X` есть купон на 150, поэтому цена 950 в корзине превратится в 800.

```bash
curl -X POST "$GATEWAY/basket-service/basket" \
  -H "Content-Type: application/json" \
  -d '{
    "shoppingCart": {
      "username": "user1",
      "items": [
        {
          "productId": "5334c996-8457-4cf0-815c-ed2b77c4ff61",
          "productName": "IPhone X",
          "quantity": 1,
          "color": "Black",
          "price": 950
        }
      ]
    }
  }'

curl "$GATEWAY/basket-service/basket/user1"
```

**Оформление заказа.** Basket публикует событие в RabbitMQ, Ordering получает его и создаёт заказ асинхронно.

```bash
curl -X POST "$GATEWAY/basket-service/basket/checkout" \
  -H "Content-Type: application/json" \
  -d '{
    "shoppingCartCheckoutDto": {
      "userName": "user1",
      "customerId": "58c49479-ec65-4de2-86e7-033c546291aa",
      "firstName": "Ivan",
      "lastName": "Ivanov",
      "emailAddress": "ivan@gmail.com",
      "addressLine": "Lenina 1",
      "country": "Russia",
      "state": "Moscow",
      "zipCode": "10100",
      "cardName": "Ivan Ivanov",
      "cardNumber": "4111111111111111",
      "expiration": "12/28",
      "cvv": "123",
      "paymentMethod": 1
    }
  }'

curl "$GATEWAY/ordering-service/orders/user1"
curl "$GATEWAY/ordering-service/orders?pageIndex=0&pageSize=10"
```

**Rate limiting.** На маршрут Ordering действует лимит 5 запросов за 10 секунд, лишние запросы отклоняются со статусом `503`.

```bash
for i in $(seq 1 7); do
  curl -s -o /dev/null -w "%{http_code}\n" "$GATEWAY/ordering-service/orders"
done
```

**Health checks:**

```bash
curl "$GATEWAY/catalog-service/health"
curl "$GATEWAY/basket-service/health"
curl "$GATEWAY/ordering-service/health"
```

**gRPC** (нужен [grpcurl](https://github.com/fullstorydev/grpcurl), команда выполняется из папки `shop-microservices`):

```bash
grpcurl -plaintext \
  -import-path Services/Discount/Discount.Grpc/Protos -proto discount.proto \
  -d '{"productName": "IPhone X"}' \
  localhost:6002 discount.DiscountProtoService/GetDiscount
```

## Конфигурация

Настройки сервисов задаются в `appsettings.json` и переопределяются переменными окружения в `compose.yaml`.

| Параметр | Сервис | Назначение |
|---|---|---|
| `ConnectionStrings__Database` | все, кроме gateway | Строка подключения к БД сервиса |
| `ConnectionStrings__Redis` | Basket | Адрес Redis |
| `GrpcSettings__DiscountUrl` | Basket | Адрес Discount.Grpc |
| `MessageBroker__Host`, `__UserName`, `__Password` | Basket, Ordering | Подключение к RabbitMQ |
| `FeatureManagement__OrderFulfillment` | Ordering | Публиковать ли событие о созданном заказе (по умолчанию `false`) |
| `ReverseProxy` (секция) | Gateway | Маршруты и кластеры YARP |
