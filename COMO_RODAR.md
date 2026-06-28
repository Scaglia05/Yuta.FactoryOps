# 🚀 Como Rodar o Projeto Yuta.FactoryOps

## ⚡ Resumo Rápido

```bash
# Passo 1: Restaurar dependências
dotnet restore

# Passo 2: Rodar o projeto
dotnet run --project Yuta.FactoryOps.Server
```

A aplicação estará disponível em: `https://localhost:7183`

---

## 📋 Passo a Passo Detalhado

### 1. Pré-requisitos
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Acesso ao banco de dados Supabase (já configurado)

### 2. Restaurar Dependências
```bash
# Na raiz do projeto
dotnet restore
```

### 3. Build do Projeto
```bash
# Verificar se tudo compila corretamente
dotnet build
```

### 4. Rodar a Aplicação
```bash
# Opção 1: Na raiz do projeto
dotnet run --project Yuta.FactoryOps.Server

# Opção 2: Entrar na pasta do Server
cd Yuta.FactoryOps.Server
dotnet run
```

### 5. Acessar a Aplicação
Abra o navegador em: `https://localhost:7183`

---

## 🏗️ Arquitetura de Execução

### Camada que você roda: **Yuta.FactoryOps.Server**

O projeto Server é o ponto de entrada que integra todas as camadas:

```
Yuta.FactoryOps.Server (EntryPoint)
├── API REST Controllers
├── Blazor WebAssembly Hosting  
├── Configuração do DbContext (Supabase)
├── Dependency Injection
├── Middleware (Logging, Erros)
└── Integra todas as camadas:
    ├── Domain (Entidades e Interfaces)
    ├── Application (Serviços e Validação)
    ├── Infrastructure (Repositórios e Acesso a Dados)
    └── Client (Blazor Frontend)
```

### Camadas que você NÃO roda diretamente:
- ❌ **Domain** - Biblioteca de classes (referenciada)
- ❌ **Application** - Biblioteca de classes (referenciada)
- ❌ **Infrastructure** - Biblioteca de classes (referenciada)
- ❌ **Client** - Blazor WebAssembly (servido pelo Server)
- ❌ **Tests** - Projeto de testes (rodado com `dotnet test`)

---

## 🔧 Comandos Úteis

### Desenvolvimento
```bash
# Hot reload (se configurado)
dotnet watch --project Yuta.FactoryOps.Server

# Build com detalhes
dotnet build --verbosity detailed

# Limpar build
dotnet clean
```

### Testes
```bash
# Rodar todos os testes
dotnet test

# Rodar testes com detalhes
dotnet test --logger "console;verbosity=detailed"

# Rodar testes específicos
dotnet test --filter "FullyQualifiedName~TokenService"
```

### Publicação
```bash
# Publicar para produção
dotnet publish Yuta.FactoryOps.Server -c Release -o ./publish

# Publicar self-contained
dotnet publish Yuta.FactoryOps.Server -c Release -o ./publish --self-contained
```

### Migrações do Banco de Dados
```bash
# Criar nova migração
dotnet ef migrations add NomeDaMigracao --project Yuta.FactoryOps.Infrastructure --startup-project Yuta.FactoryOps.Server

# Aplicar migrações
dotnet ef database update --project Yuta.FactoryOps.Infrastructure --startup-project Yuta.FactoryOps.Server

# Remover última migração
dotnet ef migrations remove --project Yuta.FactoryOps.Infrastructure --startup-project Yuta.FactoryOps.Server
```

---

## 🐛 Solução de Problemas

### Erro de Porta em Uso
```bash
# Mudar a porta em launchSettings.json ou usar:
dotnet run --project Yuta.FactoryOps.Server --urls "http://localhost:5000"
```

### Erro de Conexão com Supabase
- Verifique `appsettings.json` no projeto Server
- Confirme a connectionString está correta
- Verifique se o banco Supabase está acessível

### Erro no Frontend Blazor
- Limpe o cache do navegador
- Execute `dotnet clean` e `dotnet build`
- Verifique se não há erros no console do navegador (F12)

### Erro de Dependências
```bash
# Limpar e restaurar
dotnet clean
dotnet restore
dotnet build
```

---

## 📊 Estrutura de Portas

Por padrão, o projeto usa:
- **HTTPS:** `https://localhost:7183`
- **HTTP:** `http://localhost:5000`

Você pode alterar isso em:
- `Yuta.FactoryOps.Server/Properties/launchSettings.json`

---

## 🔐 Configuração de Ambiente

### Desenvolvimento
- Usa `appsettings.Development.json`
- Hot reload habilitado
- Logging detalhado

### Produção
- Usa `appsettings.json`
- Otimizações habilitadas
- Logging de produção

---

## 📝 Notas Importantes

1. **Sempre rode o projeto Server** - Ele hospeda toda a aplicação
2. **O Client é servido automaticamente** - Não precisa rodar separadamente
3. **Banco de dados Supabase** - Já configurado e funcional
4. **Logging** - Logs são salvos em `logs/log-.txt`
5. **Testes** - Execute `dotnet test` para verificar qualidade

---

## 🎯 Próximos Passos Após Rodar

1. Acesse `https://localhost:7183`
2. Teste a funcionalidade de login
3. Verifique os logs em `logs/`
4. Execute os testes com `dotnet test`
5. Explore o código da nova arquitetura

---

## 💡 Dicas

- Use `Ctrl+C` para parar o servidor
- Use `dotnet watch` para desenvolvimento com hot reload
- Verifique o console do navegador para erros do frontend
- Os logs do servidor ajudam a identificar problemas de backend

---

## ✅ Verificação de Funcionamento

Após rodar, verifique:
- [ ] Servidor iniciou sem erros
- [ ] Navegador acessa `https://localhost:7183`
- [ ] Página de login carrega
- [ ] Console do navegador sem erros
- [ ] Logs do servidor em `logs/`

---

## 🆘 Suporte

Se encontrar problemas:
1. Verifique os logs em `logs/log-.txt`
2. Execute `dotnet build` para identificar erros de compilação
3. Execute `dotnet test` para verificar testes
4. Revise a configuração em `appsettings.json`

---

**Pronto! Seu projeto está configurado para rodar com a nova arquitetura empresarial.** 🚀