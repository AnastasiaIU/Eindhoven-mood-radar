# Deployment Guide: Render.com

> This guide covers deploying the Eindhoven Mood Radar to Render.com with auto-deployment from the `main` branch.

## Prerequisites

- GitHub account with repository access
- [Render.com](https://render.com) account (free tier available)
- `.env` files configured locally (backend API keys, database credentials)

## Architecture Overview

```
┌─────────────────────────────────────────────────┐
│         GitHub Repository (main branch)         │
│       (Triggers webhook on push/PR merge)       │
└──────────────────────┬──────────────────────────┘
                       │
                       ▼
         ┌─────────────────────────────┐
         │   GitHub Actions CI/CD      │
         │  (lint → build → test)      │
         └─────────────────────────────┘
                       │
                       ▼
        ┌──────────────────────────────┐
        │   Render.com Auto-Deploy     │
        │  (triggered on main branch)  │
        └──────────────────────────────┘
                       │
         ┌─────────────┴──────────────┐
         ▼                            ▼
    ┌─────────────┐          ┌──────────────┐
    │ Backend API │          │ Frontend App │
    │ (.NET Core) │          │  (Next.js)   │
    └─────────────┘          └──────────────┘
```

---

## 1. Backend Deployment (C# ASP.NET Core)

### Step 1.1: Create Backend Service on Render

1. Log in to [Render Dashboard](https://dashboard.render.com)
2. Click **New** → **Web Service**
3. Select **Deploy from GitHub Repository**
4. Connect your GitHub account and select `Eindhoven-mood-radar` repository

### Step 1.2: Configure Backend Service

**Name**: `eindhoven-mood-radar-api`

**Environment**: `Docker`

**Build Command**:

```bash
cd moonshot-backend && dotnet publish -c Release -o out
```

**Start Command**:

```bash
cd moonshot-backend/out && dotnet MoodRadar.API.dll
```

**Runtime**: Ubuntu (default)

**Plan**: Free tier (with limitations on uptime)

### Step 1.3: Environment Variables

Click **Environment** and add:

| Key | Value | Notes |
|-----|-------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Enables optimized performance |
| `TICKETMASTER_API_KEY` | `<your-api-key>` | Get from [Ticketmaster Developer](https://developer.ticketmaster.com) |
| `WEATHER_API_KEY` | `<optional>` | Open-Meteo is free; not required |
| `DATABASE_URL` | (See PostgreSQL setup below) | Connection string |

### Step 1.4: Deploy

1. Click **Create Web Service**
2. Render builds and deploys automatically
3. Monitor progress in the **Logs** tab
4. Once live, note your service URL: `https://eindhoven-mood-radar-api.render.com`

**Note**: Free tier has auto-sleep after 15 min inactivity.

---

## 2. Frontend Deployment (Next.js)

### Step 2.1: Create Frontend Service on Render

1. Log in to [Render Dashboard](https://dashboard.render.com)
2. Click **New** → **Static Site** (or **Web Service** for server-side rendering)
3. Select your GitHub repository

### Step 2.2: Configure Frontend Service

**Name**: `eindhoven-mood-radar-web`

**Build Command**:

```bash
cd moonshot-webapp && npm install && npm run build
```

**Publish Directory**: `moonshot-webapp/.next/standalone` (if using `output: 'standalone'` in `next.config.ts`)
or 
`moonshot-webapp/.next` (for default Next.js builds)

**Plan**: Free tier

### Step 2.3: Environment Variables (Optional)

If your Next.js app needs backend API URL:

| Key | Value | Notes |
|-----|-------|-------|
| `NEXT_PUBLIC_API_URL` | `https://eindhoven-mood-radar-api.render.com` | Public API endpoint |

### Step 2.4: Deploy

1. Click **Create Static Site** (or **Web Service**)
2. Render clones, installs, builds, and deploys
3. Monitor in the **Logs** tab
4. Your frontend URL: `https://eindhoven-mood-radar-web.onrender.com`

---

## 3. PostgreSQL Database (Optional - Phase 1 Uses Mock Data)

If you need to move from mock data to a live database:

### Step 3.1: Create PostgreSQL Database

1. On Render Dashboard, click **New** → **PostgreSQL**
2. **Name**: `eindhoven-mood-radar-db`
3. **PostgreSQL Version**: 15 (or latest)
4. **Region**: Frankfurt (or your region)
5. **Plan**: Free tier (limited storage)

### Step 3.2: Add Database URL to Backend

1. Copy the **Connections** → **Internal Database URL** from your PostgreSQL service
2. Update Backend service environment variable:
   - **Key**: `DATABASE_URL`
   - **Value**: `<your-internal-database-url>`

### Step 3.3: Run Migrations

SSH into your backend service or use a migration runner:

```bash
# From local machine (if you have CLI access)
dotnet ef database update -p moonshot-backend/MoodRadar.API

# Or manually in Render: add migration script to startup
```

---

## 4. GitHub Actions Integration

### Step 4.1: Configure Automatic Deployment

Render **automatically deploys** on every push to `main` if you've connected your GitHub repository. To verify:

1. Go to Backend/Frontend service settings
2. Check **Deploy on Push** is enabled (it should be by default)
3. Branch should be set to `main`

### Step 4.2: Monitor CI/CD

- **GitHub**: Push to `main` → GitHub Actions runs lint/build/test
- **Render**: On successful GitHub push, auto-deploys immediately
- Check **Logs** on Render to debug deployment issues

---

## 5. Environment-Specific Configuration

### appsettings.Production.json (Backend)

Create or update `moonshot-backend/MoodRadar.API/appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning"
    }
  },
  "AllowedHosts": "*",
  "TicketmasterApiKey": "ASPNETCORE_ENVIRONMENT=Production will use Render env var",
  "Database": {
    "ConnectionString": "Will use DATABASE_URL environment variable"
  }
}
```

### next.config.ts (Frontend)

Ensure `.next/standalone` output is configured:

```typescript
const nextConfig: NextConfig = {
  output: 'standalone', // Reduces deployment size
  // ... other config
};
```

---

## 6. Troubleshooting

### Backend Build Fails: "dotnet: command not found"

- **Solution**: Render may not have .NET SDK pre-installed. Use a Dockerfile instead:

```dockerfile
# moonshot-backend/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 5000
CMD ["dotnet", "MoodRadar.API.dll"]
```

Then set **Build Command** to: `docker build -t myapp .`

### Frontend Build Fails: "Module not found"

- **Solution**: Ensure `package-lock.json` is committed to Git, or use `npm ci` instead of `npm install` in build script

### Service Keeps Auto-Sleeping (Free Tier)

- **Solution**: Upgrade to **Starter plan** ($7/month) for continuous uptime, or use a service like [Kping](https://kping.com) to ping the service every 10 min

---

## 7. Monitoring & Logs

### View Logs on Render

1. Select service from dashboard
2. Click **Logs** tab to see real-time output
3. Search or filter errors

### Trigger Manual Deploy

1. Go to service settings
2. Click **Manual Deploy** → **Deploy latest commit**

### Rollback to Previous Version

- Render keeps deployment history; click **Deployments** tab to revert

---

## 8. HTTPS & Custom Domain

### Add Custom Domain

1. Go to Frontend service settings
2. Click **Custom Domain**
3. Point your domain's DNS CNAME to Render-provided URL
4. Render auto-provisions SSL certificate (Let's Encrypt)

### Redirect Backend to Frontend

In your frontend's `next.config.ts`:

```typescript
async redirects() {
  return [
    {
      source: '/api/:path*',
      destination: 'https://eindhoven-mood-radar-api.render.com/api/:path*',
      permanent: false,
    },
  ];
}
```

---

## Additional Resources

- [Render Documentation](https://render.com/docs)
- [Deploying .NET to Render](https://render.com/docs/deploy-dotnet)
- [Deploying Next.js to Render](https://render.com/docs/deploy-nextjs)
- [GitHub Actions Docs](https://docs.github.com/en/actions)
