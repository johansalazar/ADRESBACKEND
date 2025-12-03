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
          Adapters/
          Program.cs
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
                    | Adaptadores de Persist. |
                    |  Archivo JSONL          |
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
