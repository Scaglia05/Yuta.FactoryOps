namespace Yuta.FactoryOps.Client.FactoryOps;

public class AssetSpec
{
    public string Modelo { get; set; } = string.Empty;
    public string Fabricante { get; set; } = string.Empty;
    public string Serial { get; set; } = string.Empty;
    public string Aquisicao { get; set; } = string.Empty;
    public string Garantia { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public List<string> Atividades { get; set; } = new();
    public Dictionary<string, string> Specs { get; set; } = new();
}

public class AssetAlert
{
    public string Id { get; set; } = string.Empty;
    public string Sev { get; set; } = "info"; // crit | warn | info | ok
    public string Msg { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Asset { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public long Ts { get; set; }
}

public class Asset
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Img { get; set; } = string.Empty;
    public string Status { get; set; } = "running"; // running | warning | fault
    public int Oee { get; set; }
    public int Avail { get; set; }
    public int Perf { get; set; }
    public int Qual { get; set; }
    public int Health { get; set; }
    public double VibBase { get; set; }
    public double TempBase { get; set; }
    public double Rms { get; set; }
    public double Bearing { get; set; }
    public int Power { get; set; }
    public int Rpm { get; set; }
    public List<string> Process { get; set; } = new();
    public int CurrentState { get; set; }
    public long Cycles { get; set; }
    public Dictionary<string, int> Components { get; set; } = new();
    public AssetSpec Spec { get; set; } = new();
    public List<AssetAlert> Alerts { get; set; } = new();
}

public class Prescription
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Asset { get; set; } = string.Empty;
    public string Priority { get; set; } = "info"; // crit | warn | info
    public string Status { get; set; } = "pending"; // pending | done
    public string Body { get; set; } = string.Empty;
    public string Created { get; set; } = string.Empty;
}

public class MaintenanceRecord
{
    public string Id { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Asset { get; set; } = string.Empty;
    public string Type { get; set; } = "preventive"; // preventive | corrective | predictive
    public string Desc { get; set; } = string.Empty;
    public string Status { get; set; } = "done"; // done | pending | scheduled
    public string Tech { get; set; } = string.Empty;
}

public class VibThreshold
{
    public double Warn { get; set; }
    public double Crit { get; set; }
}

public class TempThreshold
{
    public double Warn { get; set; }
    public double Crit { get; set; }
}

public class Thresholds
{
    public VibThreshold Vib { get; set; } = new();
    public TempThreshold Temp { get; set; } = new();
}

public class AcqMode
{
    public string Label { get; set; } = string.Empty;
    public string Sub { get; set; } = string.Empty;
    public bool Active { get; set; }
}

public class FaultType
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}
