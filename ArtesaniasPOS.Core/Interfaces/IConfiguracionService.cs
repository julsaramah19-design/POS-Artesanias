namespace ArtesaniasPOS.Core.Interfaces
{
    public interface IConfiguracionService
    {
        Task<string> ObtenerValorAsync(string clave);
        Task<Dictionary<string, string>> ObtenerTodasAsync();
        Task GuardarValorAsync(string clave, string valor);
        Task GuardarVariosAsync(Dictionary<string, string> configuraciones);
        Task<bool> WizardCompletadoAsync();
        Task MarcarWizardCompletadoAsync();
    }
}
