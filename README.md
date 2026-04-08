# 🛍️ Artesanías POS

Sistema de Punto de Venta (POS) diseñado para tiendas de artesanías, desarrollado en **WPF (.NET)** con arquitectura **MVVM**.

## Descripción

Artesanías POS es una aplicación de escritorio que permite gestionar ventas, productos, inventario y reportes de un negocio de artesanías. Cuenta con soporte multimoneda, descuentos por producto, generación de recibos tipo ticket y un sistema de perfiles con permisos por rol.

## Arquitectura

El proyecto sigue una arquitectura de **3 capas** separadas en proyectos independientes:

```
Solución ArtesaniasPOS (3 proyectos)
│
├── ArtesaniasPOS          → UI (WPF)
│   ├── Views/
│   │   ├── Wizard/              Asistente de configuración inicial
│   │   ├── Ventas/              Punto de venta + recibos
│   │   ├── Productos/           Gestión de productos
│   │   ├── Reportes/            Dashboard y reportes
│   │   ├── Configuracion/       Ajustes del negocio
│   │   ├── ShellView.xaml       Layout principal con sidebar
│   │   └── LoginView.xaml       Inicio de sesión
│   ├── App.xaml
│   └── MainWindow.xaml
│
├── ArtesaniasPOS.Core     → Lógica de negocio
│   ├── ViewModels/              ViewModels por módulo
│   ├── Interfaces/              Contratos de servicios + DTOs
│   └── Models/                  Modelos de dominio
│
└── ArtesaniasPOS.Data     → Acceso a datos
    └── Services/                Implementaciones con Dapper + SQLite
```

## Tecnologías

| Componente | Tecnología |
|---|---|
| **Framework UI** | WPF (.NET) |
| **Patrón** | MVVM (sin frameworks externos) |
| **Base de datos** | SQLite |
| **ORM** | Dapper (micro-ORM) |
| **Lenguaje** | C# |

## Módulos

### 💰 Punto de Venta
- Búsqueda de productos por código de barras o nombre
- Carrito de compras con gestión de cantidades
- Descuento por producto (solo para artículos ≥ $60.000 por defecto, configurable)
- Soporte multimoneda con conversión automática por tasa de cambio
- Múltiples medios de pago
- Cálculo automático de cambio
- Generación de recibo tipo ticket (80mm) al confirmar venta

### 📦 Productos
- CRUD completo de productos
- Código de barras, nombre, precio base, stock
- Control de inventario con movimientos automáticos al vender

### 📊 Reportes
- **KPIs del dashboard:** Total ventas, ingresos, productos vendidos, ticket promedio
- **Historial de ventas:** Detalle completo con vendedor, medio de pago, descuentos
- **Productos más vendidos:** Ranking por cantidad y total generado
- **Ventas por vendedor:** Rendimiento individual
- Filtros por rango de fechas (hoy, 7 días, 30 días, personalizado)
- Vista detallada de cada venta con desglose de productos y descuentos

### ⚙️ Configuración
- Nombre del negocio
- Colores del tema (primario y secundario)
- Monto mínimo para descuentos
- Gestión de monedas y tasas de cambio
- Gestión de medios de pago
- Gestión de usuarios y perfiles

### 👤 Usuarios y Perfiles
- **Administrador (Perfil 1):** Acceso completo a todos los módulos
- **Vendedor (Perfil 2):** Acceso a Inicio, Productos y Ventas
- Inicio de sesión con autenticación

## Características Técnicas

### Patrón MVVM
- `ViewModelBase` con implementación de `INotifyPropertyChanged` y método `SetProperty`
- `RelayCommand` y `AsyncRelayCommand` para comandos sincrónicos y asincrónicos
- Navegación por `ContentControl` + `DataTemplate` en el Shell

### Sistema de Descuentos
- Descuento por producto individual en el carrito
- Solo productos con precio ≥ monto mínimo configurable aplican descuento
- El descuento no puede exceder el subtotal del producto
- Validación visual: campo habilitado (naranja) o deshabilitado (gris) según elegibilidad

### Soporte Multimoneda
- Moneda base (COP) y monedas adicionales con tasa de cambio
- Conversión automática del total según moneda seleccionada
- Cambio calculado en la moneda de pago

### Recibo de Venta
- Formato tipo ticket (80mm) con vista previa
- Impresión mediante `PrintDialog` de Windows
- Se genera automáticamente al confirmar una venta
- Incluye: datos del negocio, vendedor, detalle de productos con descuentos, totales, medio de pago y cambio

### Configuración Dinámica
- Tabla `ConfiguracionNegocio` con pares clave-valor
- Wizard de configuración inicial en el primer arranque
- Colores del sidebar personalizables en tiempo real

## Base de Datos

SQLite con las siguientes tablas principales:

- `Usuario` — Usuarios del sistema
- `Perfil` — Roles (Admin, Vendedor)
- `Producto` — Catálogo de productos
- `Venta` — Encabezado de ventas
- `DetalleVenta` — Líneas de cada venta
- `Moneda` — Monedas disponibles con tasa de cambio
- `MedioPago` — Medios de pago activos
- `ConfiguracionNegocio` — Configuración del negocio (clave-valor)

## Requisitos

- Windows 10/11
- .NET 8.0 (o la versión del SDK utilizada)
- No requiere instalación de base de datos externa (SQLite embebido)

## Ejecución

```bash
# Clonar el repositorio
git clone https://github.com/tu-usuario/ArtesaniasPOS.git

# Restaurar paquetes
dotnet restore

# Ejecutar
dotnet run --project ArtesaniasPOS
```

## Paquetes NuGet

| Paquete | Proyecto | Uso |
|---|---|---|
| `Microsoft.Data.Sqlite` | Data | Conexión a SQLite |
| `Dapper` | Data | Micro-ORM para consultas SQL |

## Converters Disponibles (XAML)

| Converter | Función |
|---|---|
| `BoolToVisibilityConverter` | `true` → Visible, `false` → Collapsed |
| `StringToVisibilityConverter` | String no vacío → Visible |
| `StringToVisibilityInvertedConverter` | String vacío → Visible |
| `DoubleToMoneyConverter` | Formato de moneda para TextBox |

## Licencia

Este proyecto es de uso privado.
