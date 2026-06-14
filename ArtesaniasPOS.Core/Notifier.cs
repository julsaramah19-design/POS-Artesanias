namespace ArtesaniasPOS.Core
{
    public enum NotificacionTipo { Exito, Error, Info, Advertencia }

    /// <summary>
    /// Puente ligero para mostrar notificaciones (toasts) desde la capa de
    /// ViewModels sin acoplar Core con la UI. La capa de UI asigna
    /// <see cref="Handler"/> al iniciar; si nadie lo asigna, no pasa nada.
    /// </summary>
    public static class Notifier
    {
        public static Action<string, NotificacionTipo>? Handler { get; set; }

        public static void Mostrar(string mensaje, NotificacionTipo tipo = NotificacionTipo.Info)
            => Handler?.Invoke(mensaje, tipo);

        public static void Exito(string mensaje) => Mostrar(mensaje, NotificacionTipo.Exito);
        public static void Error(string mensaje) => Mostrar(mensaje, NotificacionTipo.Error);
        public static void Info(string mensaje) => Mostrar(mensaje, NotificacionTipo.Info);
        public static void Advertencia(string mensaje) => Mostrar(mensaje, NotificacionTipo.Advertencia);
    }
}
