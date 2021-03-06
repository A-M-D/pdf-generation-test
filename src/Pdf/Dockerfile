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
      nss \
      fontconfig \
      wkhtmltopdf

RUN ln -s /usr/lib/libwkhtmltox.so.0 /usr/lib/libwkhtmltox.so

# Install fonts
# RUN mkdir -p /usr/share/fonts/opentype
# COPY ./fonts/*.otf /usr/share/fonts/opentype/
# RUN fc-cache -f -v

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src
COPY ["src/Pdf/Pdf.csproj", "src/Pdf/"]
RUN dotnet restore "src/Pdf/Pdf.csproj"
COPY . .
WORKDIR "/src/src/Pdf"
RUN dotnet build "Pdf.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pdf.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pdf.dll"]
