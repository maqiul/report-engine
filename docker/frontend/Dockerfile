# 前端 Dockerfile - Vue 3 + Nginx
# 构建：cd D:\report-engine\docker && docker build -f frontend\Dockerfile -t reportengine-frontend:0.2.0 ..
# 运行：docker run -d -p 3000:80 --name reportengine-frontend reportengine-frontend:0.2.0

# 构建阶段
FROM node:20-alpine AS builder

LABEL maintainer="maqiul <maqiuliang4@163.com>"
LABEL version="0.2.0"

WORKDIR /build

# 拷贝 package 文件（context 是仓库根 D:\report-engine）
COPY web/frontend/package*.json ./
COPY web/frontend/pnpm-lock.yaml* ./

# 安装依赖（pnpm 优先，fallback npm）
RUN if [ -f pnpm-lock.yaml ]; then \
        npm install -g pnpm && pnpm install --frozen-lockfile; \
    else \
        npm ci; \
    fi

# 拷贝源码并构建
COPY web/frontend/ ./
RUN npm run build

# 运行阶段 - nginx 托管静态文件 + 反代后端
FROM nginx:1.27-alpine

# 拷贝构建产物
COPY --from=builder /build/dist /usr/share/nginx/html

# 拷贝 nginx 配置（从 docker 根 context 拷入）
COPY docker/frontend/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
  CMD wget -qO- http://127.0.0.1/ >/dev/null || exit 1

CMD ["nginx", "-g", "daemon off;"]
