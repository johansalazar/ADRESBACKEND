# Proyecto Adquisiciones -- Arquitectura Hexagonal (.NET 8 + Angular)

Este proyecto implementa una **arquitectura hexagonal** tanto en
**backend .NET 8** como en **frontend Angular 17**, usando un **archivo
plano (JSONL)** como almacenamiento y con **Swagger** para documentación
del API.

------------------------------------------------------------------------

# 🚀 Tecnologías

### Backend (.NET 8)

-   ASP.NET Core 8
-   Controladores (MVC)
-   Swagger/OpenAPI
-   Inyección de dependencias
-   Arquitectura Hexagonal
-   Persistencia en SQL Server (vía Entity Framework Core)
-   Migraciones de Entity Framework Core
-   Persistencia en archivo JSONL
-   Repositorios basados en puertos/adaptadores

### Frontend (Angular 17)

-   Angular Standalone Components
-   Servicios basados en Hexagonal Ports
-   Proxy para CORS
-   CRUD completo

------------------------------------------------------------------------

# 📁 Estructura del Proyecto

    adquisiciones-app/
  backend/
    Adq.Backend.Api/
      Controllers/
      Application/
      Domain/
      Ports/
    Adq.Backend.Infrastructure/  
      Persistence/
        EFCore/
          Migrations/           
          SqlServerAcquisitionRepository.cs
          AcquisitionDbContext.cs
    data/
      acquisitions.jsonl
      frontend/
        src/
          app/
            core/
              ports/
              services/
            pages/
            shared/
            app.routes.ts

------------------------------------------------------------------------

# 🧩 Arquitectura Hexagonal

## 🟦 Diagrama General

             +---------------------------+
             |        FRONTEND          |
             |   Angular (Puertos)      |
             +-----------+---------------+
                         |
                         | HTTP REST
                         v
            +------------+--------------+
            |         API (.NET 8)      |
            | Controllers (Entradas)    |
            +------------+--------------+
                         |
                  Aplicación (Casos de Uso)
                         |
             +-----------+------------+
             |        Dominio         |
             |  Entidades + Reglas    |
             +-----------+------------+
                         |
            +------------+------------+
            | Adaptador de Persistencia|
            |  **EF Core/SQL Server** |
            +--------------------------+

------------------------------------------------------------------------

# 📌 UML

## 1️⃣ Diagrama de Caso de Uso

               +---------------------+
               |     Usuario         |
               +----------+----------+
                          |
                          v
            +-----------------------------+
            |   Gestionar Adquisiciones   |
            +-----------------------------+
             /        |        \ 
            v         v         v
      Registrar   Consultar   Actualizar/Eliminar

------------------------------------------------------------------------

## 2️⃣ Diagrama de Componentes

    +---------------------------+
    |      API .NET 8          |
    |---------------------------|
    | Controllers               |
    | Application Services      |
    | Domain Models             |
    | Ports (Interfaces)        |
    +-------------+-------------+
                  |
                  v
    +-------------+-------------+
    | Adaptador Archivo JSONL   |
    | (Infraestructura)         |
    +---------------------------+
    +-------------+-------------+
    | Adaptador EF Core/SQL S.  |
    | (Infraestructura)         |
    | **AcquisitionDbContext** |
    | **Migrations** |
    +---------------------------+

------------------------------------------------------------------------

## 3️⃣ Diagrama de Secuencia -- Crear Adquisición

    Usuario → Frontend → Backend Controller → Application Service → Repository → Archivo JSONL

Secuencia:

    Usuario
      │  (POST /acquisition)
      ▼
    Frontend Angular
      │ envia DTO
      ▼
    AcquisitionController
      │ valida y delega
      ▼
    AcquisitionService
      │ crea objeto dominio
      ▼
    FileAcquisitionRepository
      │ guarda en JSONL
      ▼
    Archivo del sistema

------------------------------------------------------------------------

# ▶️ Cómo ejecutar el proyecto

## Backend

    cd backend/Adq.Backend.Api
    dotnet restore
    dotnet run
Backend (SQL Server/EF Core)
Configuración de Conexión: Asegúrese de que el archivo appsettings.json en Adq.Backend.Api contiene la cadena de conexión a su instancia de SQL Server.

JSON

{
  "ConnectionStrings": {
    "AcquisitionsDb": "Server=(localdb)\\mssqllocaldb;Database=ADRES;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
  // ... otros ajustes
}
Ejecutar Migraciones: Para crear o actualizar la estructura de la base de datos SQL Server, ejecute los comandos de EF Core.

Añadir Migración Inicial: (Solo la primera vez, si el proyecto es nuevo)

Bash

dotnet ef migrations add InitialCreate --project ../Adq.Backend.Infrastructure
Aplicar Migraciones: (Para actualizar la base de datos)

Bash

cd backend/Adq.Backend.Api
dotnet ef database update --project ../Adq.Backend.Infrastructure

Swagger:

    http://localhost:5000/swagger

## Frontend

    cd frontend
    npm install
    npx ng serve --proxy-config proxy.conf.json

App en:

    http://localhost:4200

------------------------------------------------------------------------

# 📦 Estructura de almacenamiento (JSONL)

Cada línea del archivo `acquisitions.jsonl` contiene una adquisición
serializada en JSON.

Ejemplo:

    {"id":"guid","budget":1000,"unit":"UND","quantity":5,...}

------------------------------------------------------------------------

# ✨ Autor

Generado automáticamente por Johan Ivan Salazar Santana.
