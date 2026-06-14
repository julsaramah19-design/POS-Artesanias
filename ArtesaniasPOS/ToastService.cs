using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using ArtesaniasPOS.Core;

namespace ArtesaniasPOS.UI
{
    /// <summary>
    /// Muestra notificaciones flotantes (toasts) en una esquina de la ventana.
    /// Se inicializa una vez con el panel contenedor y luego se invoca desde
    /// cualquier parte (directamente o vía <see cref="Notifier"/>).
    /// </summary>
    public static class ToastService
    {
        private static Panel? _host;

        public static void Initialize(Panel host) => _host = host;

        public static void Show(string mensaje, NotificacionTipo tipo = NotificacionTipo.Info)
        {
            if (_host == null || string.IsNullOrWhiteSpace(mensaje)) return;

            // Asegurar ejecución en el hilo de UI.
            if (!_host.Dispatcher.CheckAccess())
            {
                _host.Dispatcher.Invoke(() => Show(mensaje, tipo));
                return;
            }

            var (fondo, acento, icono) = Estilo(tipo);

            var barra = new Border
            {
                Width = 5,
                Background = new SolidColorBrush(acento),
                CornerRadius = new CornerRadius(3, 0, 0, 3)
            };

            var iconoTxt = new TextBlock
            {
                Text = icono,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(acento),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 10, 0)
            };

            var texto = new TextBlock
            {
                Text = mensaje,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12.5,
                Foreground = new SolidColorBrush(Color.FromRgb(0x1b, 0x1c, 0x1b)),
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 14, 0)
            };

            var fila = new DockPanel { LastChildFill = true };
            DockPanel.SetDock(barra, Dock.Left);
            DockPanel.SetDock(iconoTxt, Dock.Left);
            fila.Children.Add(barra);
            fila.Children.Add(iconoTxt);
            fila.Children.Add(texto);

            var tarjeta = new Border
            {
                Background = new SolidColorBrush(fondo),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xdd, 0xc0, 0xb8)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                MinWidth = 260,
                MaxWidth = 340,
                Margin = new Thickness(0, 8, 0, 0),
                Padding = new Thickness(0, 10, 0, 10),
                Child = fila,
                Opacity = 0,
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(0x56, 0x42, 0x3c),
                    Opacity = 0.22,
                    BlurRadius = 18,
                    ShadowDepth = 3,
                    Direction = 270
                },
                RenderTransform = new TranslateTransform(40, 0)
            };

            _host.Children.Add(tarjeta);

            // Entrada: desliza desde la derecha + aparece.
            tarjeta.BeginAnimation(UIElement.OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)));
            tarjeta.RenderTransform.BeginAnimation(TranslateTransform.XProperty,
                new DoubleAnimation(40, 0, TimeSpan.FromMilliseconds(260))
                { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });

            // Salida automática.
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3.6) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                var salida = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(280));
                salida.Completed += (_, __) => _host?.Children.Remove(tarjeta);
                tarjeta.BeginAnimation(UIElement.OpacityProperty, salida);
            };
            timer.Start();
        }

        private static (Color fondo, Color acento, string icono) Estilo(NotificacionTipo tipo) => tipo switch
        {
            NotificacionTipo.Exito       => (Color.FromRgb(0xe6, 0xf0, 0xe9), Color.FromRgb(0x3f, 0x6b, 0x4f), "✓"),
            NotificacionTipo.Error       => (Color.FromRgb(0xfd, 0xec, 0xea), Color.FromRgb(0xba, 0x1a, 0x1a), "✕"),
            NotificacionTipo.Advertencia => (Color.FromRgb(0xf7, 0xef, 0xdf), Color.FromRgb(0x7a, 0x5a, 0x17), "!"),
            _                            => (Color.FromRgb(0xff, 0xff, 0xff), Color.FromRgb(0x9d, 0x3d, 0x1c), "i"),
        };
    }
}
