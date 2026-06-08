# Yuta - FactoryOps 🏭🛡️

A plataforma **FactoryOps** é um ecossistema industrial de alta tecnologia projetado pela **YUTA LTDA** para unificar o monitoramento operacional de processos e a manutenção preditiva/prescritiva de ativos críticos.

A plataforma integra-se diretamente ao hardware **Yuta Omega Sense** (sensor inteligente de vibração triaxial e temperatura com TinyML integrado).

## 🚀 Funcionalidades Principais

- **Autenticação Segura & Login Social:** Integração nativa com Contas Google.
- **Arquitetura Multi-Tenancy:** Isolamento absoluto de dados em nível de banco de dados por empresa (Row Level Security).
- **Gestão de Sessão Industrial:** Monitor de inatividade com alertas flutuantes e logout automatizado para proteção em computadores de fábrica.
- **Dashboard de Ativos:** Visualização em tempo real de KPIs de processo, cálculo de OEE desagregado e análise espectral de sensores.
- **IA Prescritiva com RAG:** Diagnósticos automáticos de causa-raiz gerados por múltiplos agentes de IA (LLMs) fundamentados nos manuais técnicos dos ativos, mitigando alucinações.

## 🛠️ Stack Tecnológica

- **Frontend & UI:** C# Blazor (Interactive WebAssembly) + Tailwind CSS
- **Backend & APIs:** ASP.NET Core (.NET 8/9)
- **Banco de Dados & Autenticação:** PostgreSQL (Supabase Cloud Free Tier)

## 📋 Especificações de Hardware Suportadas (Yuta Omega Sense)

- **Faixa de Frequência de Análise:** 0 Hz a 32.000 Hz.
- **Aceleração Máxima:** Até 16g.
- **Velocidade Vibracional:** Até 100 mm/s RMS.
- **Protocolo de Rede:** Redes de sensores sem fio em malha (Mesh).

---

## ⚖️ Licença e Propriedade Intelectual

Este é um software proprietário e de uso exclusivo da **YUTA LTDA**. Todos os direitos reservados. É estritamente proibida a cópia, modificação ou distribuição não autorizada deste código-fonte conforme detalhado no arquivo `LICENSE`.