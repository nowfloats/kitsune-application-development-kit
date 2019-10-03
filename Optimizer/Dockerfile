FROM node:8 as builder

RUN npm i -g npm yarn typescript
WORKDIR /app/src

COPY package.json .
COPY yarn.lock .
COPY tsconfig.json .

RUN yarn install
COPY . .

RUN yarn run build

FROM node:8

WORKDIR /usr/src
COPY package.json yarn.lock ./
ENV NODE_ENV production
RUN yarn install

# Keep at last to override node_moudles with patch
COPY --from=builder /app/src/ .

CMD [ "node", "app.js" ]
