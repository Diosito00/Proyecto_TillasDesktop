# 👟 Tillas Desktop

![Cátedra](https://img.shields.io/badge/C%C3%A1tedra-Taller_de_Programaci%C3%B3n_II-555555?style=flat-square)
![Lenguaje](https://img.shields.io/badge/C%23-.NET_10-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![Interfaz](https://img.shields.io/badge/UI-WPF_MVVM-0EA5E9?style=flat-square)
![Motor](https://img.shields.io/badge/SQL_Server-T--SQL-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white)
![Arquitectura](https://img.shields.io/badge/Arquitectura-3_capas-555555?style=flat-square)

**Tillas Desktop** es un sistema de gestión y ventas para una tienda de zapatillas. Es una aplicación de escritorio desarrollada en **C# y .NET (WPF)** que permite administrar el inventario por talle, registrar ventas con ticket en PDF, gestionar clientes y usuarios, consultar reportes y respaldar la base de datos, con permisos según el rol de cada usuario.

<p align="center">
  <img alt="Login" src="https://img.shields.io/badge/M%C3%93DULO-LOGIN_Y_ROLES-3B82F6?style=for-the-badge">
  <img alt="Inventario" src="https://img.shields.io/badge/M%C3%93DULO-INVENTARIO-8B5CF6?style=for-the-badge">
  <img alt="Ventas" src="https://img.shields.io/badge/M%C3%93DULO-PUNTO_DE_VENTA-22C55E?style=for-the-badge">
  <img alt="Reportes" src="https://img.shields.io/badge/M%C3%93DULO-REPORTES-F59E0B?style=for-the-badge">
</p>

| Módulo | Descripción |
| :--- | :--- |
| Login y roles | Autenticación, recuperación de contraseña y permisos según el rol. |
| Inventario | Modelos, marcas, categorías y stock por talle. |
| Punto de venta | Carrito, reserva temporal de stock, pago y ticket en PDF. |
| Clientes | Alta, edición y baja de clientes. |
| Reportes | Gráficos y reportes en PDF de las ventas. |
| Usuarios y respaldos | Administración de usuarios y copias de seguridad de la base. |

---

## ![01](https://img.shields.io/badge/01-555555?style=flat-square)![PRESENTACI%C3%93N](https://img.shields.io/badge/PRESENTACI%C3%93N_Y_CONTEXTO-3B82F6?style=flat-square) Presentación y contexto

**Tillas Desktop** centraliza las operaciones diarias de una zapatería en una sola aplicación: el personal inicia sesión, gestiona el catálogo y el stock por talle, y registra las ventas, que quedan almacenadas en una base de datos relacional en **SQL Server**.

---

## ![02](https://img.shields.io/badge/02-555555?style=flat-square)![CARACTER%C3%8DSTICAS](https://img.shields.io/badge/CARACTER%C3%8DSTICAS-22C55E?style=flat-square) Características principales

**Gestión de usuarios y roles**

- Sistema de autenticación (login).
- Roles con distintos permisos: **Admin**, **Gerente** y **Vendedor**.
- La restauración de un respaldo requiere la autorización de otro Gerente o Administrador.

**Gestión de inventario**

- Administración de modelos, marcas y categorías.
- Stock por talle, con ingreso de stock y detalle por modelo.

**Ventas y facturación**

- Carrito de compras con selección de cliente y medio de pago.
- Reserva temporal de stock durante 10 minutos, para evitar que dos cajas vendan la misma unidad.
- Ticket de venta en PDF (formato 80 mm).
- La venta se registra en una única transacción: pago, venta, detalle, descuento de stock y liberación de la reserva.

**Reportes y respaldos**

- Gráficos (LiveCharts) y reportes en PDF (QuestPDF).
- Generación y restauración de copias de seguridad de la base desde la aplicación.

**Persistencia de datos**

- Base de datos relacional en SQL Server.
- Consultas parametrizadas con ADO.NET y un procedimiento almacenado, `sp_LimpiarReservasVencidas`, que libera las reservas vencidas.

---

## ![03](https://img.shields.io/badge/03-555555?style=flat-square)![ROLES](https://img.shields.io/badge/ROLES_Y_PERMISOS-F59E0B?style=flat-square) Roles y permisos

El menú lateral muestra u oculta módulos según el rol:

| Rol | Módulos visibles |
| :--- | :--- |
| `Admin` | Inicio, Inventario, Clientes, Reportes, Configuración (usuarios) y Respaldos. No ve el Punto de venta. |
| `Gerente` | Inicio, Inventario y Reportes. |
| `Vendedor` | Inicio, Punto de venta y Clientes. |

---

## ![04](https://img.shields.io/badge/04-555555?style=flat-square)![TECNOLOG%C3%8DAS](https://img.shields.io/badge/TECNOLOG%C3%8DAS-8B5CF6?style=flat-square) Tecnologías utilizadas

| Componente | Tecnología |
| :--- | :--- |
| Lenguaje principal | C# |
| Framework | .NET 10 (`net10.0-windows`), WPF con patrón MVVM |
| Acceso a datos | ADO.NET con `Microsoft.Data.SqlClient` |
| Gráficos | LiveCharts.Wpf |
| Reportes y tickets en PDF | QuestPDF |
| Lenguaje de base de datos | T-SQL |
| Motor de base de datos | SQL Server / SQL Server Express |
| Gestor de base de datos | SQL Server Management Studio (SSMS) |
| Entorno de desarrollo | Visual Studio |

---

## ![05](https://img.shields.io/badge/05-555555?style=flat-square)![ESTRUCTURA](https://img.shields.io/badge/ESTRUCTURA_DEL_REPOSITORIO-6B7280?style=flat-square) Estructura del repositorio

La solución sigue una arquitectura de tres capas más un proyecto compartido de entidades (`UI → BLL → DAL → SQL Server`):

```text
Proyecto_TillasDesktop/
│
├── Proyecto_TillasDesktop/     # Interfaz de usuario: vistas XAML y ViewModels (WPF)
├── TillasDesktop.BLL/          # Lógica de negocio: servicios y validaciones
├── TillasDesktop.DAL/          # Acceso a datos: repositorios con ADO.NET
├── TillasDesktop.Entities/     # Entidades y DTOs compartidos entre capas
├── Proyecto_TillasDesktop.slnx # Solución de Visual Studio
├── .gitattributes
├── .gitignore
└── README.md
```

---

## ![06](https://img.shields.io/badge/06-555555?style=flat-square)![EJECUCI%C3%93N](https://img.shields.io/badge/EJECUCI%C3%93N-14B8A6?style=flat-square) Cómo ejecutar el proyecto

**Requisitos**

- Windows 10 u 11.
- Visual Studio con la carga de trabajo de escritorio de .NET y el SDK de .NET 10.
- SQL Server o SQL Server Express (instancia `SQLEXPRESS`) y SQL Server Management Studio (SSMS).

**Pasos**

| Orden | Acción | Nota |
| :---: | :--- | :--- |
| 1 | Clonar el repositorio. | `git clone https://github.com/Diosito00/Proyecto_TillasDesktop.git` |
| 2 | Crear la base de datos `TillasDB` en SQL Server. | Ejecutar en SSMS el script de creación de la base (tablas, restricciones y `sp_LimpiarReservasVencidas`). |
| 3 | Revisar la cadena de conexión. | En `Proyecto_TillasDesktop/App.config`, clave `ConexionTillas`. Por defecto: `Server=.\SQLEXPRESS;Database=TillasDB;Integrated Security=True;TrustServerCertificate=True;` |
| 4 | Abrir `Proyecto_TillasDesktop.slnx` en Visual Studio y ejecutar. | Usar `TillasDesktop.UI` (carpeta `Proyecto_TillasDesktop`) como proyecto de inicio. |

**Limpieza automática de reservas (opcional)**

SQL Server Express no incluye SQL Server Agent. Para liberar las reservas vencidas cada 10 minutos, programá una tarea en el Programador de tareas de Windows que ejecute:

```text
sqlcmd -S .\SQLEXPRESS -d TillasDB -E -Q "EXEC sp_LimpiarReservasVencidas"
```
---

## ![07](https://img.shields.io/badge/07-555555?style=flat-square)![DESARROLLADORES](https://img.shields.io/badge/DESARROLLADORES-EC4899?style=flat-square) Desarrolladores

- [Aguirre Mauricio Alejandro](https://github.com/Diosito00). 
- [Micaela Nataly Pawlizki](https://github.com/nattOnmars).
