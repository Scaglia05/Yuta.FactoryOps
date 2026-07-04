namespace Yuta.FactoryOps.Client.FactoryOps;

public class MockFactoryOpsDataService : IFactoryOpsDataService
{
    private readonly List<Asset> _assets;
    private readonly List<Prescription> _prescriptions;
    private readonly List<MaintenanceRecord> _maintenanceLog;
    private readonly Thresholds _thresholds = new()
    {
        Vib = new VibThreshold { Warn = 2.5, Crit = 4.0 },
        Temp = new TempThreshold { Warn = 65, Crit = 75 }
    };
    private readonly List<AcqMode> _acqModes = new()
    {
        new AcqMode { Label = "Contínuo — Baixa Freq.", Sub = "Processo / OEE", Active = true },
        new AcqMode { Label = "Pontual — Alta Freq.", Sub = "PdM / Espectral", Active = false }
    };
    private readonly List<FaultType> _faultTypes = new()
    {
        new FaultType { Id = "cavitation", Label = "Cavitação — Bomba C", Desc = "Injeta pico de vibração 3× baseline com frequência de cavitação", Color = "var(--red2)" },
        new FaultType { Id = "bearing", Label = "Falha de Rolamento", Desc = "Simula frequências BPFO/BPFI elevadas no espectro", Color = "var(--red2)" },
        new FaultType { Id = "unbalance", Label = "Desbalanceamento", Desc = "Aumenta amplitude 1× RPM — desbalanceamento dinâmico", Color = "var(--gold2)" },
        new FaultType { Id = "overheat", Label = "Superaquecimento", Desc = "Temperatura do rolamento acima do limiar crítico", Color = "var(--gold2)" },
        new FaultType { Id = "microstop", Label = "Microparada", Desc = "Simula 5 microparadas consecutivas impactando OEE", Color = "var(--pu2)" }
    };

    private readonly Random _rng = new();

    public MockFactoryOpsDataService()
    {
        _assets = BuildAssets();
        _prescriptions = BuildPrescriptions();
        _maintenanceLog = BuildMaintenanceLog();
    }

    public Task<List<Asset>> GetAssetsAsync() => Task.FromResult(_assets);

    public Task<Asset?> GetAssetAsync(string assetId) => Task.FromResult(_assets.FirstOrDefault(a => a.Id == assetId));

    public Task<List<Prescription>> GetPrescriptionsAsync() => Task.FromResult(_prescriptions);

    public Task<List<MaintenanceRecord>> GetMaintenanceLogAsync() => Task.FromResult(_maintenanceLog);

    public Task<Thresholds> GetThresholdsAsync() => Task.FromResult(_thresholds);

    public Task<List<AcqMode>> GetAcquisitionModesAsync() => Task.FromResult(_acqModes);

    public Task<List<FaultType>> GetFaultTypesAsync() => Task.FromResult(_faultTypes);

    public Task SetAcquisitionModeAsync(int index)
    {
        for (int i = 0; i < _acqModes.Count; i++)
            _acqModes[i].Active = i == index;
        return Task.CompletedTask;
    }

    public Task ApplyRealtimeTickAsync(string assetId)
    {
        var a = _assets.FirstOrDefault(x => x.Id == assetId);
        if (a == null) return Task.CompletedTask;

        double Noise() => (_rng.NextDouble() - 0.5) * 0.08;
        double spike = _rng.NextDouble() < 0.03 ? 1.5 : 1;
        a.Rms = Math.Max(0.1, Math.Round(a.Rms * (1 + Noise() * 1.5) * spike, 3));
        a.Bearing = Math.Max(20, Math.Round(a.Bearing * (1 + Noise() * 0.4), 1));
        return Task.CompletedTask;
    }

    public Task<AssetAlert> InjectFaultAsync(string assetId, string faultTypeId)
    {
        var a = _assets.First(x => x.Id == assetId);
        var nowLabel = DateTime.Now.ToString("HH:mm");
        var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        AssetAlert alert;

        if (faultTypeId is "cavitation" or "bearing")
        {
            a.Rms = Math.Round(a.VibBase * 3.2, 2);
            a.Status = "warning";
            a.Health = Math.Max(a.Health - 25, 30);
            alert = new AssetAlert
            {
                Id = "f" + ts,
                Sev = "crit",
                Msg = $"[FALHA INJETADA] {(faultTypeId == "cavitation" ? "Cavitação detectada" : "Falha de rolamento")} — pico {a.Rms:0.00}g",
                Detail = "Falha injetada para fins de teste. Score de confiança: 94%.",
                Asset = a.Name,
                Time = nowLabel,
                Ts = ts
            };
        }
        else if (faultTypeId == "overheat")
        {
            a.Bearing = Math.Round(_thresholds.Temp.Crit + 5 + _rng.NextDouble() * 5, 1);
            alert = new AssetAlert
            {
                Id = "f" + ts,
                Sev = "crit",
                Msg = $"[FALHA INJETADA] Superaquecimento: {a.Bearing:0.0}°C",
                Detail = "Temperatura crítica simulada.",
                Asset = a.Name,
                Time = nowLabel,
                Ts = ts
            };
        }
        else if (faultTypeId == "microstop")
        {
            a.Oee = Math.Max(a.Oee - 18, 40);
            alert = new AssetAlert
            {
                Id = "f" + ts,
                Sev = "warn",
                Msg = "[FALHA INJETADA] 5 microparadas consecutivas",
                Detail = "OEE reduzido em 18pp. Disponibilidade comprometida.",
                Asset = a.Name,
                Time = nowLabel,
                Ts = ts
            };
        }
        else
        {
            a.Rms = Math.Round(a.VibBase * 2.1, 2);
            alert = new AssetAlert
            {
                Id = "f" + ts,
                Sev = "warn",
                Msg = "[FALHA INJETADA] Desbalanceamento dinâmico detectado",
                Detail = string.Empty,
                Asset = a.Name,
                Time = nowLabel,
                Ts = ts
            };
        }

        a.Alerts.Insert(0, alert);
        _prescriptions.Insert(0, new Prescription
        {
            Id = "pf" + ts,
            Title = "[AUTO] Diagnóstico de falha injetada — " + a.Name,
            Asset = a.Name,
            Priority = faultTypeId == "microstop" ? "warn" : "crit",
            Status = "pending",
            Body = "Prescrição automática gerada pelo Agente Omega. Verificar ativo imediatamente e executar protocolo de resposta a falha.",
            Created = nowLabel
        });

        return Task.FromResult(alert);
    }

    public Task CompletePrescriptionAsync(string prescriptionId)
    {
        var p = _prescriptions.FirstOrDefault(x => x.Id == prescriptionId);
        if (p != null) p.Status = "done";
        return Task.CompletedTask;
    }

    public Task DismissAlertAsync(string alertId)
    {
        foreach (var a in _assets)
            a.Alerts.RemoveAll(al => al.Id == alertId);
        return Task.CompletedTask;
    }

    public Task ClearAllAlertsAsync()
    {
        foreach (var a in _assets)
            a.Alerts.Clear();
        return Task.CompletedTask;
    }

    private static List<Asset> BuildAssets() => new()
    {
        new Asset
        {
            Id = "CL", Name = "Corte Laser CL-01", Type = "Máquina de Corte a Laser 4kW", Img = "img/ativos/corte_laser.jpg",
            Status = "running", Oee = 89, Avail = 94, Perf = 93, Qual = 98, Health = 88,
            VibBase = 0.6, TempBase = 38, Rms = 0.62, Bearing = 39, Power = 25, Rpm = 1750,
            Process = new() { "Carregamento de Chapa", "Alinhamento CNC", "Corte a Laser", "Descarga/Empilhamento" },
            CurrentState = 2, Cycles = 3421,
            Components = new() { ["fonte_laser"] = 92, ["mesa_cnc"] = 88, ["sistema_optico"] = 82, ["exaustor"] = 90, ["chiller"] = 85 },
            Spec = new AssetSpec
            {
                Modelo = "TruLaser 3030 Fiber", Fabricante = "TRUMPF GmbH", Serial = "TL3030-BR-2024-0847",
                Aquisicao = "15/03/2024", Garantia = "15/03/2027",
                Descricao = "Máquina de corte a laser de fibra óptica para processamento de chapas metálicas de até 20mm de espessura. Utilizada na etapa inicial de corte de geometrias complexas para peças automotivas e linha branca.",
                Atividades = new() { "Corte de chapas de aço carbono (0.5–20mm)", "Corte de aço inox (0.5–12mm)", "Corte de alumínio (0.5–8mm)", "Gravação e marcação a laser" },
                Specs = new() { ["Potência Laser"] = "4.000 W (fibra)", ["Área de Trabalho"] = "3.000 × 1.500 mm", ["Velocidade Máx."] = "40 m/min", ["Precisão"] = "± 0.03 mm", ["Gas Assistente"] = "N₂ / O₂", ["Controle"] = "Siemens SINUMERIK 840D", ["Alimentação"] = "400V 3F 50/60Hz", ["Peso"] = "12.500 kg" }
            },
            Alerts = new() { new AssetAlert { Id = "cl1", Sev = "info", Msg = "Ciclo #3.421 concluído — qualidade OK", Detail = "Todas peças dentro da tolerância dimensional. OEE do ciclo: 91%.", Asset = "Corte Laser CL-01", Time = "09:10", Ts = NowMinus(300000) } }
        },
        new Asset
        {
            Id = "PH", Name = "Prensa Hidráulica PH-02", Type = "Prensa Hidráulica 200 ton", Img = "img/ativos/compressores_hidraulicos.jpg",
            Status = "warning", Oee = 72, Avail = 82, Perf = 87, Qual = 96, Health = 61,
            VibBase = 3.2, TempBase = 55, Rms = 5.1, Bearing = 68, Power = 75, Rpm = 1450,
            Process = new() { "Alimentação de Blank", "Prensagem/Estampagem", "Retenção", "Extração da Peça" },
            CurrentState = 1, Cycles = 1892,
            Components = new() { ["cilindro_hidraulico"] = 58, ["vedacoes"] = 52, ["valvulas_direcionais"] = 70, ["bomba_oleo"] = 65, ["matriz"] = 75 },
            Spec = new AssetSpec
            {
                Modelo = "HPX-200T Progressive", Fabricante = "Schuler AG", Serial = "HPX200-BR-2023-1192",
                Aquisicao = "08/11/2023", Garantia = "08/11/2026",
                Descricao = "Prensa hidráulica de dupla ação com capacidade de 200 toneladas para operações de estampagem profunda, conformação e repuxo de chapas metálicas. Integrada à linha de conformação de componentes estruturais.",
                Atividades = new() { "Estampagem profunda de componentes", "Conformação de peças estruturais", "Repuxo de chapas de aço", "Calibração de geometrias estampadas" },
                Specs = new() { ["Força Máxima"] = "200 ton (1.960 kN)", ["Curso"] = "350 mm", ["Abertura Máx."] = "600 mm", ["Área da Mesa"] = "1.200 × 800 mm", ["Velocidade Aprox."] = "25 mm/s", ["Pressão Sistema"] = "280 bar", ["Óleo Hidráulico"] = "ISO VG 46 (320L)", ["Controle"] = "Siemens S7-1500" }
            },
            Alerts = new()
            {
                new AssetAlert { Id = "ph1", Sev = "crit", Msg = "CRÍTICO: Vibração 5.1g no cilindro — possível vazamento", Detail = "RMS 60% acima do baseline. Verificar vedações do cilindro e pressão do sistema. Parada recomendada em 1h.", Asset = "Prensa Hidráulica PH-02", Time = "08:55", Ts = NowMinus(420000) },
                new AssetAlert { Id = "ph2", Sev = "warn", Msg = "Temperatura óleo hidráulico: 68°C", Detail = "Acima do limiar de 65°C. Verificar trocador de calor e nível do reservatório.", Asset = "Prensa Hidráulica PH-02", Time = "08:48", Ts = NowMinus(720000) }
            }
        },
        new Asset
        {
            Id = "CA", Name = "Caldeira Industrial CA-03", Type = "Caldeira a Vapor 15 t/h", Img = "img/ativos/caldeira.jpeg",
            Status = "running", Oee = 92, Avail = 97, Perf = 95, Qual = 99, Health = 91,
            VibBase = 1.2, TempBase = 180, Rms = 1.25, Bearing = 182, Power = 15, Rpm = 3500,
            Process = new() { "Alimentação de Água", "Combustão", "Vaporização", "Distribuição" },
            CurrentState = 3, Cycles = 8744,
            Components = new() { ["queimador"] = 93, ["tubulacao"] = 89, ["economizador"] = 91, ["bomba_alimentacao"] = 88, ["valvulas_seguranca"] = 95 },
            Spec = new AssetSpec
            {
                Modelo = "ATA FT-15 Flamotubular", Fabricante = "ATA Indústria Mecânica", Serial = "FT15-BR-2022-0331",
                Aquisicao = "22/06/2022", Garantia = "22/06/2025",
                Descricao = "Caldeira flamotubular de 3 passes para geração de vapor saturado. Fornece vapor para os processos de tratamento térmico (alívio de tensões) e aquecimento de tanques de desengraxe na linha de acabamento.",
                Atividades = new() { "Geração de vapor saturado a 10 kgf/cm²", "Alimentação do sistema de tratamento térmico", "Aquecimento de tanques de processo", "Suporte ao sistema de climatização industrial" },
                Specs = new() { ["Produção de Vapor"] = "15.000 kg/h", ["Pressão de Trabalho"] = "10 kgf/cm² (manométrica)", ["Temperatura Vapor"] = "184°C", ["Combustível"] = "Gás Natural (GN)", ["Eficiência Térmica"] = "94%", ["Superfície de Aquecimento"] = "125 m²", ["NR-13"] = "Classe A — Cat. V", ["Alimentação Bomba"] = "15 kW 440V" }
            },
            Alerts = new() { new AssetAlert { Id = "ca1", Sev = "info", Msg = "Eficiência térmica estável em 94.2%", Detail = "Dentro da faixa nominal. Próxima inspeção NR-13 em 45 dias.", Asset = "Caldeira Industrial CA-03", Time = "09:05", Ts = NowMinus(180000) } }
        },
        new Asset
        {
            Id = "RC", Name = "Rolo Compressor RC-04", Type = "Laminador de Conformação", Img = "img/ativos/rolo_compressor.png",
            Status = "running", Oee = 85, Avail = 91, Perf = 92, Qual = 98, Health = 80,
            VibBase = 2.4, TempBase = 48, Rms = 2.55, Bearing = 50, Power = 45, Rpm = 1200,
            Process = new() { "Desbobinamento", "Conformação por Rolos", "Calibração", "Corte Final" },
            CurrentState = 1, Cycles = 2156,
            Components = new() { ["rolos_conformacao"] = 78, ["mancais"] = 76, ["motor_principal"] = 85, ["sistema_guia"] = 82, ["lubrificacao"] = 80 },
            Spec = new AssetSpec
            {
                Modelo = "RF-800 Multi-Stage", Fabricante = "Gasparini S.p.A.", Serial = "RF800-BR-2023-0564",
                Aquisicao = "14/05/2023", Garantia = "14/05/2026",
                Descricao = "Perfiladeira de rolos multi-estágio para conformação contínua de perfis metálicos a partir de bobinas de aço. Produz perfis U, C, Z e especiais para estruturas metálicas e componentes automotivos.",
                Atividades = new() { "Conformação contínua de perfis de aço", "Perfilagem de chapas a partir de bobinas", "Produção de perfis U, C e Z", "Corte por cisalhamento no comprimento final" },
                Specs = new() { ["Largura Máx. Bobina"] = "800 mm", ["Espessura"] = "0.5 – 4.0 mm", ["Nº de Estágios"] = "16 estações", ["Velocidade Linha"] = "30 m/min", ["Motor Principal"] = "45 kW", ["Desbobinador"] = "Cap. 10 ton", ["Corte"] = "Cisalhamento volante", ["Controle"] = "Allen-Bradley CompactLogix" }
            },
            Alerts = new() { new AssetAlert { Id = "rc1", Sev = "warn", Msg = "Vibração nos mancais 6% acima do baseline", Detail = "RMS: 2.55g / Baseline: 2.4g. Monitorar tendência nas próximas 4h.", Asset = "Rolo Compressor RC-04", Time = "08:30", Ts = NowMinus(2400000) } }
        },
        new Asset
        {
            Id = "MC", Name = "Máquina de Corte MC-05", Type = "Serra Automática CNC", Img = "img/ativos/maquina_de_corte.jpg",
            Status = "running", Oee = 87, Avail = 93, Perf = 92, Qual = 99, Health = 84,
            VibBase = 1.8, TempBase = 42, Rms = 1.85, Bearing = 43, Power = 18, Rpm = 2200,
            Process = new() { "Fixação do Material", "Avanço da Serra", "Corte", "Retorno/Descarga" },
            CurrentState = 2, Cycles = 4567,
            Components = new() { ["disco_serra"] = 80, ["mandril"] = 88, ["motor"] = 90, ["guias_lineares"] = 85, ["refrigeracao"] = 82 },
            Spec = new AssetSpec
            {
                Modelo = "KASTOmicut E 4.6", Fabricante = "KASTO Maschinenbau", Serial = "KME46-BR-2024-0218",
                Aquisicao = "02/01/2024", Garantia = "02/01/2027",
                Descricao = "Serra circular automática CNC para corte de barras, tubos e perfis metálicos. Opera com avanço automático e sistema de medição integrado para cortes em série com alta precisão.",
                Atividades = new() { "Corte de barras redondas e quadradas", "Corte de tubos de aço", "Corte de perfis estruturais", "Produção de blanks para estampagem" },
                Specs = new() { ["Capacidade Redondo"] = "Ø 460 mm", ["Capacidade Quadrado"] = "400 × 400 mm", ["Disco Serra"] = "Ø 520 mm (HSS-DMo5)", ["Rotação"] = "20 – 120 RPM (variável)", ["Avanço"] = "Hidráulico CNC", ["Alimentador"] = "Rolo motorizado 6m", ["Refrigeração"] = "Emulsão semi-sintética", ["Controle"] = "KASTO EasyControl" }
            },
            Alerts = new() { new AssetAlert { Id = "mc1", Sev = "info", Msg = "Desgaste do disco de serra: 62% vida útil restante", Detail = "Próxima troca estimada em ~1.200 ciclos.", Asset = "Máquina de Corte MC-05", Time = "07:45", Ts = NowMinus(5400000) } }
        },
        new Asset
        {
            Id = "MR", Name = "Manipulador Robótico MR-06", Type = "Robô Articulado 6 Eixos", Img = "img/ativos/manipulador_robotico.jpg",
            Status = "running", Oee = 94, Avail = 98, Perf = 96, Qual = 99, Health = 93,
            VibBase = 0.4, TempBase = 35, Rms = 0.42, Bearing = 36, Power = 12, Rpm = 3000,
            Process = new() { "Posicionamento", "Soldagem MIG/TIG", "Verificação de Junta", "Reset/Próxima Peça" },
            CurrentState = 1, Cycles = 12340,
            Components = new() { ["servomotores"] = 95, ["redutores"] = 91, ["tocha_solda"] = 88, ["sensor_forca"] = 94, ["controlador"] = 97 },
            Spec = new AssetSpec
            {
                Modelo = "IRB 6700-200/2.60", Fabricante = "ABB Robotics", Serial = "IRB6700-BR-2024-1105",
                Aquisicao = "18/07/2024", Garantia = "18/07/2027",
                Descricao = "Robô industrial articulado de 6 eixos para soldagem MIG/TIG automatizada. Equipado com tocha de soldagem e sistema de visão para verificação de juntas em tempo real. Atua na célula de montagem de subconjuntos metálicos.",
                Atividades = new() { "Soldagem MIG de componentes estruturais", "Soldagem TIG de peças críticas", "Montagem automatizada de subconjuntos", "Inspeção de juntas por visão computacional" },
                Specs = new() { ["Carga Máxima"] = "200 kg", ["Alcance"] = "2.600 mm", ["Eixos"] = "6 (articulado)", ["Repetibilidade"] = "± 0.05 mm", ["Tocha"] = "Fronius TPS 500i", ["Sensor Visão"] = "Cognex IS7802", ["Controlador"] = "ABB OmniCore C90XT", ["IP"] = "IP67 (punho)" }
            },
            Alerts = new() { new AssetAlert { Id = "mr1", Sev = "info", Msg = "Calibração periódica OK — desvio < 0.02mm", Detail = "Precisão dentro da tolerância. Próxima calibração em 30 dias.", Asset = "Manipulador Robótico MR-06", Time = "06:30", Ts = NowMinus(9000000) } }
        }
    };

    private static List<Prescription> BuildPrescriptions() => new()
    {
        new Prescription { Id = "p1", Title = "Verificar vedações cilindro — Prensa PH-02", Asset = "Prensa Hidráulica PH-02", Priority = "crit", Status = "pending", Body = "Inspecionar vedações do cilindro hidráulico. Verificar pressão do sistema e nível de óleo. Testar trocador de calor. Prazo: 1 hora.", Created = "09:05" },
        new Prescription { Id = "p2", Title = "Monitorar mancais — Rolo Compressor RC-04", Asset = "Rolo Compressor RC-04", Priority = "warn", Status = "pending", Body = "Acompanhar tendência de vibração nos mancais. Se RMS ultrapassar 2.8g, programar relubrificação. Prazo: 4 horas.", Created = "08:35" },
        new Prescription { Id = "p3", Title = "Troca de disco — Máquina de Corte MC-05", Asset = "Máquina de Corte MC-05", Priority = "info", Status = "pending", Body = "Programar troca do disco de serra quando atingir 40% de vida útil restante (~1.200 ciclos). Estoque de discos: 3 unidades.", Created = "07:50" },
        new Prescription { Id = "p4", Title = "Calibração robô — Manipulador MR-06", Asset = "Manipulador Robótico MR-06", Priority = "info", Status = "done", Body = "Calibração realizada. Desvio < 0.02mm. Precisão nominal confirmada.", Created = "06:30" },
        new Prescription { Id = "p5", Title = "Inspeção NR-13 — Caldeira CA-03", Asset = "Caldeira Industrial CA-03", Priority = "info", Status = "done", Body = "Inspeção periódica concluída. Todos os itens aprovados. Próxima em 12 meses.", Created = "05:00" },
        new Prescription { Id = "p6", Title = "Limpeza óptica — Corte Laser CL-01", Asset = "Corte Laser CL-01", Priority = "warn", Status = "done", Body = "Limpeza das lentes e espelhos do sistema óptico realizada. Potência de saída normalizada.", Created = "04:30" }
    };

    private static List<MaintenanceRecord> BuildMaintenanceLog() => new()
    {
        new MaintenanceRecord { Id = "m1", Date = "09/04", Time = "06:00", Asset = "Prensa Hidráulica PH-02", Type = "preventive", Desc = "Troca de óleo hidráulico ISO VG 46 e filtros", Status = "done", Tech = "Carlos M." },
        new MaintenanceRecord { Id = "m2", Date = "08/04", Time = "14:30", Asset = "Corte Laser CL-01", Type = "predictive", Desc = "Substituição de lente focalizadora — degradação detectada via FFT", Status = "done", Tech = "Ana R." },
        new MaintenanceRecord { Id = "m3", Date = "08/04", Time = "08:00", Asset = "Caldeira Industrial CA-03", Type = "preventive", Desc = "Inspeção de tubulações e válvulas de segurança (NR-13)", Status = "done", Tech = "João P." },
        new MaintenanceRecord { Id = "m4", Date = "07/04", Time = "22:15", Asset = "Manipulador Robótico MR-06", Type = "corrective", Desc = "Recalibração de encoder do eixo 3 — deriva detectada", Status = "done", Tech = "Pedro S." },
        new MaintenanceRecord { Id = "m5", Date = "07/04", Time = "10:00", Asset = "Rolo Compressor RC-04", Type = "predictive", Desc = "Relubrificação de mancais — vibração acima do baseline", Status = "done", Tech = "Carlos M." },
        new MaintenanceRecord { Id = "m6", Date = "09/04", Time = "14:00", Asset = "Máquina de Corte MC-05", Type = "preventive", Desc = "Troca de disco de serra e alinhamento de guias lineares", Status = "pending", Tech = "Ana R." },
        new MaintenanceRecord { Id = "m7", Date = "10/04", Time = "06:00", Asset = "Prensa Hidráulica PH-02", Type = "predictive", Desc = "Inspeção de vedações do cilindro — temperatura de óleo elevada", Status = "scheduled", Tech = "João P." }
    };

    private static long NowMinus(long milliseconds) => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - milliseconds;
}
