FROM nginx
EXPOSE 80
RUN rm -rf /etc/nginx/conf.d
COPY conf /etc/nginx
COPY dist /usr/share/nginx/html