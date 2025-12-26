# 🏥 MedCitas - Sistema de Gestión de Citas Médicas

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-14+-316192?logo=postgresql)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Sistema web para la gestión de citas médicas desarrollado con **ASP.NET Core 9** (Razor Pages + MVC), **Entity Framework Core** y **PostgreSQL**.
**Esta es una migracion del repositorio original. El historial de commits puedes verlo original, en mi rama pablodev:** https://github.com/CarlosJ18G/MedCitas/tree/pablodev 

## 📋 Tabla de Contenidos

- [Características](#-características)
- [Arquitectura](#-arquitectura)
- [Tecnologías](#-tecnologías)
- [Requisitos Previos](#-requisitos-previos)
- [Instalación](#-instalación)
- [Configuración](#-configuración)
- [Ejecución](#-ejecución)
- [Testing](#-testing)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Seguridad](#-seguridad)
- [Contribuir](#-contribuir)
- [Equipo](#-equipo)

---

## ✨ Características

- ✅ **Registro de Pacientes** con validación de datos
- ✅ **Sistema de Autenticación** con contraseñas hasheadas (BCrypt)
- ✅ **Verificación por OTP** (One-Time Password) vía email
- ✅ **Gestión de Sesiones** segura
- ✅ **Arquitectura en Capas** (Clean Architecture)
- ✅ **Pruebas Unitarias** con cobertura >80%
- ✅ **Análisis de Código** con SonarQube
- ✅ **User Secrets** para manejo seguro de credenciales

---

## 🏗️ Arquitectura

El proyecto sigue los principios de **Clean Architecture** con separación de responsabilidades:

```
┌─────────────────────────────────────────┐
│         MedCitas.Web (UI Layer)         │
│   Razor Pages + MVC Controllers         │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│      MedCitas.Core (Domain Layer)       │
│   Entities, Services, Interfaces        │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│  MedCitas.Infrastructure (Data Layer)   │
│   Repositories, DbContext, Migrations   │
└─────────────────────────────────────────┘
```

### Capas del Proyecto

| Capa | Responsabilidad | Dependencias |
|------|----------------|--------------|
| **MedCitas.Core** | Lógica de negocio, entidades, interfaces | Ninguna |
| **MedCitas.Infrastructure** | Acceso a datos, servicios externos | Core |
| **MedCitas.Web** | Presentación, controladores, Razor Pages | Core, Infrastructure |
| **MedCitas.Tests** | Pruebas unitarias e integración | Todos |

---

## 🚀 Tecnologías

### Backend
- **.NET 9** - Framework principal
- **C# 13** - Lenguaje de programación
- **ASP.NET Core MVC + Razor Pages** - UI Framework
- **Entity Framework Core 9** - ORM
- **PostgreSQL 14+** - Base de datos
- **Npgsql** - Provider para PostgreSQL

### Seguridad
- **BCrypt.Net** - Hash de contraseñas
- **User Secrets** - Gestión de credenciales en desarrollo
- **ASP.NET Core Identity** - Sesiones y autenticación

### Testing & Quality
- **xUnit** - Framework de pruebas
- **Moq** - Mocking framework
- **FluentAssertions** - Aserciones expresivas
- **Coverlet** - Cobertura de código
- **SonarQube** - Análisis estático de código

---

## 📦 Requisitos Previos

Antes de comenzar, asegúrate de tener instalado:

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (versión 9.0.0 o superior)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### Verificar instalación

```bash
dotnet --version  # Debe mostrar 9.0.x
psql --version    # Debe mostrar PostgreSQL 14.x o superior
```

---

## 🛠️ Instalación

### 1. Clonar el repositorio

```bash
git clone https://github.com/CarlosJ18G/MedCitas.git
cd MedCitas
```

### 2. Restaurar dependencias

```bash
dotnet restore
```

### 3. Crear la base de datos

Conéctate a PostgreSQL y ejecuta:

```sql
CREATE DATABASE medcitas_database;
```

---

## ⚙️ Configuración

### 1. Configurar User Secrets (Desarrollo)

Para evitar exponer credenciales, usa **User Secrets**:

```bash
# Inicializar User Secrets
dotnet user-secrets init --project MedCitas.Web

# Configurar contraseña de BD
dotnet user-secrets set "ConnectionStrings:DbPassword" "tu_contraseña_postgres" --project MedCitas.Web
```

### 2. Verificar `appsettings.json`

El archivo `MedCitas.Web/appsettings.json` debe verse así:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=medcitas_database;Username=postgres"
  }
}
```

### 3. Aplicar Migraciones

```bash
cd MedCitas.Infrastructure
dotnet ef database update --startup-project ../MedCitas.Web
```

Verifica que las tablas se hayan creado:

```sql
\dt  -- En psql, lista las tablas
```

---

## ▶️ Ejecución

### Modo Desarrollo

```bash
cd MedCitas.Web
dotnet run
```

La aplicación estará disponible en:
- **HTTPS:** https://localhost:7001
- **HTTP:** http://localhost:5000

---

## 🧪 Testing

### Ejecutar todas las pruebas

```bash
dotnet test
```

### Ejecutar con cobertura de código

```powershell
# Usando el script PowerShell
./run-tests-coverage.ps1

# O manualmente
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Ejecutar análisis de SonarQube

```powershell
./run-sonarqube-analysis.ps1
```

### Estructura de Tests

```
MedCitas.Tests/
├── Controllers/          # Tests de controladores MVC
│   ├── HomeControllerTests.cs
│   └── PacienteControllerTests.cs
├── Pages/               # Tests de Razor Pages
│   └── VerificarOtpModelTests.cs
├── Services/            # Tests de servicios
│   ├── OtpServiceTests.cs
│   ├── PacienteServiceTests.cs
│   └── FakeEmailServiceTests.cs
├── Repositories/        # Tests de repositorios
│   └── EfPacienteRepositorioTests.cs
└── Entities/           # Tests de entidades
    └── PacienteTests.cs
```

**Cobertura actual:** ~85%

---

## 📁 Estructura del Proyecto

```
MedCitas/
│
├── MedCitas.Core/                    # 🎯 Capa de Dominio
│   ├── Entities/
│   │   └── Paciente.cs              # Entidad principal
│   ├── Interfaces/
│   │   ├── IPacienteRepository.cs
│   │   └── IEmailService.cs
│   └── Services/
│       ├── PacienteService.cs       # Lógica de negocio
│       └── OtpService.cs            # Generación de OTP
│
├── MedCitas.Infrastructure/          # 🗄️ Capa de Datos
│   ├── DataDb/
│   │   └── MedCitasDbContext.cs
│   ├── Migrations/                   # Migraciones EF Core
│   ├── Repositories/
│   │   └── EfPacienteRepositorio.cs
│   └── Services/
│       └── FakeEmailService.cs
│
├── MedCitas.Web/                     # 🌐 Capa de Presentación
│   ├── Controllers/
│   │   ├── HomeController.cs
│   │   └── PacienteController.cs
│   ├── Pages/
│   │   └── Cuenta/
│   │       └── VerificarOTP.cshtml
│   ├── Views/
│   │   ├── Home/
│   │   ├── Paciente/
│   │   └── Shared/
│   ├── wwwroot/                      # Archivos estáticos
│   ├── appsettings.json
│   └── Program.cs
│
├── MedCitas.Tests/                   # 🧪 Capa de Pruebas
│   ├── Controllers/
│   ├── Pages/
│   ├── Services/
│   ├── Repositories/
│   └── Entities/
│
├── .gitignore
├── MedCitas.sln
└── README.md
```

---

## 🔒 Seguridad

### Manejo de Credenciales

| Entorno | Método |
|---------|--------|
| **Desarrollo** | User Secrets (`dotnet user-secrets`) |
| **Staging/CI** | Variables de Entorno |
| **Producción** | Azure Key Vault / AWS Secrets Manager |

### Características de Seguridad

- ✅ Contraseñas hasheadas con **BCrypt** (factor de trabajo: 12)
- ✅ Validación de OTP con expiración (15 minutos)
- ✅ Límite de intentos fallidos (3 intentos)
- ✅ Sesiones HttpOnly y SameSite
- ✅ Protección HTTPS en producción
- ✅ Validación de entrada con Data Annotations
- ✅ Prevención de ReDoS con timeouts en regex

### .gitignore

Asegúrate de que estos archivos/carpetas estén excluidos:

```
# User Secrets
**/appsettings.*.json
!**/appsettings.json
secrets.json

# Build results
bin/
obj/
.vs/

# SonarQube
.sonarqube/
```

---

## 🤝 Contribuir

### Flujo de Trabajo

1. **Fork** el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. **Escribe tests** para tu código
4. Asegúrate de que todos los tests pasen (`dotnet test`)
5. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
6. Push a la rama (`git push origin feature/AmazingFeature`)
7. Abre un **Pull Request**

### Estándares de Código

- Sigue las convenciones de C# ([Microsoft Guidelines](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions))
- Escribe tests unitarios para nueva funcionalidad
- Mantén cobertura de código >80%
- Documenta código complejo con comentarios XML

---

## 👥 Equipo

Desarrollado por estudiantes de **Ingeniería Informatica** del curso **Calidad de software** - Universidad IUE

- **Juan Pablo Ríos Ortiz** - [@elrios893](https://github.com/elrios893)
- **Carlos Jiménez** - [@CarlosJ18G](https://github.com/CarlosJ18G)

---

---

## 🎯 Roadmap

- [x] Implementar sistema de roles (Paciente, Doctor, Admin)
- [x] Módulo de agendamiento de citas
- [x] Historial médico de pacientes
- [ ] Notificaciones push
- [x] Dashboard de administración
- [ ] API REST para integración móvil

---

<div align="center">

**⭐ Si este proyecto te fue útil, considera darle una estrella en GitHub ⭐**

</div>

