using System.IO;
using System.Text.Json;

namespace ArtesaniasPOS.Data.Database
{
    /// <summary>
    /// Ajustes de la aplicación que viven FUERA de la base de datos (en
    /// %APPDATA%\ArtesaniasPOS\settings.json), porque definen dónde está la
    /// base de datos y dónde se guardan los respaldos. No pueden depender de
    /// la propia base de datos.
    /// </summary>
    public static class AppSettings
    {
        private static readonly string CarpetaBase = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ArtesaniasPOS");

        private static readonly string ArchivoConfig = Path.Combine(CarpetaBase, "settings.json");

        public static string DbPath { get; private set; } = string.Empty;
        public static string CarpetaRespaldos { get; private set; } = string.Empty;
        public static DateTime? UltimoRespaldoAuto { get; private set; }

        static AppSettings()
        {
            Directory.CreateDirectory(CarpetaBase);
            Cargar();
        }

        private static void Cargar()
        {
            try
            {
                if (File.Exists(ArchivoConfig))
                {
                    var json = File.ReadAllText(ArchivoConfig);
                    var data = JsonSerializer.Deserialize<SettingsData>(json);
                    if (data != null)
                    {
                        DbPath = data.DbPath ?? string.Empty;
                        CarpetaRespaldos = data.CarpetaRespaldos ?? string.Empty;
                        UltimoRespaldoAuto = data.UltimoRespaldoAuto;
                    }
                }
            }
            catch { /* si el archivo está corrupto, se usan valores por defecto */ }

            if (string.IsNullOrWhiteSpace(DbPath))
                DbPath = Path.Combine(CarpetaBase, "ArtesaniasPOS.db");
            if (string.IsNullOrWhiteSpace(CarpetaRespaldos))
                CarpetaRespaldos = Path.Combine(CarpetaBase, "Respaldos");
        }

        public static void Guardar()
        {
            try
            {
                Directory.CreateDirectory(CarpetaBase);
                var json = JsonSerializer.Serialize(
                    new SettingsData
                    {
                        DbPath = DbPath,
                        CarpetaRespaldos = CarpetaRespaldos,
                        UltimoRespaldoAuto = UltimoRespaldoAuto
                    },
                    new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ArchivoConfig, json);
            }
            catch { /* no es crítico si no se puede guardar el ajuste */ }
        }

        public static void SetDbPath(string ruta)
        {
            DbPath = ruta;
            Guardar();
        }

        public static void SetCarpetaRespaldos(string carpeta)
        {
            CarpetaRespaldos = carpeta;
            Guardar();
        }

        public static void SetUltimoRespaldoAuto(DateTime fecha)
        {
            UltimoRespaldoAuto = fecha;
            Guardar();
        }

        private class SettingsData
        {
            public string? DbPath { get; set; }
            public string? CarpetaRespaldos { get; set; }
            public DateTime? UltimoRespaldoAuto { get; set; }
        }
    }
}
