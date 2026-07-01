# Guia Completo de Configuração de Autenticação Social

## 🎯 Por que os botões não funcionam?

Os botões de login com Google e Microsoft não funcionam porque as credenciais de API estão configuradas como placeholders no arquivo `appsettings.json`.

## 🔧 Como Configurar

### Passo 1: Configurar Google OAuth 2.0

1. **Acesse o Google Cloud Console**
   - URL: https://console.cloud.google.com/
   - Faça login com sua conta Google

2. **Crie um Projeto**
   - Clique em "Select a project" > "NEW PROJECT"
   - Nome: "Yuta FactoryOps"
   - Clique em "CREATE"

3. **Ative as APIs Necessárias**
   - Vá para "APIs & Services" > "Library"
   - Busque por "Google+ API"
   - Clique em "ENABLE"

4. **Configure OAuth 2.0**
   - Vá para "APIs & Services" > "Credentials"
   - Clique em "Create Credentials" > "OAuth client ID"
   - **Application type**: Web application
   - **Name**: Yuta FactoryOps Web

5. **Configure as URIs de Redirecionamento**
   Adicione as seguintes URIs:
   - `https://localhost:7183/signin-google`
   - `https://localhost:5227/signin-google`
   - Para produção: `https://seu-dominio.com/signin-google`

6. **Copie as Credenciais**
   - **Client ID**: Copie o código (ex: `123456789-abcdefghijklmnop.apps.googleusercontent.com`)
   - **Client Secret**: Clique no ícone de olho para revelar e copie

### Passo 2: Configurar Microsoft Azure AD

1. **Acesse o Azure Portal**
   - URL: https://portal.azure.com/
   - Faça login com sua conta Microsoft

2. **Crie um App Registration**
   - Vá para "Azure Active Directory" > "App registrations"
   - Clique em "New registration"
   - **Name**: Yuta FactoryOps Web
   - **Supported account types**: "Accounts in any organizational directory and personal Microsoft accounts"

3. **Configure Redirect URIs**
   - Em "Redirect URI", selecione "Web"
   - Adicione as seguintes URIs:
   - `https://localhost:7183/signin-microsoft`
   - `https://localhost:5227/signin-microsoft`
   - Para produção: `https://seu-dominio.com/signin-microsoft`

4. **Copie as Credenciais**
   - **Application (client) ID**: Copie da página "Overview"
   - **Client Secret**:
     - Vá para "Certificates & secrets" > "New client secret"
     - Descrição: "Yuta FactoryOps"
     - Clique em "Add"
     - **IMPORTANTE**: Copie o valor do secret imediatamente (só aparece uma vez)

### Passo 3: Atualizar appsettings.json

No arquivo `D:\YutaFactoryOpsWeb\Yuta.FactoryOps\Yuta.FactoryOps.Server\appsettings.json`, substitua:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=aws-1-us-east-1.pooler.supabase.com;Database=postgres;Username=postgres.hyyxfprhtsopmmbvvcug;Password=FactoryOps@5432;SSL Mode=Require;Trust Server Certificate=true"
  },
  "Authentication": {
    "Google": {
      "ClientId": "123456789-abcdefghijklmnop.apps.googleusercontent.com",
      "ClientSecret": "GOCSPX-SEU_GOOGLE_SECRET_REAL"
    },
    "Microsoft": {
      "ClientId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "ClientSecret": "SEU_MICROSOFT_SECRET_REAL"
    }
  },
  "Jwt": {
    "ChaveSecreta": "SuaChaveSuperSecretaComMaisDe32CaracteresYutaOps"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Passo 4: Reinciar o Servidor

Após atualizar o `appsettings.json`, reinicie o servidor:
```bash
# Pare o servidor atual (Ctrl+C no terminal)
# Execute novamente:
dotnet run --project "D:\YutaFactoryOpsWeb\Yuta.FactoryOps\Yuta.FactoryOps.Server\Yuta.FactoryOps.Server.csproj" --launch-profile https
```

### Passo 5: Reativar os Botões

No código, descomente as linhas que foram comentadas:

**No Login.razor:**
```csharp
private void LoginGoogle()
{
    // Remova a mensagem de teste e descomente:
    Navigation.NavigateTo("/api/externalauth/google", forceLoad: true);
}

private void LoginMicrosoft()
{
    // Remova a mensagem de teste e descomente:
    Navigation.NavigateTo("/api/externalauth/microsoft", forceLoad: true);
}
```

**No Registro.razor:**
```csharp
private void LoginGoogle()
{
    Navigation.NavigateTo("/api/externalauth/google", forceLoad: true);
}

private void LoginMicrosoft()
{
    Navigation.NavigateTo("/api/externalauth/microsoft", forceLoad: true);
}
```

## 🧪 Testes

### Teste 1: Google
1. Clique no botão "Google"
2. Você será redirecionado para a página de login do Google
3. Faça login com sua conta Google
4. Será redirecionado de volta para o sistema
5. Se não tiver conta, será redirecionado para criar uma

### Teste 2: Microsoft
1. Clique no botão "Microsoft"
2. Você será redirecionado para a página de login da Microsoft
3. Faça login com sua conta Microsoft
4. Será redirecionado de volta para o sistema
5. Se não tiver conta, será redirecionado para criar uma

## ⚠️ Solução de Problemas Comuns

### Erro "redirect_uri_mismatch"
- **Causa**: As URIs de redirecionamento não estão configuradas corretamente
- **Solução**: Verifique se as URIs no console do provedor correspondem exatamente às configuradas no appsettings.json

### Erro "invalid_client"
- **Causa**: Client ID ou Client Secret incorretos
- **Solução**: Verifique se as credenciais no appsettings.json estão corretas

### Erro "consent_required"
- **Causa**: O app precisa ser verificado pelo Google/Microsoft
- **Solução**: Verifique se o app está configurado corretamente no console do provedor

### Erro "access_denied"
- **Causa**: Permissões insuficientes ou app não configurado
- **Solução**: Verifique as permissões configuradas no console do provedor

## 🎁 Benefícios da Autenticação Social

- ✅ Segurança: Senhas gerenciadas pelos provedores
- ✅ UX: Experiência de login familiar para usuários
- ✅ Redução de fricção: Menos usuários esquecem senhas
- ✅ Acesso a dados: Pode acessar foto, nome, email automaticamente
- ✅ Single Sign-On: Usuários logados em outros serviços

## 🚀 Após Configuração

Com as credenciais configuradas:
- Login com Google: Funciona perfeitamente
- Login com Microsoft: Funciona perfeitamente
- Registro via auth social: Campos pré-preenchidos automaticamente
- Login automático: Se o usuário já existir, loga automaticamente

## 📱 Ambiente de Produção

Para produção, configure:
1. URIs de redirecionamento com seu domínio real
2. Verifique se os apps estão publicados/verificados
3. Configure HTTPS obrigatório
4. Use variáveis de ambiente para credenciais (não commitar secrets no git)