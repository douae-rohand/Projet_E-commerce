# Configuration n8n pour le Chatbot AI

## Installation locale

1. Installer n8n :
```bash
   npm install -g n8n
```

2. Démarrer n8n :
```bash
   n8n start
```

3. Importer le workflow :
   - Ouvrir http://localhost:5678
   - Workflows → Import from File
   - Sélectionner `ai-chatbot-workflow.json`

4. Configurer les credentials :
   - **Google Gemini API** : Votre clé API
   - **Microsoft SQL** : Connection à votre base locale
   
5. Activer le workflow (toggle en haut à droite)

6. Mettre à jour l'URL dans `appsettings.Development.json` :
```json
   {
     "N8n": {
       "WebhookUrl": "http://localhost:5678/webhook/[votre-id-unique]"
     }
   }
```

## URLs

- **Production** : http://localhost:5678/webhook/3dc65d23-466f-47a6-828e-e3f4f5c4e0fd
- **Test** : http://localhost:5678/webhook-test/3dc65d23-466f-47a6-828e-e3f4f5c4e0fd
```

---

## 3. 🚀 **Déploiement en Production**

### Architecture recommandée :
```
┌─────────────────┐
│   Application   │
│   ASP.NET Core  │ ──┐
└─────────────────┘   │
                      │ HTTP Request
┌─────────────────┐   │
│   SQL Server    │   │
└─────────────────┘   │
                      ▼
                ┌──────────────┐
                │  n8n Server  │
                │  (Docker)    │
                └──────────────┘
                      │
                      ▼
                ┌──────────────┐
                │ Google Gemini│
                │     API      │
                └──────────────┘