using System.IO;
using ArtesaniasPOS.Core.Interfaces;
using ArtesaniasPOS.Data.Database;

namespace ArtesaniasPOS.Data.Services
{
    public class BackupService : IBackupService
    {
        public string RutaBaseDatos => AppSettings.DbPath;
        public string CarpetaRespaldos => AppSettings.CarpetaRespaldos;
        public DateTime? UltimoRespaldoAuto => AppSettings.UltimoRespaldoAuto;

        public string TamanoBaseDatos
        {
            get
            {
                if (!File.Exists(RutaBaseDatos)) return "—";
                var bytes = new FileInfo(RutaBaseDatos).Length;
                return bytes < 1024 * 1024
                    ? $"{bytes / 1024.0:N0} KB"
                    : $"{bytes / (1024.0 * 1024.0):N2} MB";
            }
        }

        public void RespaldoAutomatico()
        {
            try
            {
                if (!File.Exists(RutaBaseDatos)) return;

                Directory.CreateDirectory(CarpetaRespaldos);

                // 1) Respaldo "rodante": siempre el último estado, se reemplaza.
                var rodante = Path.Combine(CarpetaRespaldos, "ArtesaniasPOS_auto.db");
                File.Copy(RutaBaseDatos, rodante, overwrite: true);

                // 2) Snapshot del día: un archivo por fecha (se sobreescribe
                //    durante el día, así queda el último estado de cada día).
                var snapshotDia = Path.Combine(
                    CarpetaRespaldos, $"ArtesaniasPOS_{DateTime.Now:yyyy-MM-dd}.db");
                File.Copy(RutaBaseDatos, snapshotDia, overwrite: true);

                AppSettings.SetUltimoRespaldoAuto(DateTime.Now);
            }
            catch
            {
                // Un fallo de respaldo nunca debe interrumpir la operación del usuario.
            }
        }

        public ResultadoBackup CrearRespaldo(string destino)
        {
            try
            {
                if (!File.Exists(RutaBaseDatos))
                    return ResultadoBackup.Error("No se encontró la base de datos para respaldar.");

                File.Copy(RutaBaseDatos, destino, overwrite: true);
                return ResultadoBackup.Ok($"Copia de seguridad creada en:\n{destino}");
            }
            catch (Exception ex)
            {
                return ResultadoBackup.Error($"No se pudo crear la copia: {ex.Message}");
            }
        }

        public ResultadoBackup Restaurar(string origen)
        {
            try
            {
                if (!File.Exists(origen))
                    return ResultadoBackup.Error("El archivo seleccionado no existe.");

                if (File.Exists(RutaBaseDatos))
                    File.Copy(RutaBaseDatos, RutaBaseDatos + ".bak", overwrite: true);

                File.Copy(origen, RutaBaseDatos, overwrite: true);
                return ResultadoBackup.Ok(
                    "Base de datos restaurada. Reinicia la aplicación para ver los cambios.");
            }
            catch (Exception ex)
            {
                return ResultadoBackup.Error($"No se pudo restaurar: {ex.Message}");
            }
        }

        public ResultadoBackup CambiarUbicacionBaseDatos(string nuevaCarpeta)
        {
            try
            {
                Directory.CreateDirectory(nuevaCarpeta);
                var nuevaRuta = Path.Combine(nuevaCarpeta, "ArtesaniasPOS.db");

                if (string.Equals(nuevaRuta, RutaBaseDatos, StringComparison.OrdinalIgnoreCase))
                    return ResultadoBackup.Error("La base de datos ya está en esa ubicación.");

                if (File.Exists(RutaBaseDatos))
                    File.Copy(RutaBaseDatos, nuevaRuta, overwrite: true);

                AppSettings.SetDbPath(nuevaRuta);
                return ResultadoBackup.Ok(
                    $"La base de datos ahora está en:\n{nuevaRuta}\n\n" +
                    "Reinicia la aplicación para usar la nueva ubicación.");
            }
            catch (Exception ex)
            {
                return ResultadoBackup.Error($"No se pudo cambiar la ubicación: {ex.Message}");
            }
        }

        public void CambiarCarpetaRespaldos(string carpeta)
        {
            AppSettings.SetCarpetaRespaldos(carpeta);
        }
    }
}
