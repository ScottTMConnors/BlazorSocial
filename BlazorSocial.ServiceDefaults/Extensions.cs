using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace BlazorSocial.ServiceDefaults;

public static class Extensions
{
    /// <summary>
    ///     Maps default health check endpoints (/health for readiness, /alive for liveness).
    /// </summary>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // All health checks must pass for the app to be considered ready to accept traffic
        app.MapHealthChecks("/health");

        // Only health checks tagged with "live" must pass for the app to be considered alive
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        return app;
    }

    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Adds default services including OpenTelemetry, health checks, service discovery, and resilience.
        /// </summary>
        public IHostApplicationBuilder AddServiceDefaults()
        {
            builder.ConfigureOpenTelemetry();
            builder.AddDefaultHealthChecks();
            builder.Services.AddServiceDiscovery();
            builder.Services.ConfigureHttpClientDefaults(http =>
            {
                http.AddStandardResilienceHandler();
                http.AddServiceDiscovery();
            });

            return builder;
        }

        /// <summary>
        ///     Configures OpenTelemetry logging, metrics, and tracing for the application.
        /// </summary>
        public IHostApplicationBuilder ConfigureOpenTelemetry()
        {
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });

            var otel = builder.Services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    metrics.AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation();
                })
                .WithTracing(tracing =>
                {
                    tracing.AddSource(builder.Environment.ApplicationName)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation();
                });

            var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
            if (useOtlpExporter)
            {
                otel.UseOtlpExporter();
            }

            return builder;
        }

        /// <summary>
        ///     Adds default health checks for the application.
        /// </summary>
        public IHostApplicationBuilder AddDefaultHealthChecks()
        {
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

            return builder;
        }
    }
}