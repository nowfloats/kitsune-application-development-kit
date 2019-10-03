FROM node:8-alpine as builder
RUN apk add --no-cache git
WORKDIR /app

COPY package.json yarn.lock ./
RUN yarn --pure-lockfile

COPY . /app
RUN yarn run build

FROM getkitsune/nginx:latest
RUN rm /etc/nginx/sites-enabled/default
WORKDIR /var/www

COPY --from=builder /app/dist .
COPY src/404.html .
ADD nginx_config /etc/nginx/sites-enabled/
