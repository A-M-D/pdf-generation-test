FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS="http://*:5000"
ENV PUPPETEER_EXECUTABLE_PATH "/usr/bin/chromium-browser"

# Puppeteer
RUN apk update && apk upgrade && \
    echo @edge http://nl.alpinelinux.org/alpine/edge/community >> /etc/apk/repositories && \
    echo @edge http://nl.alpinelinux.org/alpine/edge/main >> /etc/apk/repositories && \
    apk add --no-cache \
      chromium \
      harfbuzz \
      nss

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Pdf.csproj", "./"]
RUN dotnet restore "Pdf.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Pdf.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Pdf.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pdf.dll"]
