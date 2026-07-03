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

## ✅ Estado atual da implementação

As seções acima descrevem a visão completa do produto. O que está implementado e funcional hoje no código:

- Login por e-mail/senha (JWT), com usuário admin padrão criado automaticamente (`admin@yuta.com` / `Admin@123`) — veja `CONFIGURACAO_AMBIENTE.md`.
- CRUD de Usuários e Empresas (telas + API), protegido por login.
- Dashboard com métricas reais de usuários/empresas (não a análise de ativos/OEE/vibração descrita acima).
- Deploy via Docker/`render.yaml` para o Render, banco Postgres no Supabase.

Ainda **não** implementado (fazem parte da visão, não do estado atual): login social (Google/Microsoft — scaffolding pronto no backend, falta credenciais reais), Row Level Security no Postgres, monitoramento de ativos/sensores (Yuta Omega Sense), OEE, análise espectral e o módulo de IA prescritiva/RAG.

---

## ⚖️ Licença e Propriedade Intelectual

Este é um software proprietário e de uso exclusivo da **YUTA LTDA**. Todos os direitos reservados. É estritamente proibida a cópia, modificação ou distribuição não autorizada deste código-fonte conforme detalhado no arquivo `LICENSE`.