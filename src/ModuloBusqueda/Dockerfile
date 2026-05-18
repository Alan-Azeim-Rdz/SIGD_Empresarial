# ==========================================
# ETAPA 1: BASE DE DEPENDENCIAS
# ==========================================
FROM node:20-alpine AS dependencies
WORKDIR /usr/src/app

# Copiar manifiestos primero para aprovechar la caché de Docker
COPY package*.json ./

# Instalar TODAS las dependencias (incluyendo devDependencies como ts-node-dev)
RUN npm install

# Copiar el código fuente y archivos de configuración
COPY . .


# ==========================================
# ETAPA 2: DESARROLLO (TARGET DESTINO PARA LOCAL)
# Esta etapa conserva ts-node-dev y las herramientas de monitoreo en tiempo real
# ==========================================
FROM dependencies AS development
EXPOSE 3000
# El comando por defecto si no lo sobreescribe el docker-compose
CMD ["npm", "run dev"]


# ==========================================
# ETAPA 3: COMPILACIÓN PARA PRODUCCIÓN
# ==========================================
FROM dependencies AS builder
RUN npx tsc


# ==========================================
# ETAPA 4: PRODUCCIÓN (IMAGEN LIMPIA Y EMPRESARIAL)
# ==========================================
FROM node:20-alpine AS production
WORKDIR /usr/src/app

# Copiar manifiestos e instalar exclusivamente librerías de ejecución
COPY package*.json ./
RUN npm install --only=production

# Traer únicamente los archivos JavaScript transpilados de la etapa builder
COPY --from=builder /usr/src/app/dist ./dist

EXPOSE 3000

# Iniciar el servicio optimizado
CMD ["node", "dist/index.js"]