using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Yuta.FactoryOps.Client.FactoryOps;

namespace Yuta.FactoryOps.Client.Pages;

public partial class Dashboard : IAsyncDisposable
{
    [Inject] private IFactoryOpsDataService FactoryOpsData { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private const int MaxPts = 50;

    private List<Asset> _assets = new();
    private List<Prescription> _prescriptions = new();
    private List<MaintenanceRecord> _maintenanceLog = new();
    private List<AcqMode> _acqModes = new();
    private List<FaultType> _faultTypes = new();
    private Thresholds _thresholds = new();

    private string _selectedAssetId = "PH";
    private string _activeTab = "overview";
    private string _activeRightPanel = "chat";
    private string _alertFilter = "all";
    private string _maintFilter = "all";
    private string _theme = "light";
    private string _historyFilter = "Todos os ativos";

    private readonly List<double> _vibData = new();
    private readonly List<double> _tempData = new();
    private readonly List<string> _labels = new();
    private readonly Random _rng = new();

    private readonly List<(string Role, string Html)> _chatMessages = new();
    private string _chatInput = string.Empty;
    private bool _chatTyping;

    private bool _faultModalOpen;
    private string _faultType = string.Empty;

    private bool _assetDetailModalOpen;
    private string _assetDetailTab = "ident";
    private string? _assetDetailAssetId;

    private bool _aboutModalOpen;
    private string _aboutTab = "sensor";

    private bool _reportModalOpen;
    private int _reportProgress;
    private string _reportStatus = string.Empty;
    private bool _reportDone;

    private string? _mapSelectedComponent;
    private int _mapSelectedScore;
    private string? _mapSelectedAssetName;

    private DateTime _now = DateTime.Now;
    private Timer? _clockTimer;
    private Timer? _tickTimer;
    private bool _chartsReady;

    private Asset CurrentAsset => _assets.First(a => a.Id == _selectedAssetId);

    protected override async Task OnInitializedAsync()
    {
        _assets = await FactoryOpsData.GetAssetsAsync();
        _prescriptions = await FactoryOpsData.GetPrescriptionsAsync();
        _maintenanceLog = await FactoryOpsData.GetMaintenanceLogAsync();
        _thresholds = await FactoryOpsData.GetThresholdsAsync();
        _acqModes = await FactoryOpsData.GetAcquisitionModesAsync();
        _faultTypes = await FactoryOpsData.GetFaultTypesAsync();

        _selectedAssetId = _assets.FirstOrDefault(a => a.Status == "warning")?.Id ?? _assets[0].Id;
        SeedTrendData();

        _clockTimer = new Timer(_ => InvokeAsync(() => { _now = DateTime.Now; StateHasChanged(); }), null, 0, 1000);
        _tickTimer = new Timer(_ => InvokeAsync(Tick), null, 1300, 1300);

        _ = WelcomeMessageAsync();
    }

    private async Task WelcomeMessageAsync()
    {
        await Task.Delay(600);
        await InvokeAsync(() =>
        {
            _chatMessages.Add(("ai", "Sistema inicializado. Monitorando <b>6 ativos</b> — Linha 1, MetalForm Indústria, Campinas.<br><br>⚠️ <b>Atenção:</b> detectei alertas críticos na <b>Prensa Hidráulica PH-02</b> — vibração elevada e temperatura de óleo acima do limiar. Recomendo diagnóstico imediato.<br><br>Use os atalhos rápidos ou descreva qualquer sintoma para análise."));
            StateHasChanged();
        });
    }

    private string _lastRenderedTab = string.Empty;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitAllChartsAsync();
            _chartsReady = true;
            _lastRenderedTab = _activeTab;
            return;
        }

        // Chart.js precisa que o canvas já esteja visível (display != none) para calcular o
        // tamanho correto. Como as abas ficam sempre montadas no DOM (só trocam de classe
        // "active"), reinicializamos o gráfico da aba assim que ela se torna visível.
        if (_lastRenderedTab != _activeTab)
        {
            _lastRenderedTab = _activeTab;
            switch (_activeTab)
            {
                case "fft": await RenderFftAsync(); break;
                case "history": await RenderHistoryAsync(); break;
                case "energy": await RenderEnergyAsync(); break;
            }
        }
    }

    private void SeedTrendData()
    {
        _vibData.Clear(); _tempData.Clear(); _labels.Clear();
        var a = CurrentAsset;
        for (int i = 0; i < MaxPts; i++)
        {
            _labels.Add("");
            _vibData.Add(Math.Round(a.VibBase * (0.88 + _rng.NextDouble() * 0.28), 3));
            _tempData.Add(Math.Round(a.TempBase * (0.97 + _rng.NextDouble() * 0.06), 1));
        }
    }

    private async Task Tick()
    {
        await FactoryOpsData.ApplyRealtimeTickAsync(_selectedAssetId);
        var a = CurrentAsset;
        if (_vibData.Count >= MaxPts) { _vibData.RemoveAt(0); _tempData.RemoveAt(0); _labels.RemoveAt(0); }
        _vibData.Add(a.Rms); _tempData.Add(a.Bearing); _labels.Add("");
        if (_chartsReady)
            await JS.InvokeVoidAsync("fops.pushTrendData", a.Rms, a.Bearing, MaxPts);
        StateHasChanged();
    }

    private async Task SelectAsset(string id)
    {
        _selectedAssetId = id;
        SeedTrendData();
        StateHasChanged();
        await InitAllChartsAsync();
    }

    private async Task InitAllChartsAsync()
    {
        var a = CurrentAsset;
        await JS.InvokeVoidAsync("fops.drawOeeRing", "oeeRing", a.Oee, OeeRingColor(a.Oee));
        double vColor = a.Rms > _thresholds.Vib.Crit ? 0 : a.Rms > _thresholds.Vib.Warn ? 1 : 2;
        await JS.InvokeVoidAsync("fops.initTrendCharts", "vibChart", "tempChart", _labels, _vibData, _tempData,
            HexColor(a.Rms > _thresholds.Vib.Crit ? "crit" : a.Rms > _thresholds.Vib.Warn ? "warn" : "pu"),
            HexColor(a.Bearing > _thresholds.Temp.Crit ? "crit" : a.Bearing > _thresholds.Temp.Warn ? "warn" : "ok"),
            a.VibBase * 2.5, a.TempBase - 20, a.TempBase + 25);

        await RenderFftAsync();
        await RenderHistoryAsync();
        await RenderEnergyAsync();
    }

    private static string HexColor(string kind) => kind switch
    {
        "crit" => "#8b1515",
        "warn" => "#c8941a",
        "ok" => "#1a8c4e",
        _ => "#7b2fbe"
    };

    private static string OeeRingColor(int oee) => oee >= 80 ? "#9b45d8" : oee >= 65 ? "#c8941a" : "#8b1515";

    // ======== TABS ========
    private void ShowTab(string tab) => _activeTab = tab;
    private void ShowRightPanel(string panel) => _activeRightPanel = panel;

    private void ToggleTheme()
    {
        _theme = _theme == "dark" ? "light" : "dark";
    }

    private bool _adminMenuOpen;
    private void ToggleAdminMenu() => _adminMenuOpen = !_adminMenuOpen;

    private bool _mobileRightPanelOpen;
    private void ToggleMobileRightPanel() => _mobileRightPanelOpen = !_mobileRightPanelOpen;

    private bool _sidebarMenuOpen;
    private void ToggleSidebarMenu() => _sidebarMenuOpen = !_sidebarMenuOpen;

    private bool _moreTabsMenuOpen;
    private void ToggleMoreTabsMenu() => _moreTabsMenuOpen = !_moreTabsMenuOpen;

    private static readonly string[] SecondaryTabs = { "config", "energy", "maint" };
    private bool IsSecondaryTabActive => SecondaryTabs.Contains(_activeTab);
    private static string TabLabel(string tab) => tab switch
    {
        "config" => "Limiares",
        "energy" => "Energia",
        "maint" => "Manutenção",
        _ => "Mais"
    };

    private void ShowTabFromMenu(string tab)
    {
        _activeTab = tab;
        _moreTabsMenuOpen = false;
    }

    private static double ParseDouble(object? value) =>
        double.TryParse(value?.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0;

    private int LineOee => _assets.Count == 0 ? 0 : (int)Math.Round(_assets.Average(a => a.Oee));

    private readonly Dictionary<string, List<int>> _sidebarSparkCache = new();
    private List<int> SidebarSpark(Asset a)
    {
        if (_sidebarSparkCache.TryGetValue(a.Id, out var cached)) return cached;
        var seedRng = new Random(a.Id.GetHashCode());
        var vals = Enumerable.Range(0, 10).Select(_ => a.Oee * (0.92 + seedRng.NextDouble() * .16)).ToList();
        double mx = vals.Max(), mn = vals.Min();
        var heights = vals.Select(v => (int)Math.Round((v - mn) / (mx - mn + .01) * 12 + 3)).ToList();
        _sidebarSparkCache[a.Id] = heights;
        return heights;
    }

    private readonly Dictionary<string, int> _meshSignalCache = new();
    private int MeshSignal(Asset a)
    {
        if (_meshSignalCache.TryGetValue(a.Id, out var cached)) return cached;
        var seedRng = new Random(a.Id.GetHashCode() + 1);
        var sig = seedRng.Next(82, 97);
        _meshSignalCache[a.Id] = sig;
        return sig;
    }

    // ======== FFT ========
    private List<string> _fftLabels = new();
    private List<double> _fftAmps = new();
    private double _fftMax; private int _fftPeakFreq; private double _fftKurtosis; private double _fftSnr;
    private List<(int N, double Freq, double Amp, string Label)> _harmonics = new();
    private List<(string Sev, string Msg, string Rec)> _spectralDiag = new();

    private async Task RenderFftAsync()
    {
        var a = CurrentAsset;
        var freqs = Enumerable.Range(0, 80).Select(i => i * 50 + 50).ToList();
        var rpm = a.Rpm;
        double bpfo = rpm / 60.0 * 4.7, bpfi = rpm / 60.0 * 5.4;
        var amps = new List<double>();
        var colors = new List<string>();
        foreach (var f in freqs)
        {
            double v = _rng.NextDouble() * 0.05 + 0.02;
            foreach (var n in new[] { 1, 2, 3, 4 })
            {
                if (Math.Abs(f - rpm / 60.0 * n) < 30) v += 0.4 / n + (a.Rms > 3 ? 0.25 : 0.05);
                if (Math.Abs(f - bpfo * n) < 25) v += 0.15 / n + (a.Rms > 3 ? 0.35 : 0.03);
                if (Math.Abs(f - bpfi * n) < 25) v += 0.12 / n;
            }
            amps.Add(Math.Round(v, 4));
            colors.Add(v > 0.4 ? "rgba(139,21,21,.85)" : v > 0.2 ? "rgba(200,148,26,.75)" : "rgba(123,47,190,.65)");
        }
        _fftLabels = freqs.Select(f => f + "Hz").ToList();
        _fftAmps = amps;
        _fftMax = amps.Max();
        _fftPeakFreq = freqs[amps.IndexOf(_fftMax)];
        _fftKurtosis = Math.Round(2.8 + _rng.NextDouble() * 4, 2);
        _fftSnr = Math.Round(18 + _rng.NextDouble() * 8, 1);

        var harmLabels = new[] { "1X (rotação)", "2X (desbal.)", "3X (misan.)", "4X (folga)" };
        _harmonics = new[] { 1, 2, 3, 4 }.Select(n => (n, Math.Round(rpm / 60.0 * n, 1), Math.Round(_rng.NextDouble() * .3 + .05, 3), harmLabels[n - 1])).ToList();

        _spectralDiag = _fftMax > 0.4
            ? new List<(string, string, string)>
            {
                ("crit", "Amplitude elevada na freq. de defeito externo de rolamento (BPFO)", "Substituição de rolamento em até 72h"),
                ("warn", "Harmônico 2× acima do threshold — possível desbalanceamento", "Balanceamento dinâmico no próximo turno")
            }
            : new List<(string, string, string)>
            {
                ("info", "Espectro dentro dos limites operacionais normais", "Manter monitoramento periódico"),
                ("info", "Nenhuma frequência de defeito de rolamento identificada", "Próxima análise em 7 dias")
            };

        if (_chartsReady || true)
            await JS.InvokeVoidAsync("fops.initFftChart", "fftChart", _fftLabels, _fftAmps, colors);
    }

    private async Task TriggerHighFreqAsync()
    {
        Notify("Aquisição de alta frequência iniciada...");
        await Task.Delay(400);
        await RenderFftAsync();
        StateHasChanged();
    }

    // ======== HISTORY ========
    private static readonly string[] Days = { "Seg", "Ter", "Qua", "Qui", "Sex", "Sab", "Dom" };
    private static readonly string[] ChartColors = { "#7b2fbe", "#8b1515", "#1a8c4e", "#c8941a", "#2196F3", "#FF5722" };

    private async Task RenderHistoryAsync()
    {
        var datasets = _assets.Select((a, i) => new
        {
            label = string.Join(' ', a.Name.Split(' ').Take(2)),
            data = Days.Select(_ => (int)Math.Round(a.Oee * (0.9 + _rng.NextDouble() * .15))).ToArray(),
            borderColor = ChartColors[i % ChartColors.Length],
            backgroundColor = ChartColors[i % ChartColors.Length] + "18",
            borderWidth = 1.5,
            pointRadius = 3,
            tension = .3
        }).ToList();

        var availData = Days.Select(_ => (int)Math.Round(88 + _rng.NextDouble() * 10)).ToList();
        var stopData = new[] { 35, 25, 25, 15 };
        var cycleData = new[] { (int)Math.Round(120 + _rng.NextDouble() * 30), (int)Math.Round(120 + _rng.NextDouble() * 30), (int)Math.Round(100 + _rng.NextDouble() * 20) };

        await JS.InvokeVoidAsync("fops.initHistoryCharts", Days, datasets, availData,
            new[] { "Mec.", "Elét.", "Proc.", "Qual." }, stopData,
            new[] { "M", "T", "N" }, cycleData);
    }

    private async Task OnHistoryFilterChanged(ChangeEventArgs e)
    {
        _historyFilter = e.Value?.ToString() ?? "Todos os ativos";
        await RenderHistoryAsync();
    }

    // ======== HEALTH MAP ========
    private void ShowMapDetail(string comp, int score, string assetName)
    {
        _mapSelectedComponent = comp;
        _mapSelectedScore = score;
        _mapSelectedAssetName = assetName;
    }

    private static readonly (string Time, string Color, string Text)[] TimelineEvents =
    {
        ("09:05", "var(--red2)", "CRÍTICO: Vibração elevada — Prensa PH-02"),
        ("08:55", "var(--gold2)", "Alerta: Temp óleo 68°C — Prensa PH-02"),
        ("08:47", "var(--gold2)", "Eficiência térmica 94.2% — Caldeira CA-03"),
        ("08:31", "var(--pu2)", "Ciclo #3.421 concluído — Corte Laser CL-01"),
        ("08:00", "var(--green2)", "Início de turno — todos os ativos operacionais"),
        ("07:15", "var(--pu2)", "Calibração OK — Robô MR-06"),
        ("06:30", "var(--green2)", "Sistema inicializado — Gateway YUT-GW-01 online")
    };

    // ======== ENERGY ========
    private async Task RenderEnergyAsync()
    {
        var powerData = _assets.Select(a => a.Power * (0.85 + _rng.NextDouble() * .3)).ToList();
        var labels = _assets.Select(a => string.Join(' ', a.Name.Split(' ').Take(2))).ToList();
        var colors = ChartColors.Select(c => c + "aa").ToList();
        var hours = Enumerable.Range(0, 24).Select(i => $"{i:00}:00").ToList();
        int totalPower = _assets.Sum(a => a.Power);
        var trendData = hours.Select((_, i) =>
        {
            double baseVal = (i >= 6 && i <= 22) ? totalPower * 0.85 : totalPower * 0.4;
            return Math.Round(baseVal * (0.9 + _rng.NextDouble() * .2));
        }).ToList();

        await JS.InvokeVoidAsync("fops.initEnergyCharts", labels, powerData, colors, hours, trendData);
    }

    private int TotalPowerNominal => _assets.Sum(a => a.Power);
    private int CurrentPowerEstimate => (int)Math.Round(TotalPowerNominal * (0.85 + _rng.NextDouble() * .15));

    // ======== ALERTS ========
    private IEnumerable<AssetAlert> AllAlerts => _assets.SelectMany(a => a.Alerts).OrderByDescending(a => a.Ts);
    private IEnumerable<AssetAlert> FilteredAlerts => _alertFilter == "all" ? AllAlerts : AllAlerts.Where(a => a.Sev == _alertFilter);
    private int AlertBadgeCount => AllAlerts.Count(a => a.Sev == "crit") is var c && c > 0 ? c : AllAlerts.Count();

    private void FilterAlerts(string f) => _alertFilter = f;

    private async Task DismissAlertAsync(string id)
    {
        await FactoryOpsData.DismissAlertAsync(id);
    }

    private async Task ClearAlertsAsync()
    {
        await FactoryOpsData.ClearAllAlertsAsync();
        Notify("Todos os alertas foram limpos");
    }

    // ======== MAINTENANCE ========
    private IEnumerable<MaintenanceRecord> FilteredMaint => _maintFilter == "all" ? _maintenanceLog : _maintenanceLog.Where(m => m.Type == _maintFilter);
    private void FilterMaint(string f) => _maintFilter = f;

    // ======== PRESCRIPTIONS ========
    private IEnumerable<Prescription> PendingPrescriptions => _prescriptions.Where(p => p.Status == "pending");
    private IEnumerable<Prescription> DonePrescriptions => _prescriptions.Where(p => p.Status == "done");

    private async Task CompletePrescriptionAsync(string id)
    {
        await FactoryOpsData.CompletePrescriptionAsync(id);
        Notify("Prescrição marcada como concluída", "var(--green)");
    }

    // ======== FAULT INJECTION ========
    private void OpenFaultModal()
    {
        _faultType = _faultTypes[0].Id;
        _faultModalOpen = true;
    }

    private void CloseFaultModal() => _faultModalOpen = false;

    private async Task InjectFaultAsync()
    {
        _faultModalOpen = false;
        var alert = await FactoryOpsData.InjectFaultAsync(_selectedAssetId, _faultType);
        var faultLabel = _faultTypes.First(f => f.Id == _faultType).Label;
        Notify("⚠ Falha injetada em " + CurrentAsset.Name, "var(--red)");
        await InitAllChartsAsync();
        StateHasChanged();

        await Task.Delay(1000);
        _chatMessages.Add(("ai", $"Detectei uma falha injetada em <b>{CurrentAsset.Name}</b>. Tipo: <b>{faultLabel}</b>.<br><br>Diagnóstico preliminar gerado. Recomendo inspeção imediata e revisão das prescrições no painel ao lado."));
        _activeRightPanel = "presc";
        StateHasChanged();
    }

    // ======== ASSET DETAIL MODAL ========
    private void OpenAssetDetail(string id)
    {
        _assetDetailAssetId = id;
        _assetDetailTab = "ident";
        _assetDetailModalOpen = true;
    }

    private void CloseAssetDetail() => _assetDetailModalOpen = false;
    private void SwitchAdmTab(string tab) => _assetDetailTab = tab;

    // ======== ABOUT MODAL ========
    private void OpenAboutModal() { _aboutTab = "sensor"; _aboutModalOpen = true; }
    private void CloseAboutModal() => _aboutModalOpen = false;
    private void ShowAbt(string tab) => _aboutTab = tab;

    private static readonly string[] FactoryProcessSteps =
    {
        "Recebimento — Bobinas e barras de aço inspecionadas",
        "Corte a Laser — Chapas cortadas em geometria CNC",
        "Corte de Perfis — Barras e tubos seccionados por serra CNC",
        "Conformação — Laminação e prensagem hidráulica 200t",
        "Soldagem — Montagem robotizada MIG/TIG 6 eixos",
        "Tratamento Térmico — Vapor 15 t/h para alívio de tensões",
        "Acabamento — Pintura eletrostática e revestimento",
        "Inspeção — Controle dimensional e ensaios não destrutivos"
    };

    // ======== REPORT ========
    private async Task ExportReportAsync()
    {
        _reportModalOpen = true; _reportProgress = 0; _reportDone = false;
        _reportStatus = "Compilando dados de OEE e vibração...";
        StateHasChanged();
        await Task.Delay(800); _reportProgress = 30; _reportStatus = "Analisando espectros FFT..."; StateHasChanged();
        await Task.Delay(800); _reportProgress = 60; _reportStatus = "Processando histórico de manutenção..."; StateHasChanged();
        await Task.Delay(800); _reportProgress = 90; _reportStatus = "Gerando diagnósticos do Agente IA..."; StateHasChanged();
        await Task.Delay(800); _reportProgress = 100; _reportDone = true; StateHasChanged();
    }

    private void CloseReportModal() => _reportModalOpen = false;

    private async Task DownloadReportAsync()
    {
        var a = CurrentAsset;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("RELATÓRIO FACTORYOPS — OMEGA SENSE");
        sb.AppendLine(new string('=', 40));
        sb.AppendLine($"Gerado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        sb.AppendLine();
        sb.AppendLine($"ATIVO: {a.Name}");
        sb.AppendLine($"Tipo: {a.Type}");
        sb.AppendLine($"Status: {a.Status.ToUpperInvariant()}");
        sb.AppendLine();
        sb.AppendLine($"OEE: {a.Oee}%");
        sb.AppendLine($"  Disponibilidade: {a.Avail}%");
        sb.AppendLine($"  Desempenho: {a.Perf}%");
        sb.AppendLine($"  Qualidade: {a.Qual}%");
        sb.AppendLine();
        sb.AppendLine($"Saúde: {a.Health}%");
        sb.AppendLine($"Vibração RMS: {a.Rms:0.000} g (baseline: {a.VibBase} g)");
        sb.AppendLine($"Temperatura: {a.Bearing:0.0}°C");
        sb.AppendLine();
        sb.AppendLine($"ALERTAS ATIVOS: {a.Alerts.Count}");
        foreach (var al in a.Alerts) sb.AppendLine($"  [{al.Sev.ToUpperInvariant()}] {al.Msg}");
        sb.AppendLine();
        sb.AppendLine("LIMIARES:");
        sb.AppendLine($"  Vibração — Aviso: {_thresholds.Vib.Warn}g | Crítico: {_thresholds.Vib.Crit}g");
        sb.AppendLine($"  Temperatura — Aviso: {_thresholds.Temp.Warn}°C | Crítico: {_thresholds.Temp.Crit}°C");

        await JS.InvokeVoidAsync("fops.downloadTextFile", $"factoryops_report_{a.Id}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.txt", sb.ToString());
        _reportModalOpen = false;
        Notify("Relatório exportado com sucesso", "var(--green)");
    }

    // ======== ACQUISITION MODE ========
    private async Task ToggleAcqAsync(int index)
    {
        await FactoryOpsData.SetAcquisitionModeAsync(index);
        Notify(_acqModes[index].Label + " ativado");
    }

    // ======== NOTIFICATIONS ========
    private string? _notifMessage;
    private string _notifColor = "var(--pu)";
    private CancellationTokenSource? _notifCts;

    private void Notify(string msg, string color = "var(--pu)")
    {
        _notifMessage = msg;
        _notifColor = color;
        StateHasChanged();
        _notifCts?.Cancel();
        _notifCts = new CancellationTokenSource();
        var token = _notifCts.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(3500, token);
                if (!token.IsCancellationRequested)
                    await InvokeAsync(() => { _notifMessage = null; StateHasChanged(); });
            }
            catch (TaskCanceledException) { }
        });
    }

    // ======== CHAT ========
    private async Task SendChatAsync()
    {
        var text = _chatInput.Trim();
        if (string.IsNullOrEmpty(text)) return;
        _chatInput = string.Empty;
        _chatMessages.Add(("user", text));
        _chatTyping = true;
        StateHasChanged();
        await JS.InvokeVoidAsync("fops.scrollToBottom", "chatMsgs");

        var delay = 400 + _rng.Next(0, 600);
        await Task.Delay(delay);
        var response = OmegaAssistant.GetResponse(text, CurrentAsset, _thresholds);
        _chatTyping = false;
        _chatMessages.Add(("ai", response));
        StateHasChanged();
        await JS.InvokeVoidAsync("fops.scrollToBottom", "chatMsgs");
    }

    private async Task UsePreset(string preset)
    {
        _chatInput = preset;
        await SendChatAsync();
    }

    private async Task OnChatKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendChatAsync();
        }
    }

    // ======== HELPERS PARA MARKUP ========
    private static string StatusLabel(string status) => status switch { "running" => "OPERANDO", "warning" => "ALERTA", _ => "FALHA" };
    private static string StatusBadgeClass(string status) => status switch { "running" => "badge-ok", "warning" => "badge-warn", _ => "badge-crit" };
    private static string StatusDotClass(string status) => status switch { "running" => "dot-ok", "warning" => "dot-warn", _ => "dot-crit" };
    private static string AssetDotClass(string status) => status switch { "running" => "dot-g", "warning" => "dot-y", _ => "dot-r" };
    private static string OeeTextColor(int oee) => oee >= 80 ? "var(--green2)" : oee >= 65 ? "var(--gold2)" : "var(--red2)";
    private static string HealthColor(int health) => health >= 80 ? "var(--green2)" : health >= 55 ? "var(--gold2)" : "var(--red2)";
    private static string HealthKpiClass(int health) => health >= 80 ? "kpi-green" : health >= 55 ? "kpi-gold" : "kpi-red";
    private static string VibKpiClass(Asset a) => a.Rms <= a.VibBase * 1.2 ? "kpi-green" : a.Rms <= a.VibBase * 2 ? "kpi-gold" : "kpi-red";
    private static string TempKpiClass(Asset a) => a.Bearing <= 65 ? "kpi-green" : a.Bearing <= 70 ? "kpi-gold" : "kpi-red";
    private static string ComponentColor(int score) => score >= 85 ? "rgba(26,140,78,.8)" : score >= 65 ? "rgba(200,148,26,.75)" : score >= 45 ? "rgba(139,21,21,.8)" : "rgba(80,10,10,.9)";
    private static string ComponentTextColor(int score) => score >= 85 ? "var(--green2)" : score >= 65 ? "var(--gold2)" : "var(--red2)";
    private static string ComponentLabel(string key) => System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(key.Replace('_', ' '));
    private static string MaintTypeLabel(string type) => type switch { "preventive" => "Preventiva", "corrective" => "Corretiva", _ => "Preditiva" };
    private static string MaintTypeBadge(string type) => type switch { "preventive" => "mb-prev", "corrective" => "mb-corr", _ => "mb-pred" };
    private static string MaintStatusIcon(string status) => status switch { "done" => "✓", "pending" => "◷", _ => "📅" };
    private static string MaintStatusClass(string status) => status switch { "done" => "mb-done", "pending" => "mb-pending", _ => "mb-sched" };

    public async ValueTask DisposeAsync()
    {
        _clockTimer?.Dispose();
        _tickTimer?.Dispose();
        _notifCts?.Cancel();
        await Task.CompletedTask;
    }
}
