# ReportEngine Docker 部署

## 一键启动（推荐）

```bash
cd D:\report-engine\docker
docker compose up -d
```

启动后访问：
- 前端：http://localhost:3000
- 后端 API：http://localhost:5000
- Swagger UI：http://localhost:5000/swagger-ui.html
- OpenAPI JSON：http://localhost:5000/v3/api-docs

## 单独构建

### 后端
```bash
cd D:\report-engine
docker build -f docker\backend\Dockerfile -t reportengine-backend:0.2.0 web\backend
docker run -d -p 5000:5000 --name reportengine-backend reportengine-backend:0.2.0
```

### 前端
```bash
cd D:\report-engine
docker build -f docker\frontend\Dockerfile -t reportengine-frontend:0.2.0 web\frontend
docker run -d -p 3000:80 --name reportengine-frontend reportengine-frontend:0.2.0
```

## 架构

```
┌─────────────────┐      /api/*       ┌─────────────────┐
│   Frontend      │  ───────────────► │   Backend       │
│   (nginx:80)    │                   │   (Spring Boot) │
│   localhost:3000│                   │   :5000         │
└─────────────────┘                   └─────────────────┘
```

前端容器内置 nginx，负责：
- 托管 Vue 静态资源
- SPA 路由 fallback（`try_files`）
- `/api/*` 反向代理到后端
- Swagger UI 路径反代

## 镜像大小

- 后端（eclipse-temurin:17-jre）：约 280MB
- 前端（nginx:1.27-alpine + node builder）：约 50MB（运行时）

## 停止 / 清理

```bash
# 停止
docker compose down

# 停止 + 删除镜像
docker compose down --rmi all

# 停止 + 删除卷
docker compose down -v
```
