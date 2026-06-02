FROM node:22-alpine AS deps
WORKDIR /app

COPY package.json ./
RUN npm install

FROM node:22-alpine AS build
WORKDIR /app

ARG NEXT_PUBLIC_API_URL=http://conectasti-api:8080
ARG NEXT_PUBLIC_BIFROST_URL=http://localhost:8080
ARG NEXT_PUBLIC_CLIENT_ID=conecta-sti
ARG NEXT_PUBLIC_REDIRECT_URL=http://localhost:3000/callback
ARG NEXT_PUBLIC_BIFROST_RESPONSE_TYPE=token
ARG NEXT_PUBLIC_USE_MOCK=false
ARG JWT_SECRET=){2H:t@WJ[h(Td+w>69%V]7Hj<:5-@m!Yi5q9?-w[}S

ENV NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL
ENV NEXT_PUBLIC_BIFROST_URL=$NEXT_PUBLIC_BIFROST_URL
ENV NEXT_PUBLIC_CLIENT_ID=$NEXT_PUBLIC_CLIENT_ID
ENV NEXT_PUBLIC_REDIRECT_URL=$NEXT_PUBLIC_REDIRECT_URL
ENV NEXT_PUBLIC_BIFROST_RESPONSE_TYPE=$NEXT_PUBLIC_BIFROST_RESPONSE_TYPE
ENV NEXT_PUBLIC_USE_MOCK=$NEXT_PUBLIC_USE_MOCK
ENV JWT_SECRET=$JWT_SECRET
ENV NEXT_TELEMETRY_DISABLED=1

COPY --from=deps /app/node_modules ./node_modules
COPY . .
RUN npm run build

FROM node:22-alpine AS runner
WORKDIR /app

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
ENV PORT=3000
ENV HOSTNAME=0.0.0.0

COPY --from=build /app/package.json ./package.json
COPY --from=build /app/node_modules ./node_modules
COPY --from=build /app/.next ./.next
COPY --from=build /app/public ./public
COPY --from=build /app/next.config.mjs ./next.config.mjs

EXPOSE 3000

CMD ["npm", "run", "start"]
