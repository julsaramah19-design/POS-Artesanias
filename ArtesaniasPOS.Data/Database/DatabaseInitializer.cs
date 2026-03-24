using Dapper;
using Microsoft.Data.Sqlite;

namespace ArtesaniasPOS.Data.Database
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Initialize()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            connection.Execute("PRAGMA foreign_keys = ON;");

            CrearTablas(connection);
            InsertarDatosIniciales(connection);
        }

        private void CrearTablas(SqliteConnection connection)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS Perfil (
                    Id      INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre  TEXT NOT NULL UNIQUE,
                    Activo  INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS Usuario (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    PerfilId        INTEGER NOT NULL,
                    Nombre          TEXT NOT NULL,
                    NombreUsuario   TEXT NOT NULL UNIQUE,
                    PasswordHash    TEXT NOT NULL,
                    Activo          INTEGER NOT NULL DEFAULT 1,
                    FechaCreacion   TEXT NOT NULL,
                    FOREIGN KEY (PerfilId) REFERENCES Perfil(Id)
                );

                CREATE TABLE IF NOT EXISTS Categoria (
                    Id      INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre  TEXT NOT NULL UNIQUE,
                    Activo  INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS Producto (
                    Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                    CategoriaId         INTEGER NOT NULL,
                    CodigoBarras        TEXT NOT NULL UNIQUE,
                    Nombre              TEXT NOT NULL,
                    Descripcion         TEXT,
                    PrecioBase          REAL NOT NULL,
                    StockActual         INTEGER NOT NULL DEFAULT 0,
                    StockMinimo         INTEGER NOT NULL DEFAULT 5,
                    Activo              INTEGER NOT NULL DEFAULT 1,
                    FechaCreacion       TEXT NOT NULL,
                    FechaActualizacion  TEXT NOT NULL,
                    FOREIGN KEY (CategoriaId) REFERENCES Categoria(Id)
                );

                CREATE TABLE IF NOT EXISTS Moneda (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    Codigo          TEXT NOT NULL UNIQUE,
                    Nombre          TEXT NOT NULL,
                    Simbolo         TEXT NOT NULL,
                    TasaCambio      REAL NOT NULL DEFAULT 1.0,
                    EsMonedaBase    INTEGER NOT NULL DEFAULT 0,
                    Activo          INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS MedioPago (
                    Id      INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre  TEXT NOT NULL UNIQUE,
                    Activo  INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS Venta (
                    Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                    UsuarioId           INTEGER NOT NULL,
                    MedioPagoId         INTEGER NOT NULL,
                    MonedaId            INTEGER NOT NULL,
                    TasaCambioUsada     REAL NOT NULL,
                    Subtotal            REAL NOT NULL,
                    Descuento           REAL NOT NULL DEFAULT 0,
                    Total               REAL NOT NULL,
                    TotalMonedaVenta    REAL NOT NULL,
                    MontoPagado         REAL,
                    Cambio              REAL,
                    Estado              TEXT NOT NULL DEFAULT 'Completada',
                    Observaciones       TEXT,
                    FechaVenta          TEXT NOT NULL,
                    FOREIGN KEY (UsuarioId)   REFERENCES Usuario(Id),
                    FOREIGN KEY (MedioPagoId) REFERENCES MedioPago(Id),
                    FOREIGN KEY (MonedaId)    REFERENCES Moneda(Id)
                );

                CREATE TABLE IF NOT EXISTS DetalleVenta (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    VentaId         INTEGER NOT NULL,
                    ProductoId      INTEGER NOT NULL,
                    Cantidad        INTEGER NOT NULL,
                    PrecioUnitario  REAL NOT NULL,
                    Subtotal        REAL NOT NULL,
                    FOREIGN KEY (VentaId)    REFERENCES Venta(Id),
                    FOREIGN KEY (ProductoId) REFERENCES Producto(Id)
                );

                CREATE TABLE IF NOT EXISTS MovimientoInventario (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProductoId      INTEGER NOT NULL,
                    UsuarioId       INTEGER NOT NULL,
                    TipoMovimiento  TEXT NOT NULL,
                    Cantidad        INTEGER NOT NULL,
                    StockAnterior   INTEGER NOT NULL,
                    StockResultante INTEGER NOT NULL,
                    Referencia      TEXT,
                    Motivo          TEXT,
                    Fecha           TEXT NOT NULL,
                    FOREIGN KEY (ProductoId) REFERENCES Producto(Id),
                    FOREIGN KEY (UsuarioId)  REFERENCES Usuario(Id)
                );

                CREATE TABLE IF NOT EXISTS Licencia (
                    Id                  INTEGER PRIMARY KEY,
                    NombreCliente       TEXT NOT NULL,
                    HardwareId          TEXT NOT NULL,
                    FechaEmision        TEXT NOT NULL,
                    FechaVencimiento    TEXT,
                    Activa              INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS ConfiguracionNegocio (
                    Clave       TEXT PRIMARY KEY,
                    Valor       TEXT NOT NULL,
                    Descripcion TEXT
                );

                CREATE INDEX IF NOT EXISTS IX_Producto_CodigoBarras 
                    ON Producto(CodigoBarras);
                CREATE INDEX IF NOT EXISTS IX_Venta_FechaVenta 
                    ON Venta(FechaVenta);
                CREATE INDEX IF NOT EXISTS IX_DetalleVenta_VentaId 
                    ON DetalleVenta(VentaId);
                CREATE INDEX IF NOT EXISTS IX_Movimiento_ProductoId 
                    ON MovimientoInventario(ProductoId);
            ";

            connection.Execute(sql);
        }

        private void InsertarDatosIniciales(SqliteConnection connection)
        {
            var yaExisteDatos = connection.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM Perfil") > 0;

            if (yaExisteDatos) return;

            connection.Execute(@"
                INSERT INTO Perfil (Nombre) VALUES ('Admin'), ('Vendedor');

                INSERT INTO Moneda (Codigo, Nombre, Simbolo, TasaCambio, EsMonedaBase)
                VALUES
                    ('COP', 'Peso Colombiano', '$',   1.0,    1),
                    ('USD', 'Dólar',           'US$', 4100.0, 0),
                    ('EUR', 'Euro',            '€',   4400.0, 0);

                INSERT INTO MedioPago (Nombre)
                VALUES
                    ('Efectivo'),
                    ('Tarjeta Débito'),
                    ('Tarjeta Crédito'),
                    ('Nequi'),
                    ('Daviplata');
                INSERT INTO Categoria (Nombre) VALUES ('General'), ('Artesanías'), ('Accesorios');

                INSERT INTO ConfiguracionNegocio (Clave, Valor, Descripcion)
                VALUES
                    ('NombreNegocio',   '',  'Nombre del negocio'),
                    ('NIT',             '',  'NIT o cédula'),
                    ('Telefono',        '',  'Teléfono de contacto'),
                    ('Direccion',       '',  'Dirección del negocio'),
                    ('Email',           '',  'Email de contacto'),
                    ('LogoPath',        '',  'Ruta del logo'),
                    ('ColorPrimario',   '#3B82F6', 'Color principal de la app'),
                    ('ColorSecundario', '#1E40AF', 'Color secundario de la app'),
                    ('TipoNegocio',     '',  'Tipo de negocio'),
                    ('WizardCompletado','0', 'Si ya se completó la configuración inicial');

                INSERT OR IGNORE INTO ConfiguracionNegocio (Clave, Valor) 
                VALUES ('MontoMinimoDescuento', '60000');
            ");

            var hash = BCrypt.Net.BCrypt.HashPassword("admin123");
            connection.Execute(@"
                INSERT INTO Usuario (PerfilId, Nombre, NombreUsuario, PasswordHash, FechaCreacion)
                VALUES (1, 'Administrador', 'admin', @Hash, @Fecha)",
                new
                {
                    Hash = hash,
                    Fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                }
            );
        }
    }
}