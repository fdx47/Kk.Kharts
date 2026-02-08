# Sistema de Invalidação de Cache com Versioning

## Visão Geral

Este sistema permite invalidar seletivamente o cache do frontend (localStorage, indexedDB, cache API) através de contadores de versão gerenciados pelo backend.

## Como Funciona

1. **Backend** mantém 3 contadores na base de dados:
   - `local_storage_version` - Versão do localStorage
   - `indexed_db_version` - Versão do indexedDB
   - `cache_version` - Versão do Cache API

2. **Frontend** guarda os contadores num cookie e verifica a cada login/refresh
3. Se a versão do backend for **maior** que a do cookie, o frontend limpa o cache correspondente

## Endpoints da API

### 1. Obter Versões Atuais (Autenticado)
```http
GET /api/v1/cacheversion
Authorization: Bearer {token}
```

**Resposta:**
```json
{
  "localStorageVersion": 1,
  "indexedDbVersion": 1,
  "cacheStorageVersion": 1,
  "updatedAt": "2026-01-03T18:00:00Z"
}
```

### 2. Incrementar Versões (Root apenas)
```http
POST /api/v1/cacheversion/increment
Authorization: Bearer {token}
Content-Type: application/json

{
  "localStorageVersion": 1,
  "indexedDbVersion": 1,
  "cacheStorageVersion": 1
}
```

**Nota:** Apenas incrementa as versões especificadas (valores opcionais)

### 3. Definir Versões Específicas (Root apenas)
```http
PUT /api/v1/cacheversion/set
Authorization: Bearer {token}
Content-Type: application/json

{
  "localStorageVersion": 5,
  "indexedDbVersion": 3,
  "cacheStorageVersion": 2
}
```

### 4. Invalidar Todos os Caches (Root apenas)
```http
POST /api/v1/cacheversion/invalidate-all
Authorization: Bearer {token}
```

Incrementa automaticamente todas as versões.

---

## Implementação no Frontend

### JavaScript/TypeScript

```typescript
// cacheVersionManager.ts

interface CacheVersions {
  localStorageVersion: number;
  indexedDbVersion: number;
  cacheStorageVersion: number;
  updatedAt: string;
}

class CacheVersionManager {
  private readonly COOKIE_NAME = 'cache_versions';
  private readonly API_URL = '/api/v1/cacheversion';

  /**
   * Verifica e atualiza o cache baseado nas versões do servidor
   */
  async checkAndUpdateCache(token: string): Promise<void> {
    try {
      // 1. Obter versões do servidor
      const serverVersions = await this.fetchServerVersions(token);
      
      // 2. Obter versões locais do cookie
      const localVersions = this.getLocalVersions();
      
      // 3. Comparar e limpar se necessário
      if (!localVersions) {
        // Primeira vez - apenas salvar as versões
        this.saveLocalVersions(serverVersions);
        return;
      }

      // Verificar localStorage
      if (serverVersions.localStorageVersion > localVersions.localStorageVersion) {
        console.log('🗑️ Limpando localStorage...');
        this.clearLocalStorage();
      }

      // Verificar indexedDB
      if (serverVersions.indexedDbVersion > localVersions.indexedDbVersion) {
        console.log('🗑️ Limpando indexedDB...');
        await this.clearIndexedDB();
      }

      // Verificar Cache API
      if (serverVersions.cacheStorageVersion > localVersions.cacheStorageVersion) {
        console.log('🗑️ Limpando Cache API...');
        await this.clearCacheStorage();
      }

      // 4. Atualizar versões locais
      this.saveLocalVersions(serverVersions);
      
    } catch (error) {
      console.error('Erro ao verificar versões de cache:', error);
    }
  }

  /**
   * Busca as versões do servidor
   */
  private async fetchServerVersions(token: string): Promise<CacheVersions> {
    const response = await fetch(this.API_URL, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) {
      throw new Error('Falha ao obter versões do cache');
    }

    return await response.json();
  }

  /**
   * Obtém versões salvas no cookie
   */
  private getLocalVersions(): CacheVersions | null {
    const cookie = document.cookie
      .split('; ')
      .find(row => row.startsWith(`${this.COOKIE_NAME}=`));

    if (!cookie) return null;

    try {
      const value = cookie.split('=')[1];
      return JSON.parse(decodeURIComponent(value));
    } catch {
      return null;
    }
  }

  /**
   * Salva versões no cookie (válido por 30 dias)
   */
  private saveLocalVersions(versions: CacheVersions): void {
    const expires = new Date();
    expires.setDate(expires.getDate() + 30);
    
    const cookieValue = encodeURIComponent(JSON.stringify(versions));
    document.cookie = `${this.COOKIE_NAME}=${cookieValue}; expires=${expires.toUTCString()}; path=/; SameSite=Strict`;
  }

  /**
   * Limpa localStorage (exceto token de autenticação)
   */
  private clearLocalStorage(): void {
    const authToken = localStorage.getItem('auth_token');
    localStorage.clear();
    if (authToken) {
      localStorage.setItem('auth_token', authToken);
    }
  }

  /**
   * Limpa todas as bases de dados do indexedDB
   */
  private async clearIndexedDB(): Promise<void> {
    if (!window.indexedDB) return;

    const databases = await window.indexedDB.databases();
    
    for (const db of databases) {
      if (db.name) {
        window.indexedDB.deleteDatabase(db.name);
      }
    }
  }

  /**
   * Limpa todos os caches da Cache API
   */
  private async clearCacheStorage(): Promise<void> {
    if (!('caches' in window)) return;

    const cacheNames = await caches.keys();
    
    await Promise.all(
      cacheNames.map(cacheName => caches.delete(cacheName))
    );
  }
}

// Exportar instância singleton
export const cacheVersionManager = new CacheVersionManager();
```

### Uso no Login/Inicialização da App

```typescript
// app.ts ou main.ts

import { cacheVersionManager } from './cacheVersionManager';

async function initializeApp() {
  const token = getAuthToken(); // Seu método de obter o token
  
  if (token) {
    // Verificar e atualizar cache após login
    await cacheVersionManager.checkAndUpdateCache(token);
  }
  
  // Resto da inicialização da app...
}

// Também verificar após refresh do token
async function onTokenRefresh(newToken: string) {
  await cacheVersionManager.checkAndUpdateCache(newToken);
}
```

---

## Casos de Uso

### 1. Atualização de Dados Críticos
Quando mudas a estrutura de dados no localStorage:
```bash
curl -X POST https://kropkontrol.premiumasp.net/api/v1/cacheversion/increment \
  -H "Authorization: Bearer {root_token}" \
  -H "Content-Type: application/json" \
  -d '{"localStorageVersion": 1}'
```

### 2. Nova Versão do Service Worker
Quando atualizas o service worker e queres limpar o cache:
```bash
curl -X POST https://kropkontrol.premiumasp.net/api/v1/cacheversion/increment \
  -H "Authorization: Bearer {root_token}" \
  -H "Content-Type: application/json" \
  -d '{"cacheStorageVersion": 1}'
```

### 3. Atualização Completa da App
Quando fazes deploy de uma nova versão:
```bash
curl -X POST https://kropkontrol.premiumasp.net/api/v1/cacheversion/invalidate-all \
  -H "Authorization: Bearer {root_token}"
```

---

## Script SQL para Criar a Tabela

```sql
-- Criar tabela cache_versions
CREATE TABLE [kropKharts].[cache_versions] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [local_storage_version] INT NOT NULL DEFAULT 1,
    [indexed_db_version] INT NOT NULL DEFAULT 1,
    [cache_version] INT NOT NULL DEFAULT 1,
    [updated_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [updated_by] NVARCHAR(100) NULL
);

-- Inserir registro inicial
INSERT INTO [kropKharts].[cache_versions] 
    ([local_storage_version], [indexed_db_version], [cache_version], [updated_at], [updated_by])
VALUES 
    (1, 1, 1, GETUTCDATE(), 'System');

-- Criar índice para performance
CREATE INDEX IX_cache_versions_updated_at ON [kropKharts].[cache_versions]([updated_at] DESC);
```

---

## Vantagens desta Abordagem

1. ✅ **Granular** - Limpa apenas o que é necessário
2. ✅ **Seguro** - Não afeta o token de autenticação
3. ✅ **Eficiente** - Usa cookies (pequeno overhead)
4. ✅ **Controlado** - Apenas Root pode invalidar
5. ✅ **Auditável** - Regista quem e quando atualizou
6. ✅ **Simples** - Fácil de integrar no frontend existente

---

## Alternativas Consideradas

### Por que não usar apenas timestamps?
- Contadores são mais explícitos e previsíveis
- Evita problemas de sincronização de relógio
- Mais fácil de debugar

### Por que não usar ETags?
- Requer mais lógica no frontend
- Contadores são mais simples para este caso de uso

### Por que cookies e não localStorage?
- Cookies persistem mesmo após limpar localStorage
- Disponíveis antes do JavaScript carregar
- Podem ser lidos no servidor se necessário
