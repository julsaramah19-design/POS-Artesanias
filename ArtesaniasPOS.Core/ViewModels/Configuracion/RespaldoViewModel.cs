using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Core.ViewModels.Configuracion
{
    /// <summary>
    /// Respaldo y restauración de la base de datos, y cambio de su ubicación.
    /// La base vive en una carpeta persistente; conviene además guardar copias
    /// en un lugar seguro (USB, nube). El respaldo automático se actualiza tras
    /// cada venta y al cerrar la aplicación.
    /// </summary>
    public class RespaldoViewModel : ViewModelBase
    {
        private readonly IBackupService _backup;
        private string _mensaje = string.Empty;
        private bool _esError;

        public RespaldoViewModel(IBackupService backup)
        {
            _backup = backup;
        }

        public string RutaBaseDatos => _backup.RutaBaseDatos;
        public string CarpetaRespaldos => _backup.CarpetaRespaldos;
        public string Tamano => _backup.TamanoBaseDatos;

        public string UltimoRespaldoTexto => _backup.UltimoRespaldoAuto is { } f
            ? f.ToString("dd/MM/yyyy hh:mm tt")
            : "Aún no se ha hecho un respaldo automático";

        public string NombreSugerido =>
            $"ArtesaniasPOS_respaldo_{DateTime.Now:yyyy-MM-dd_HHmm}.db";

        public string Mensaje { get => _mensaje; private set => SetProperty(ref _mensaje, value); }
        public bool EsError { get => _esError; private set => SetProperty(ref _esError, value); }

        public void CrearRespaldo(string destino)
        {
            Aplicar(_backup.CrearRespaldo(destino));
            Refrescar();
        }

        public bool Restaurar(string origen)
        {
            var r = _backup.Restaurar(origen);
            Aplicar(r);
            Refrescar();
            return r.Exito;
        }

        public bool CambiarUbicacion(string nuevaCarpeta)
        {
            var r = _backup.CambiarUbicacionBaseDatos(nuevaCarpeta);
            Aplicar(r);
            Refrescar();
            return r.Exito;
        }

        public void CambiarCarpetaRespaldos(string carpeta)
        {
            _backup.CambiarCarpetaRespaldos(carpeta);
            Mensaje = $"Carpeta de respaldos actualizada:\n{carpeta}";
            EsError = false;
            Refrescar();
        }

        private void Aplicar(ResultadoBackup r)
        {
            Mensaje = r.Mensaje;
            EsError = !r.Exito;
        }

        private void Refrescar()
        {
            OnPropertyChanged(nameof(RutaBaseDatos));
            OnPropertyChanged(nameof(CarpetaRespaldos));
            OnPropertyChanged(nameof(Tamano));
            OnPropertyChanged(nameof(UltimoRespaldoTexto));
        }
    }
}
