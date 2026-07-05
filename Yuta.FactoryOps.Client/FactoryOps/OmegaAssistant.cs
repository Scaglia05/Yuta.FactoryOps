using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Yuta.FactoryOps.Client.FactoryOps;

/// <summary>
/// Motor de respostas do "Agente Omega" (chat lateral). Baseado em regras locais (sem LLM real),
/// espelhando o protótipo em JS — respostas são geradas a partir do estado atual do ativo selecionado.
/// </summary>
public static class OmegaAssistant
{
    public static readonly string[] Presets = { "Diagnóstico atual", "Causa-raiz do alerta", "Ações imediatas", "Estimar RUL", "Impacto no OEE" };

    private static string Normalize(string text)
    {
        var lower = text.ToLowerInvariant();
        var formD = lower.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in formD)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private static bool Matches(string q, string pattern) => Regex.IsMatch(q, pattern);

    public static string GetResponse(string text, Asset a, Thresholds thresholds)
    {
        var q = Normalize(text);
        bool vibWarn = a.Rms > thresholds.Vib.Warn;
        bool vibCrit = a.Rms > thresholds.Vib.Crit;
        bool tempWarn = a.Bearing > thresholds.Temp.Warn;
        bool tempCrit = a.Bearing > thresholds.Temp.Crit;
        string oeeStatus = a.Oee >= 80 ? "dentro da meta" : a.Oee >= 65 ? "abaixo da meta" : "crítico — intervenção necessária";
        string healthStatus = a.Health >= 80 ? "boa" : a.Health >= 55 ? "degradada" : "crítica";
        int vibDelta = (int)Math.Round((a.Rms - a.VibBase) / a.VibBase * 100);
        double tempDelta = Math.Round(a.Bearing - a.TempBase, 1);
        var worstComp = a.Components.OrderBy(kv => kv.Value).First();
        string worstLabel = worstComp.Key.Replace('_', ' ');
        var activeAlerts = a.Alerts.Where(al => al.Sev is "crit" or "warn").ToList();
        int rul = (int)Math.Round(a.Health * 3.2);

        if (Matches(q, "^(ola|oi|hey|hello|bom dia|boa tarde|boa noite|tudo bem|tudo bom|como vai|e ai|salve)"))
        {
            var hora = DateTime.Now.Hour;
            var saudacao = hora < 12 ? "Bom dia" : hora < 18 ? "Boa tarde" : "Boa noite";
            var healthColor = a.Health >= 80 ? "var(--green2)" : a.Health >= 55 ? "var(--gold2)" : "var(--red2)";
            return $"👋 <b>{saudacao}!</b> Sou o <b>Agente Omega</b>, assistente de manutenção preditiva da FactoryOps.<br><br>Estou monitorando <b>6 ativos</b> da Linha 1 — MetalForm Indústria, Campinas.<br><br>No momento o ativo selecionado é <b>{a.Name}</b> com saúde em <b style=\"color:{healthColor}\">{a.Health}%</b>.<br><br>Como posso ajudar? Pergunte sobre diagnóstico, vibração, temperatura, OEE ou manutenção. 🔧";
        }

        if (Matches(q, "fabrica|fabr|empresa|metalform|industria|campinas|quem e|onde fica|cnpj|segmento|negocio"))
        {
            return "🏭 <b>MetalForm Indústria Ltda.</b><br><br>📍 Campinas, SP — CNPJ: 12.345.678/0001-90<br>🔩 Segmento: <b>Metalurgia / Peças Estampadas</b><br>📦 Capacidade: <b>450 ton/mês</b><br>🕐 Operação: <b>3 turnos (24h)</b><br>👥 Colaboradores: <b>182</b><br>📋 Certificações: <b>ISO 9001 / ISO 14001</b><br>🚗 Clientes: <b>Automotivo e Linha Branca</b><br><br>A fábrica opera a <b>Linha 1</b> com 6 ativos monitorados em tempo real pelo sistema FactoryOps + Omega Sense v2.";
        }

        if (Matches(q, "processo|fabricacao|como funciona|como e feito|etapa|fluxo|linha|como produz|o que produz|o que faz|faz aqui"))
        {
            return "⚙️ <b>Processo de Fabricação — Linha 1</b><br><br><b>1.</b> 📦 <b>Recebimento</b> — Bobinas e barras de aço inspecionadas<br><b>2.</b> ✂️ <b>Corte a Laser</b> — Chapas em geometria CNC <i>(Corte Laser CL-01)</i><br><b>3.</b> 🪚 <b>Corte de Perfis</b> — Barras e tubos por serra CNC <i>(Máquina de Corte MC-05)</i><br><b>4.</b> 🗜️ <b>Conformação</b> — Laminação e prensagem 200t <i>(Prensa PH-02 + Rolo RC-04)</i><br><b>5.</b> 🔥 <b>Soldagem</b> — Montagem robotizada MIG/TIG <i>(Manipulador MR-06)</i><br><b>6.</b> ♨️ <b>Tratamento Térmico</b> — Vapor 15 t/h <i>(Caldeira CA-03)</i><br><b>7.</b> 🎨 <b>Acabamento</b> — Pintura eletrostática e revestimento<br><b>8.</b> 🔍 <b>Inspeção</b> — Controle dimensional e ensaios não destrutivos";
        }

        if (Matches(q, "sistema|factoryops|omega sense|software|plataforma|o que e isso|para que serve|yuta|aplicativo"))
        {
            return "📱 <b>FactoryOps — Omega Sense v2</b><br><br>Plataforma de <b>manutenção preditiva e prescritiva</b> desenvolvida pela <b>Yuta Prevent</b>.<br><br>🔬 <b>O que monitora:</b><br>• Vibração RMS e espectro FFT (0–12.800 Hz)<br>• Temperatura de rolamentos e mancais<br>• OEE: Disponibilidade, Desempenho e Qualidade<br>• Saúde por componente de cada ativo<br><br>📡 <b>Sensor Omega Sense v2:</b><br>• Acelerômetro triaxial MEMS ±16g | IP67<br>• Mesh 915 MHz | Bateria até 5 anos<br><br>🤖 O <b>Agente Omega</b> gera diagnósticos e prescrições automáticas em tempo real.";
        }

        if (Matches(q, "ativo|maquina|equipamento|quais sao|lista|quantos ativos|que maquinas|linha 1"))
        {
            return "🏗️ <b>Ativos Monitorados — Linha 1</b><br><br><b>1.</b> ✂️ <b>Corte Laser CL-01</b> — TruLaser 3030 Fiber 4kW (TRUMPF)<br><b>2.</b> 🗜️ <b>Prensa Hidráulica PH-02</b> — HPX-200T 200 ton (Schuler AG)<br><b>3.</b> ♨️ <b>Caldeira Industrial CA-03</b> — ATA FT-15 Flamotubular 15 t/h<br><b>4.</b> 🔄 <b>Rolo Compressor RC-04</b> — RF-800 Multi-Stage (Gasparini)<br><b>5.</b> 🪚 <b>Máquina de Corte MC-05</b> — KASTOmicut E 4.6 Serra CNC<br><b>6.</b> 🤖 <b>Manipulador Robótico MR-06</b> — IRB 6700-200/2.60 (ABB)<br><br>Clique em qualquer ativo na barra lateral para monitorar em detalhes. 👈";
        }

        if (Matches(q, "obrigad|valeu|thanks|agradeço|perfeito|otimo|excelente|legal|show|gostei"))
        {
            return "😊 Disponha! Estou aqui 24/7 monitorando a Linha 1. Se precisar de mais análises, é só perguntar. Bom turno! 🔧";
        }

        if (Matches(q, "diagnostico|diagn|situacao|situação|status|como esta|como está|estado atual|resumo"))
        {
            string vibLine = vibCrit
                ? $"⚠️ Vibração em <b>{a.Rms:0.00} g</b> — <b>nível CRÍTICO</b> ({vibDelta}% acima do baseline)"
                : vibWarn
                ? $"⚠️ Vibração em <b>{a.Rms:0.00} g</b> — acima do limiar de aviso"
                : $"✅ Vibração normal em <b>{a.Rms:0.00} g</b>";
            string tempLine = tempCrit
                ? $"🌡️ Temperatura em <b>{a.Bearing:0.0}°C</b> — <b>CRÍTICO</b> ({tempDelta}°C acima do baseline)"
                : tempWarn
                ? $"🌡️ Temperatura em <b>{a.Bearing:0.0}°C</b> — aviso de aquecimento"
                : $"✅ Temperatura normal em <b>{a.Bearing:0.0}°C</b>";
            string alertLine = activeAlerts.Count > 0 ? $"⚡ <b>{activeAlerts.Count} alerta(s) ativo(s)</b> — requer atenção." : "✅ Nenhum alerta crítico ativo no momento.";
            return $"📊 <b>Diagnóstico — {a.Name}</b><br><br>{vibLine}<br>{tempLine}<br>📈 OEE: <b>{a.Oee}%</b> — {oeeStatus}<br>🫀 Saúde geral: <b>{a.Health}%</b> ({healthStatus})<br>🔄 Ciclos realizados: <b>{a.Cycles:N0}</b><br>🔧 Componente mais crítico: <b>{worstLabel}</b> ({worstComp.Value}%)<br><br>{alertLine}";
        }

        if (Matches(q, "causa|raiz|porque|por que|motivo|origem|falha|problema"))
        {
            var causas = new List<string>();
            if (vibCrit || vibWarn) causas.Add($"vibração elevada (+{vibDelta}% do baseline) — possível desbalanceamento, folga mecânica ou desgaste de componente rotativo");
            if (tempCrit || tempWarn) causas.Add($"temperatura acima do nominal (+{tempDelta}°C) — provável degradação de lubrificação ou problema no sistema de resfriamento");
            if (a.Health < 60) causas.Add($"saúde geral comprometida ({a.Health}%) — múltiplos componentes com desgaste acumulado");
            if (causas.Count == 0) causas.Add("nenhuma anomalia significativa detectada pelos sensores no momento");
            var lista = string.Join("<br>", causas.Select((c, i) => $"{i + 1}. {c}"));
            return $"🔍 <b>Análise de Causa Provável — {a.Name}</b><br><br>Com base nos dados do sensor <b>YUT-{a.Id}01</b>:<br><br>{lista}<br><br>O componente com maior índice de degradação é o <b>{worstLabel}</b> (score {worstComp.Value}%). Recomendo análise espectral FFT para confirmar diagnóstico.";
        }

        if (Matches(q, "acao|ação|imediata|fazer|recomend|prescri|intervencao|intervenção"))
        {
            var acoes = new List<string>();
            if (vibCrit) acoes.Add("🔴 <b>URGENTE:</b> Paralisar operação e inspecionar componentes rotativos — vibração em nível crítico");
            else if (vibWarn) acoes.Add("🟡 Monitorar vibração a cada 30 min e programar inspeção no próximo turno");
            if (tempCrit) acoes.Add("🔴 <b>URGENTE:</b> Verificar sistema de lubrificação e resfriamento imediatamente");
            else if (tempWarn) acoes.Add("🟡 Checar nível e qualidade do lubrificante — temperatura em zona de aviso");
            if (a.Health < 60) acoes.Add($"🔴 Agendar manutenção corretiva para o componente <b>{worstLabel}</b>");
            if (acoes.Count == 0)
            {
                acoes.Add("✅ Manter monitoramento padrão — nenhuma ação imediata necessária");
                acoes.Add("📋 Seguir plano de manutenção preventiva conforme calendário");
            }
            string prazo = (vibCrit || tempCrit) ? "Imediato — próxima hora" : a.Status == "warning" ? "Até o fim do turno" : "Próxima janela de manutenção programada";
            return $"⚙️ <b>Ações Prescritivas — {a.Name}</b><br><br>{string.Join("<br>", acoes)}<br><br>⏱ <b>Prazo:</b> {prazo}";
        }

        if (Matches(q, "rul|vida util|vida útil|restante|quanto tempo|durabilidade|vida|duracao|duração"))
        {
            double taxa = Math.Round((100.0 - a.Health) / 12.0, 1);
            string obs = worstComp.Value < 65 ? "Planejar substituição ou revisão em breve." : "Monitorar evolução nas próximas semanas.";
            return $"⏱ <b>Estimativa de Vida Útil Restante (RUL) — {a.Name}</b><br><br>Com base nos indicadores atuais:<br><br>🫀 Saúde atual: <b>{a.Health}%</b><br>📉 Taxa de degradação estimada: <b>{taxa}% / mês</b><br>⏳ RUL estimado: <b>~{rul} dias</b> até atingir limiar crítico<br><br>⚠️ O componente <b>{worstLabel}</b> ({worstComp.Value}%) é o principal limitante. {obs}";
        }

        if (Matches(q, "oee|eficiencia|eficiência|disponibilidade|desempenho|qualidade|producao|produção|perdas"))
        {
            int oeeGap = 85 - a.Oee;
            string maiorPerda = a.Avail < a.Perf && a.Avail < a.Qual ? "Disponibilidade" : a.Perf < a.Qual ? "Desempenho" : "Qualidade";
            string gapLine = oeeGap > 0 ? $"📉 Gap de <b>{oeeGap}pp</b> em relação à meta — a maior perda está em <b>{maiorPerda}</b>." : "✅ OEE acima da meta. Manter condições operacionais atuais.";
            return $"📈 <b>Análise de OEE — {a.Name}</b><br><br>OEE atual: <b>{a.Oee}%</b> (meta: 85%)<br><br>• Disponibilidade: <b>{a.Avail}%</b> {(a.Avail < 90 ? "⚠️ abaixo da meta" : "✅")}<br>• Desempenho: <b>{a.Perf}%</b> {(a.Perf < 90 ? "⚠️ abaixo da meta" : "✅")}<br>• Qualidade: <b>{a.Qual}%</b> {(a.Qual < 95 ? "⚠️" : "✅")}<br><br>{gapLine}";
        }

        if (Matches(q, "vibracao|vibração|rms|vibra|oscila|tremor|batimento"))
        {
            string concl = vibCrit ? "🔴 <b>NÍVEL CRÍTICO</b> — Parada para inspeção recomendada. Verificar rolamentos, acoplamentos e balanceamento." : vibWarn ? "🟡 <b>Nível de aviso</b> — Monitorar tendência e planejar inspeção." : "✅ <b>Vibração dentro dos parâmetros normais.</b> Continuar monitoramento padrão.";
            return $"〰️ <b>Análise de Vibração — {a.Name}</b><br><br>RMS atual: <b>{a.Rms:0.000} g</b><br>Baseline: <b>{a.VibBase} g</b> | Δ: <b>{(vibDelta > 0 ? "+" : "")}{vibDelta}%</b><br>Limiar aviso: {thresholds.Vib.Warn}g | Limiar crítico: {thresholds.Vib.Crit}g<br><br>{concl}";
        }

        if (Matches(q, "temperatura|temp|calor|aqueciment|thermal|termic"))
        {
            string concl = tempCrit ? "🔴 <b>TEMPERATURA CRÍTICA</b> — Verificar lubrificação, ventilação e trocador de calor urgentemente." : tempWarn ? "🟡 <b>Aviso de temperatura</b> — Checar lubrificante e sistema de resfriamento." : "✅ <b>Temperatura normal.</b> Continuar monitoramento padrão.";
            return $"🌡️ <b>Análise de Temperatura — {a.Name}</b><br><br>Temp. rolamento: <b>{a.Bearing:0.0}°C</b><br>Baseline: <b>{a.TempBase}°C</b> | Δ: <b>{(tempDelta > 0 ? "+" : "")}{tempDelta}°C</b><br>Limiar aviso: {thresholds.Temp.Warn}°C | Limiar crítico: {thresholds.Temp.Crit}°C<br><br>{concl}";
        }

        if (Matches(q, "manutencao|manutenção|preventiva|preditiva|trocar|troca|lubrific|inspe|agend"))
        {
            var sb = new StringBuilder();
            if (a.Health < 60) sb.Append("🔴 Manutenção <b>corretiva urgente</b> recomendada — não aguardar próxima janela programada.<br>");
            if (a.Health < 80) sb.Append($"🟡 <b>Preditiva:</b> Planejar revisão do componente <b>{worstLabel}</b> na próxima janela disponível.<br>");
            sb.Append("📋 <b>Preventiva:</b> Seguir plano de lubrificação e inspeção conforme calendário do fabricante.<br>");
            sb.Append("📡 <b>Preditiva contínua:</b> Sensor Omega Sense v2 monitorando vibração e temperatura 24/7.<br><br>");
            int proxima = rul > 30 ? Math.Min(rul, 90) : rul;
            sb.Append($"⏳ Próxima intervenção estimada: <b>~{proxima} dias</b>");
            return $"🔧 <b>Plano de Manutenção — {a.Name}</b><br><br>Com base no estado atual (saúde {a.Health}%, {a.Cycles:N0} ciclos):<br><br>{sb}";
        }

        if (Matches(q, "sensor|omega|mesh|rede|gateway|sinal|bateria|comunicacao|comunicação|conectividade"))
        {
            return $"📡 <b>Status do Sensor — YUT-{a.Id}01 (Omega Sense v2)</b><br><br>✅ Conectado via rede Mesh 915 MHz<br>✅ Gateway: YUT-GW-01 online<br>📶 RSSI: -62 dBm (sinal adequado)<br>🔋 Bateria: Li-SOCl2 3.6V — estimativa &gt;4 anos restantes<br>🔄 Intervalo de sincronização: 1.2s<br>🌡️ Temperatura interna do sensor: 32°C<br><br>Modo ativo: <b>Contínuo (baixa freq.) + Pontual (alta freq.)</b><br>Último dado recebido: há <b>&lt;2 segundos</b>";
        }

        if (Matches(q, "ajuda|help|o que voce|o que você|capacidade|funciona|comando|pode fazer|posso perguntar"))
        {
            return "🤖 <b>Agente Omega — FactoryOps</b><br><br>Olá! Sou especialista em análise de ativos industriais. Posso responder perguntas como:<br><br>• <b>\"Diagnóstico atual\"</b> — resumo completo do ativo<br>• <b>\"Causa-raiz do alerta\"</b> — análise de anomalias<br>• <b>\"Ações imediatas\"</b> — o que fazer agora<br>• <b>\"Estimar RUL\"</b> — vida útil restante<br>• <b>\"Impacto no OEE\"</b> — análise de eficiência<br>• <b>\"Vibração\"</b> ou <b>\"Temperatura\"</b> — leituras detalhadas<br>• <b>\"Manutenção\"</b> — plano e recomendações<br>• <b>\"Sensor\"</b> — status da rede e conectividade<br><br>Use os botões de atalho acima para respostas rápidas! 👆";
        }

        return $"🤖 <b>Agente Omega</b><br><br>Analisei sua pergunta sobre <b>{a.Name}</b>.<br><br>Dados atuais do ativo:<br>• Vibração: <b>{a.Rms:0.00} g</b> {(vibCrit ? "🔴" : vibWarn ? "🟡" : "✅")}<br>• Temperatura: <b>{a.Bearing:0.0}°C</b> {(tempCrit ? "🔴" : tempWarn ? "🟡" : "✅")}<br>• Saúde: <b>{a.Health}%</b> | OEE: <b>{a.Oee}%</b><br><br>Para diagnósticos específicos, use os atalhos rápidos ou pergunte sobre: <b>vibração, temperatura, OEE, manutenção, RUL</b> ou <b>diagnóstico atual</b>.";
    }
}
