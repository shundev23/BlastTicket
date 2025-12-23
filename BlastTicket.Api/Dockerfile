# デバッグ コンテナーをカスタマイズする方法と、Visual Studio がこの Dockerfile を使用してより高速なデバッグのためにイメージをビルドする方法については、https://aka.ms/customizecontainer をご覧ください。

# これらの ARG を使用すると、VS からデバッグする際に、最終的なイメージの作成に使用されるベースを入れ替えることができます。
ARG LAUNCHING_FROM_VS
# これは最終的な基本イメージを設定しますが、LAUNCHING_FROM_VS が定義されている場合にのみ設定されます
ARG FINAL_BASE_IMAGE=${LAUNCHING_FROM_VS:+aotdebug}

# このステージは、VS から高速モードで実行するときに使用されます (デバッグ構成の既定値)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# このステージは、サービス プロジェクトのビルドに使用されます
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# ネイティブに公開するための clang/zlib1g-dev 依存関係をインストールする
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    clang zlib1g-dev
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BlastTicket.Api/BlastTicket.Api.csproj", "BlastTicket.Api/"]
RUN dotnet restore "./BlastTicket.Api/BlastTicket.Api.csproj"
COPY . .
WORKDIR "/src/BlastTicket.Api"
RUN dotnet build "./BlastTicket.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# このステージは、最終ステージにコピーするサービス プロジェクトを公開するために使用されます
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BlastTicket.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

# このステージは、通常モードでのデバッグをサポートするために VS から起動するときの最終ステージのベースとして使用されます (デバッグ構成を使用しない場合の既定)
FROM base AS aotdebug
USER root
# ネイティブ デバッグをサポートするために GDB をインストールする
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    gdb
USER app

# このステージは、運用環境または VS から通常モードで実行している場合に使用されます (デバッグ構成を使用しない場合の既定)
FROM ${FINAL_BASE_IMAGE:-mcr.microsoft.com/dotnet/runtime-deps:10.0} AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["./BlastTicket.Api"]