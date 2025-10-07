# MazeWalking Build & Deployment Guide

This guide covers building and deploying the MazeWalking application, which combines a React frontend with a .NET 8 backend.

## 🚀 Quick Start

### Development Mode

**Backend (.NET):**
```bash
cd MazeWalking.Web
dotnet run
# Runs on http://localhost:5180 and https://localhost:7148
```

**Frontend (React + Vite):**
```bash
cd MazeWalking.UI
npm install
npm run dev
# Runs on http://localhost:5173 with HMR
```

The Vite dev server proxies `/api/*` requests to the .NET backend automatically.

---

## 📦 Production Build

### Option 1: Build Scripts (Recommended for local deployment)

**Windows (PowerShell):**
```powershell
.\build-artifact.ps1
```

**Linux/Mac (Bash):**
```bash
chmod +x build-artifact.sh
./build-artifact.sh
```

**What it does:**
1. Builds React UI → `MazeWalking.UI/dist/`
2. Copies `dist/` → `MazeWalking.Web/wwwroot/`
3. Publishes .NET app → `publish/` directory

**Run the artifact:**
```bash
cd publish
dotnet MazeWalking.Web.dll
# Application runs on http://localhost:5180
```

---

### Option 2: Docker (Recommended for containerized deployment)

**Build Docker image:**
```bash
docker build -t mazewalking:latest .
```

**Run container:**
```bash
docker run -d \
  -p 5180:5180 \
  -v $(pwd)/data:/app/db \
  --name mazewalking \
  mazewalking:latest
```

**With environment variables:**
```bash
docker run -d \
  -p 5180:5180 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Data Source=/app/db/mazewalking.db" \
  -v $(pwd)/data:/app/db \
  -v $(pwd)/logs:/app/logs \
  --name mazewalking \
  mazewalking:latest
```

**Stop container:**
```bash
docker stop mazewalking
docker rm mazewalking
```

---

## 🏗️ Build Architecture

### Multi-Stage Docker Build

The `Dockerfile` uses a 3-stage build:

1. **Stage 1 (ui-builder):** Node.js Alpine → builds React app
2. **Stage 2 (dotnet-builder):** .NET SDK → builds backend + copies React dist
3. **Stage 3 (runtime):** .NET ASP.NET Runtime Alpine → minimal runtime image

**Benefits:**
- Small final image (~100MB vs ~1GB)
- No build tools in production
- Optimized layer caching

### Static File Serving

The .NET backend serves the React SPA:

- `app.UseStaticFiles()` - serves files from `wwwroot/`
- `app.MapFallbackToFile("index.html")` - SPA routing fallback
- React Router handles client-side routing
- API routes (`/api/*`) are handled by .NET controllers

---

## 🔧 Configuration

### Development vs Production

**Development:**
- Vite dev server (port 5173) with HMR
- Proxies API requests to .NET backend
- CORS enabled for `http://localhost:5173`
- Swagger UI available at `/swagger`

**Production:**
- Single .NET server serves everything
- React app served from `wwwroot/`
- No CORS needed (same origin)
- Swagger disabled

### Environment Variables

**Backend (.NET):**
- `ASPNETCORE_ENVIRONMENT` - `Development` | `Production`
- `ASPNETCORE_URLS` - e.g., `http://+:5180`
- `ConnectionStrings__DefaultConnection` - SQLite connection string

**Frontend (React):**
- `VITE_API_BASE_URL` - Optional, defaults to `/api`

---

## 📁 Project Structure

```
maze-walking/
├── MazeWalking.UI/          # React frontend
│   ├── src/
│   ├── dist/                # Build output
│   ├── package.json
│   └── vite.config.ts
├── MazeWalking.Web/         # .NET backend
│   ├── wwwroot/             # Static files (React build copied here)
│   ├── Controllers/
│   ├── Services/
│   ├── MazeWalking.Web.csproj
│   └── Program.cs
├── publish/                 # Publish output (gitignored)
├── Dockerfile               # Multi-stage Docker build
├── .dockerignore
├── build-artifact.ps1       # PowerShell build script
└── build-artifact.sh        # Bash build script
```

---

## 🐛 Troubleshooting

### React app not loading in production
- Verify `wwwroot/` contains `index.html` and assets
- Check `app.UseStaticFiles()` is called before `app.MapFallbackToFile()`
- Ensure `base: '/'` in `vite.config.ts`

### API requests fail (404)
- Verify API routes are registered: `app.MapPosts()`
- Check CORS configuration in development
- In production, ensure `/api/*` routes are defined before fallback

### Database not persisting
- Mount volume for database: `-v $(pwd)/data:/app/db`
- Check connection string points to mounted path
- Verify write permissions on mounted directory

### Docker build fails
- Check `.dockerignore` isn't excluding necessary files
- Ensure `package.json` and `*.csproj` are copied before dependencies
- Verify Node.js and .NET SDK versions in Dockerfile

---

## 📊 Performance Tips

1. **Enable compression:** Add `app.UseResponseCompression()` in production
2. **Asset caching:** Configure cache headers for static files
3. **Code splitting:** Vite automatically chunks vendor code (React, React-DOM)
4. **SQLite optimization:** Use WAL mode for better concurrency
5. **Docker layers:** Order Dockerfile commands to maximize cache hits

---

## 🚢 Deployment Checklist

- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Configure proper connection string
- [ ] Mount volumes for database and logs
- [ ] Enable HTTPS (use reverse proxy like nginx/Caddy)
- [ ] Set up log rotation
- [ ] Configure health checks
- [ ] Set memory limits in Docker
- [ ] Enable monitoring/metrics

---

## 📝 Additional Commands

**Clean build artifacts:**
```bash
# PowerShell
Remove-Item -Recurse -Force publish, MazeWalking.Web\wwwroot, MazeWalking.UI\dist

# Bash
rm -rf publish MazeWalking.Web/wwwroot MazeWalking.UI/dist
```

**Rebuild Docker without cache:**
```bash
docker build --no-cache -t mazewalking:latest .
```

**View Docker logs:**
```bash
docker logs -f mazewalking
```

**Enter running container:**
```bash
docker exec -it mazewalking /bin/sh
```
