FROM node:20-slim

WORKDIR /app

COPY package.json package-lock.json* ./
RUN npm install --omit=dev

COPY src/ ./src/
COPY tsconfig.json ./

EXPOSE 3001

CMD ["node", "--import", "tsx", "src/index.ts"]
