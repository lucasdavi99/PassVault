namespace OrionVault.Configuration
{
    /// <summary>
    /// Centraliza os switches de build que precisamos alterar rapidamente durante os testes.
    /// Lembre-se de colocar ForceVipForTesting = false antes de publicar uma nova versão.
    /// </summary>
    public static class AppBuildSettings
    {
        // Ajuste para true apenas enquanto for necessário testar o app em modo premium.
        public const bool ForceVipForTesting = false;
    }
}
