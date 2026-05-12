# Etapa de compilación
FROM node:20-alpine AS builder
WORKDIR /usr/src/app

# Instalar dependencias incluyendo las de desarrollo (TypeScript)
COPY package*.json ./
RUN npm install

# Copiar el código fuente
COPY . .

# Compilar TypeScript a JavaScript (asume que tienes configurado "outDir": "./dist" en tsconfig.json)
RUN npx tsc

# Etapa final (Imagen de producción)
FROM node:20-alpine
WORKDIR /usr/src/app

# Copiar solo el package.json e instalar ÚNICAMENTE dependencias de producción
COPY --from=builder /usr/src/app/package*.json ./
RUN npm install --only=production

# Copiar el código JavaScript ya compilado
COPY --from=builder /usr/src/app/dist ./dist

# Exponer el puerto
EXPOSE 3000

# Iniciar la aplicación
CMD ["node", "dist/index.js"]