FROM php:8.2-apache

# Actualizar el sistema e instalar las dependencias de PostgreSQL
RUN apt-get update && apt-get install -y libpq-dev git unzip zip \
    && docker-php-ext-configure pgsql -with-pgsql=/usr/local/pgsql \
    && docker-php-ext-install pdo pdo_pgsql pgsql

# Habilitar el módulo de reescritura de Apache (necesario si usarás MVC o frameworks en PHP)
RUN a2enmod rewrite

# Instalar Composer globalmente copiándolo de su imagen oficial
COPY --from=composer:latest /usr/bin/composer /usr/bin/composer

# Configurar el directorio de trabajo
WORKDIR /var/www/html

# Copiar los archivos del proyecto al contenedor
COPY . /var/www/html/

# Instalar dependencias de PHP si existen (ignora si no hay paquetes aún)
RUN if [ -f "composer.json" ]; then composer install --no-dev --optimize-autoloader; fi

# Ajustar los permisos para que Apache pueda leer los archivos
RUN chown -R www-data:www-data /var/www/html