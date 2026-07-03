# Configuração de Ambiente (segredos e deploy)

## ⚠️ Ação urgente: rotacionar a senha do Supabase

O `appsettings.json` tinha a senha real do banco Supabase gravada em texto puro e commitada no Git (`Password=FactoryOps@5432`, usuário `postgres.hyyxfprhtsopmmbvvcug`). Essa senha já foi removida dos arquivos, **mas ela continua no histórico do Git** e deve ser considerada comprometida.

Antes de colocar isso em produção:
1. Acesse o painel do Supabase → **Project Settings → Database**.
2. Clique em **Reset database password** e gere uma nova senha.
3. Atualize a connection string em todos os lugares (user-secrets local, variável de ambiente no Render) com a nova senha.

## Variáveis necessárias

| Variável | Obrigatória | Descrição |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Sim | String de conexão do Supabase (Postgres via pooler) |
| `Jwt__ChaveSecreta` | Sim (produção) | Chave usada para assinar os tokens JWT. Mínimo 32 caracteres aleatórios. Em desenvolvimento, se não configurada, o app usa uma chave fixa de dev e avisa nos logs |
| `Authentication__Google__ClientId` / `Authentication__Google__ClientSecret` | Não | Só necessário se for ativar login com Google. Sem isso, a rota de login social nem é registrada |
| `Authentication__Microsoft__ClientId` / `Authentication__Microsoft__ClientSecret` | Não | Mesma lógica, para Microsoft |
| `PORT` | Automática no Render | O Render define essa variável sozinho; o app já lê ela para escutar na porta certa |

O separador `__` (duplo underscore) mapeia para as seções do `appsettings.json` (ex.: `Jwt__ChaveSecreta` = `Jwt:ChaveSecreta`). É a convenção padrão do ASP.NET Core para configuração via variável de ambiente.

## Desenvolvimento local (User Secrets)

Não coloque segredos em `appsettings.Development.json` (esse arquivo é versionado). Use o gerenciador de User Secrets do .NET, que grava fora da pasta do projeto:

```bash
cd Yuta.FactoryOps.Server
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=aws-1-us-east-1.pooler.supabase.com;Database=postgres;Username=postgres.SEU_USUARIO;Password=SUA_SENHA_NOVA;SSL Mode=Require;Trust Server Certificate=true"
dotnet user-secrets set "Jwt:ChaveSecreta" "gere-uma-string-aleatoria-de-32+-caracteres"
```

Depois disso, `dotnet run --project Yuta.FactoryOps.Server` já lê esses valores automaticamente.

## Deploy no Render

O `render.yaml` na raiz do projeto já define o serviço web (build via `Dockerfile`) e os placeholders das variáveis de ambiente. No primeiro deploy (ou em **Environment** no dashboard do serviço), preencha manualmente:

- `ConnectionStrings__DefaultConnection` — a connection string do Supabase (com a senha rotacionada)
- `Authentication__Google__*` / `Authentication__Microsoft__*` — só se for usar login social

`Jwt__ChaveSecreta` é gerada automaticamente pelo Render (`generateValue: true` no blueprint) — não precisa configurar.

Depois de configurar as variáveis, cada deploy roda `dotnet Yuta.FactoryOps.Server.dll` dentro do container; ao subir, o app aplica as migrations pendentes no Supabase e cria o usuário admin padrão automaticamente (ver `DatabaseSeeder`).

## Login padrão (após o seed rodar com sucesso)

```
Email: admin@yuta.com
Senha: Admin@123
```

Troque essa senha (ou crie outro usuário Admin e desative este) assim que tiver acesso — ela está documentada aqui e no código-fonte, então não é segura para produção de longo prazo.
