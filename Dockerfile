FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

EXPOSE 80

RUN apt-get update \
    && apt-get install -y curl jq

# Download and install the Tracer
RUN mkdir -p /opt/datadog \
    && mkdir -p /var/log/datadog \
    && TRACER_VERSION=$(curl -s https://api.github.com/repos/DataDog/dd-trace-dotnet/releases/latest | grep tag_name | cut -d '"' -f 4 | cut -c2-) \
    && curl -LO https://github.com/DataDog/dd-trace-dotnet/releases/download/v${TRACER_VERSION}/datadog-dotnet-apm_${TRACER_VERSION}_amd64.deb \
    && dpkg -i ./datadog-dotnet-apm_${TRACER_VERSION}_amd64.deb \
    && rm ./datadog-dotnet-apm_${TRACER_VERSION}_amd64.deb

# Set environment variables
ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={846F5F1C-F9AE-4B07-969E-05C26BC060D8}
ENV CORECLR_PROFILER_PATH=/opt/datadog/Datadog.Trace.ClrProfiler.Native.so
ENV DD_DOTNET_TRACER_HOME=/opt/datadog

# <DATADOG_SITE> defaults to datadoghq.com
ENV DD_SITE=<DATADOG_SITE>
ENV DD_API_KEY=<DATADOG_API_KEY>
ENV DD_LOGS_DIRECT_SUBMISSION_INTEGRATIONS=Serilog
ENV DD_LOGS_DIRECT_SUBMISSION_HOST=store-app-dd-tracer

RUN /opt/datadog/createLogPath.sh

ENTRYPOINT ["dotnet", "store-app.dll"]