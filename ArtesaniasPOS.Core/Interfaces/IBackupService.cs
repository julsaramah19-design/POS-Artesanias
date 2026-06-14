namespace ArtesaniasPOS.Core.Interfaces
{
    /// <summary>
    /// Gestiona la ubicación de la base de datos y sus copias de seguridad
    /// (manuales y automáticas).
    /// </summary>
    public interface IBackupService
    {
        string RutaBaseDatos { get; }
        string CarpetaRespaldos { get; }
        string TamanoBaseDatos { get; }
        DateTime? UltimoRespaldoAuto { get; }

        /// <summary>
        /// Respaldo automático disparado por acciones clave (tras una venta,
        /// al cerrar la app). Reemplaza un archivo "rodante" y mantiene un
        /// snapshot por día. Nunca lanza excepción.
        /// </summary>
        void RespaldoAutomatico();

        ResultadoBackup CrearRespaldo(string destino);
        ResultadoBackup Restaurar(string origen);

        /// <summary>Mueve la base de datos a una carpeta nueva y recuerda la ubicación.</summary>
        ResultadoBackup CambiarUbicacionBaseDatos(string nuevaCarpeta);

        /// <summary>Cambia la carpeta donde se guardan los respaldos automáticos.</summary>
        void CambiarCarpetaRespaldos(string carpeta);
    }

    public class ResultadoBackup
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;

        public static ResultadoBackup Ok(string mensaje) => new() { Exito = true, Mensaje = mensaje };
        public static ResultadoBackup Error(string mensaje) => new() { Exito = false, Mensaje = mensaje };
    }
}
