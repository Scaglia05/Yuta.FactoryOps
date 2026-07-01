# Configuração de Autenticação Social (Google e Microsoft)

## Visão Geral
O sistema agora suporta autenticação social via Google e Microsoft, permitindo que os usuários entrem ou criem contas usando suas credenciais existentes.

## Fluxo de Autenticação

### 1. Login com Google/Microsoft
- Usuário clica no botão de login social
- Redirecionado para a página de autenticação do provedor
- Após autenticação bem-sucedida:
  - Se o usuário já existe: login automático
  - Se o usuário não existe: redirecionado para a página de registro com dados pré-preenchidos

### 2. Registro com Google/Microsoft
- Usuário é redirecionado para a página de registro após autenticação social
- Campos de email e nome são pré-preenchidos (somente leitura)
- Usuário precisa fornecer apenas o ID da empresa
- Após confirmação, conta é criada e usuário é autenticado automaticamente

## Configuração das Credenciais

### Google OAuth 2.0

1. Acesse o [Google Cloud Console](https://console.cloud.google.com/)
2. Crie um novo projeto ou selecione um existente
3. Vá para "APIs & Services" > "Credentials"
4. Clique em "Create Credentials" > "OAuth client ID"
5. Configure:
   - **Application type**: Web application
   - **Authorized redirect URIs**: 
     - `https://seu-dominio.com/signin-google`
     - `http://localhost:7183/signin-google` (desenvolvimento)
6. Copie o Client ID e Client Secret
7. Atualize o arquivo `appsettings.json`:
   ```json
   "Authentication": {
     "Google": {
       "ClientId": "seu-google-client-id",
       "ClientSecret": "seu-google-client-secret"
     }
   }
   ```

### Microsoft Azure AD

1. Acesse o [Azure Portal](https://portal.azure.com/)
2. Vá para "Azure Active Directory" > "App registrations"
3. Clique em "New registration"
4. Configure:
   - **Name**: Nome do seu aplicativo
   - **Supported account types**: "Accounts in any organizational directory and personal Microsoft accounts"
   - **Redirect URI**: Web platform com URL:
     - `https://seu-dominio.com/signin-microsoft`
     - `http://localhost:7183/signin-microsoft` (desenvolvimento)
5. Após criar, copie o Application (client) ID
6. Vá para "Certificates & secrets" > "New client secret"
7. Copie o valor do secret (só é visível uma vez)
8. Atualize o arquivo `appsettings.json`:
   ```json
   "Authentication": {
     "Microsoft": {
       "ClientId": "seu-microsoft-client-id",
       "ClientSecret": "seu-microsoft-client-secret"
     }
   }
   ```

## Estrutura do Banco de Dados

A entidade `Usuario` foi atualizada para suportar autenticação externa:

```csharp
public class Usuario
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
    public string ProvedorAutenticacao { get; set; } = "Email"; // "Email", "Google", "Microsoft"
    public string? ProviderKey { get; set; } // ID único do provedor
    public bool EmailConfirmado { get; set; } = false;
    public string Role { get; set; } = "Operador";
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}
```

## Migração do Banco de Dados

Você precisará adicionar a coluna `ProviderKey` à tabela de usuários:

```sql
ALTER TABLE "Usuarios" ADD COLUMN "ProviderKey" text;
```

## Endpoints de Autenticação

### Login Social
- `GET /api/externalauth/google` - Inicia login com Google
- `GET /api/externalauth/microsoft` - Inicia login com Microsoft

### Callbacks
- `GET /api/externalauth/google-callback` - Callback do Google
- `GET /api/externalauth/microsoft-callback` - Callback da Microsoft

### Registro Externo
- `POST /api/externalauth/registro-externo` - Cria usuário com autenticação externa

## Páginas do Frontend

- `/login` - Página de login com botões sociais
- `/registro` - Página de registro (suporta registro normal e externo)

## Segurança

- Tokens JWT são gerados após autenticação bem-sucedida
- Email é automaticamente confirmado para usuários de autenticação social
- ProviderKey é usado para vincular contas externas
- SSL/TLS é obrigatório em produção

## Troubleshooting

### Erro "redirect_uri_mismatch"
- Verifique se as URIs de redirecionamento estão configuradas corretamente no console do provedor
- Certifique-se de que a URL do ambiente corresponde à configurada

### Erro "invalid_client"
- Verifique se o Client ID e Client Secret estão corretos no appsettings.json
- Certifique-se de que o app está registrado no console do provedor

### Usuário não é redirecionado após login
- Verifique se os endpoints de callback estão configurados corretamente
- Verifique os logs do servidor para erros de autenticação