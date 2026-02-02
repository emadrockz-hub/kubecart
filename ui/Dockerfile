FROM nginx:alpine
EXPOSE 8080
RUN rm -rf /usr/share/nginx/html/* \
 && printf "<h1>KubeCart UI placeholder</h1><p>UI will be added later.</p>" > /usr/share/nginx/html/index.html
CMD ["nginx","-g","daemon off;"]
