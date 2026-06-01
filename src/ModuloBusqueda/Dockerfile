# ── Stage 1: base ─────────────────────────────────────────
# Imagen común con workdir y package.json copiado.
# Los stages siguientes parten de aquí para no repetir instrucciones.
FROM node:20-alpine AS base
WORKDIR /usr/src/app
COPY package*.json ./

# ── Stage 2: development ───────────────────────────────────
# Instala TODAS las dependencias (incluye tsx, typescript).
# El código fuente NO se copia aquí: docker-compose lo monta como volumen
# para que los cambios locales se reflejen en el contenedor sin rebuild.
FROM base AS development
RUN npm install
EXPOSE 3000
CMD ["npm", "run", "dev"]

# ── Stage 3: builder ───────────────────────────────────────
# Compila TypeScript → JavaScript. El dist/ generado aquí
# es el único válido; no depende del dist/ del repositorio.
FROM base AS builder
RUN npm install
COPY . .
RUN npm run build

# ── Stage 4: production ────────────────────────────────────
# Imagen mínima: solo deps de producción + el dist/ compilado.
FROM base AS production
RUN npm install --only=production
COPY --from=builder /usr/src/app/dist ./dist
EXPOSE 3000
CMD ["npm", "run", "start"]
