# Contabilita - Gestionale Contabilità Personale

Gestionale per la contabilità personale con ASP.NET Core 8, Vue.js 3, Bootstrap 5 e MySQL.

## Struttura Progetto

```
Contabilita/
├── src/
│   ├── Contabilita.API/        # Web API ASP.NET Core
│   ├── Contabilita.Core/       # Entità e Interfacce
│   └── Contabilita.Infrastructure/  # DbContext e Repository
├── client-app/                  # Frontend Vue.js 3
└── Contabilita.sln
```

## Funzionalità

- **Autenticazione** - Registrazione e login con JWT
- **Dashboard** - Riepilogo budget, entrate, uscite e grafici
- **Transazioni** - Registrazione entrate e uscite giornaliere
- **Categorie** - Gestione categorie personalizzate per entrate/uscite
- **Spese Programmate** - Pianificazione spese ricorrenti con conferma manuale
- **Calendario** - Visualizzazione eventi e scadenze

## Prerequisiti

- .NET 8 SDK
- Node.js 18+
- MySQL Server

## Configurazione Database

1. Modifica `src/Contabilita.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ContabilitaDb;User=TUO_USER;Password=TUA_PASSWORD;"
  }
}
```

2. Crea la migrazione e il database:

```bash
cd src/Contabilita.API
dotnet ef migrations add InitialCreate --project ../Contabilita.Infrastructure
dotnet ef database update
```

## Avvio Sviluppo

### Backend (porta 5000)

```bash
cd src/Contabilita.API
dotnet run
```

### Frontend (porta 5173)

```bash
cd client-app
npm install
npm run dev
```

## Build Produzione

### Backend

```bash
dotnet publish src/Contabilita.API -c Release -o ./publish
```

### Frontend

```bash
cd client-app
npm run build
# Copia il contenuto di dist/ in wwwroot/ dell'API per hosting monolitico
```

## API Endpoints

### Auth
- `POST /api/auth/register` - Registrazione
- `POST /api/auth/login` - Login
- `GET /api/auth/me` - Profilo utente
- `PUT /api/auth/me` - Aggiorna profilo

### Categories
- `GET /api/categories` - Lista categorie
- `POST /api/categories` - Crea categoria
- `PUT /api/categories/{id}` - Modifica categoria
- `DELETE /api/categories/{id}` - Elimina categoria

### Transactions
- `GET /api/transactions` - Lista transazioni (con filtri)
- `GET /api/transactions/summary` - Riepilogo
- `POST /api/transactions` - Crea transazione
- `PUT /api/transactions/{id}` - Modifica transazione
- `DELETE /api/transactions/{id}` - Elimina transazione

### Scheduled Expenses
- `GET /api/scheduledexpenses` - Lista spese programmate
- `GET /api/scheduledexpenses/due` - Spese in scadenza
- `POST /api/scheduledexpenses` - Crea spesa programmata
- `POST /api/scheduledexpenses/{id}/confirm` - Conferma e registra
- `POST /api/scheduledexpenses/{id}/skip` - Salta occorrenza

### Calendar Events
- `GET /api/calendarevents` - Lista eventi
- `POST /api/calendarevents` - Crea evento
- `PUT /api/calendarevents/{id}` - Modifica evento
- `DELETE /api/calendarevents/{id}` - Elimina evento

## Tecnologie

- **Backend**: ASP.NET Core 8, Entity Framework Core 8, JWT Auth
- **Frontend**: Vue.js 3, Pinia, Vue Router, Bootstrap 5
- **Database**: MySQL (Pomelo.EntityFrameworkCore.MySql)
