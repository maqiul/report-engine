# 后端 Dockerfile - ReportEngine Java Spring Boot
# 构建：cd D:\report-engine\docker && docker build -f backend\Dockerfile -t reportengine-backend:0.2.0 ..
# 运行：docker run -d -p 5000:5000 --name reportengine-backend reportengine-backend:0.2.0

FROM eclipse-temurin:17-jre AS runtime

LABEL maintainer="maqiul <maqiuliang4@163.com>"
LABEL version="0.2.0"
LABEL description="ReportEngine Java 后端 - 报表渲染 + PDF/Excel 导出"

WORKDIR /app

# 拷贝构建好的 fat jar（context 是仓库根 D:\report-engine）
COPY web/backend/build/libs/backend-0.2.0.jar app.jar

# 拷贝中文字体（PDF 导出用）
COPY docker/fonts/ /app/fonts/

# 暴露端口
EXPOSE 5000

# 健康检查
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
  CMD wget -qO- http://localhost:5000/v3/api-docs >/dev/null || exit 1

# JVM 参数
ENV JAVA_OPTS="-Xms256m -Xmx512m -Dfile.encoding=UTF-8 -Duser.language=zh"

# 启动
ENTRYPOINT ["sh", "-c", "java $JAVA_OPTS -jar app.jar"]
