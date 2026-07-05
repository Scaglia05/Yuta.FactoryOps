namespace Yuta.FactoryOps.Client.FactoryOps;

/// <summary>
/// Fonte de dados de monitoramento de ativos (OEE, vibração, temperatura, manutenção etc.).
/// Hoje implementada por <see cref="MockFactoryOpsDataService"/> (dados simulados em memória),
/// já que ainda não existe uma API real para telemetria de ativos. A interface foi desenhada
/// para permitir substituir a implementação por uma versão que consome uma API/gateway real
/// sem alterar quem consome o serviço (Dashboard.razor).
/// </summary>
public interface IFactoryOpsDataService
{
    Task<List<Asset>> GetAssetsAsync();
    Task<Asset?> GetAssetAsync(string assetId);
    Task<List<Prescription>> GetPrescriptionsAsync();
    Task<List<MaintenanceRecord>> GetMaintenanceLogAsync();
    Task<Thresholds> GetThresholdsAsync();
    Task<List<AcqMode>> GetAcquisitionModesAsync();
    Task<List<FaultType>> GetFaultTypesAsync();

    /// <summary>Aplica uma pequena variação aleatória de vibração/temperatura, simulando leituras em tempo real do sensor.</summary>
    Task ApplyRealtimeTickAsync(string assetId);

    Task<AssetAlert> InjectFaultAsync(string assetId, string faultTypeId);
    Task CompletePrescriptionAsync(string prescriptionId);
    Task DismissAlertAsync(string alertId);
    Task ClearAllAlertsAsync();
    Task SetAcquisitionModeAsync(int index);
}
