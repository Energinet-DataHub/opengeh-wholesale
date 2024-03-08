// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Azure.Core;
using HealthChecks.AzureServiceBus;
using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Wholesale.Common.Infrastructure.HealthChecks.ServiceBus;

/// <summary>
/// Extension method to configure ServiceBus health checks using transport type AMQP over WebSocket.
///
/// It is curently necessary to use AMQP over WebSockets to be able to get past the firewall when
/// running our applications locally on developer machines.
///
/// Based on: https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/blob/master/src/HealthChecks.AzureServiceBus/DependencyInjection/AzureServiceBusHealthCheckBuilderExtensions.cs#L14
/// </summary>
public static class AzureServiceBusHealthCheckBuilderDh3Extensions
{
#pragma warning disable SA1310 // Field names should not contain underscore
    private const string AZUREQUEUE_NAME = "azurequeue";
    private const string AZURETOPIC_NAME = "azuretopic";
    private const string AZURESUBSCRIPTION_NAME = "azuresubscription";
#pragma warning restore SA1310 // Field names should not contain underscore

    /// <summary>
    /// Add a health check for specified Azure Service Bus Queue where the ServiceBus transport is set to AmqpOverWebSocket.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The azure service bus connection string to be used.</param>
    /// <param name="queueName">The name of the queue to check.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azurequeue' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusQueueUsingWebSockets(
        this IHealthChecksBuilder builder,
        string connectionString,
        string queueName,
        Action<AzureServiceBusQueueHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddAzureServiceBusQueueUsingWebSockets(
            _ => connectionString,
            _ => queueName,
            configure,
            name,
            failureStatus,
            tags,
            timeout);
    }

    /// <summary>
    /// Add a health check for specified Azure Service Bus Queue where the ServiceBus transport is set to AmqpOverWebSocket.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">A factory to build the azure service bus connection string to be used.</param>
    /// <param name="queueNameFactory">A factory to build the queue name to check.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azurequeue' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusQueueUsingWebSockets(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> connectionStringFactory,
        Func<IServiceProvider, string> queueNameFactory,
        Action<AzureServiceBusQueueHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionStringFactory);
        Guard.ThrowIfNull(queueNameFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZUREQUEUE_NAME,
            sp =>
            {
                var options = new AzureServiceBusQueueHealthCheckOptions(queueNameFactory(sp))
                {
                    ConnectionString = connectionStringFactory(sp),
                };

                configure?.Invoke(options);
                return new AzureServiceBusQueueHealthCheck(options, new WebSocketServiceBusClientProvider());
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for specified Azure Service Bus Queue where the ServiceBus transport is set to AmqpOverWebSocket.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="fullyQualifiedNamespace">The azure service bus fully qualified namespace to be used, format sb://myservicebus.servicebus.windows.net/.</param>
    /// <param name="queueName">The name of the queue to check.</param>
    /// <param name="tokenCredential">The token credential for authentication.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azurequeue' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusQueueUsingWebSockets(
        this IHealthChecksBuilder builder,
        string fullyQualifiedNamespace,
        string queueName,
        TokenCredential tokenCredential,
        Action<AzureServiceBusQueueHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddAzureServiceBusQueueUsingWebSockets(
            _ => fullyQualifiedNamespace,
            _ => queueName,
            _ => tokenCredential,
            configure,
            name,
            failureStatus,
            tags,
            timeout);
    }

    /// <summary>
    /// Add a health check for specified Azure Service Bus Queue where the ServiceBus transport is set to AmqpOverWebSocket.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="fullyQualifiedNamespaceFactory">A factory to build the azure service bus fully qualified namespace to be used, format sb://myservicebus.servicebus.windows.net/.</param>
    /// <param name="queueNameFactory">A factory to build the name of the queue to check.</param>
    /// <param name="tokenCredentialFactory">A factory to build the token credential for authentication.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azurequeue' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusQueueUsingWebSockets(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> fullyQualifiedNamespaceFactory,
        Func<IServiceProvider, string> queueNameFactory,
        Func<IServiceProvider, TokenCredential> tokenCredentialFactory,
        Action<AzureServiceBusQueueHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(fullyQualifiedNamespaceFactory);
        Guard.ThrowIfNull(queueNameFactory);
        Guard.ThrowIfNull(tokenCredentialFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZUREQUEUE_NAME,
            sp =>
            {
                var options = new AzureServiceBusQueueHealthCheckOptions(queueNameFactory(sp))
                {
                    FullyQualifiedNamespace = fullyQualifiedNamespaceFactory(sp),
                    Credential = tokenCredentialFactory(sp),
                };

                configure?.Invoke(options);
                return new AzureServiceBusQueueHealthCheck(options, new WebSocketServiceBusClientProvider());
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Azure Service Bus Topic where the ServiceBus transport is set to AmqpOverWebSocket.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The Azure ServiceBus connection string to be used.</param>
    /// <param name="topicName">The name of the topic to check.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusTopicUsingWebSockets(
        this IHealthChecksBuilder builder,
        string connectionString,
        string topicName,
        Action<AzureServiceBusTopicHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddAzureServiceBusTopicUsingWebSockets(
            _ => connectionString,
            _ => topicName,
            configure,
            name,
            failureStatus,
            tags,
            timeout);
    }

    /// <summary>
    /// Add a health check for Azure Service Bus Topic where the ServiceBus transport is set to AmqpOverWebSocket.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">A factory to build the Azure ServiceBus connection string to be used.</param>
    /// <param name="topicNameFactory">A factory to build the name of the topic to check.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusTopicUsingWebSockets(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> connectionStringFactory,
        Func<IServiceProvider, string> topicNameFactory,
        Action<AzureServiceBusTopicHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionStringFactory);
        Guard.ThrowIfNull(topicNameFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZURETOPIC_NAME,
            sp =>
            {
                var options = new AzureServiceBusTopicHealthCheckOptions(topicNameFactory(sp))
                {
                    ConnectionString = connectionStringFactory(sp),
                };

                configure?.Invoke(options);
                return new AzureServiceBusTopicHealthCheck(options, new WebSocketServiceBusClientProvider());
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Azure Service Bus Topic where the ServiceBus transport is set to AmqpOverWebSocket.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="fullyQualifiedNamespace">The azure service bus fully qualified namespace to be used, format sb://myservicebus.servicebus.windows.net/.</param>
    /// <param name="topicName">The name of the topic to check.</param>
    /// <param name="tokenCredential">The token credential for authentication.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusTopicUsingWebSockets(
        this IHealthChecksBuilder builder,
        string fullyQualifiedNamespace,
        string topicName,
        TokenCredential tokenCredential,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddAzureServiceBusTopicUsingWebSockets(
            _ => fullyQualifiedNamespace,
            _ => topicName,
            _ => tokenCredential,
            configure: null,
            name,
            failureStatus,
            tags,
            timeout);
    }

    /// <summary>
    /// Add a health check for Azure Service Bus Topic where the ServiceBus transport is set to AmqpOverWebSocket.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="fullyQualifiedNamespaceFactory">A factory to build the azure service bus fully qualified namespace to be used, format sb://myservicebus.servicebus.windows.net/.</param>
    /// <param name="topicNameFactory">A factory to build the name of the topic to check.</param>
    /// <param name="tokenCredentialFactory">A factory to build the token credential for authentication.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusTopicUsingWebSockets(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> fullyQualifiedNamespaceFactory,
        Func<IServiceProvider, string> topicNameFactory,
        Func<IServiceProvider, TokenCredential> tokenCredentialFactory,
        Action<AzureServiceBusTopicHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(fullyQualifiedNamespaceFactory);
        Guard.ThrowIfNull(topicNameFactory);
        Guard.ThrowIfNull(tokenCredentialFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZURETOPIC_NAME,
            sp =>
            {
                var options = new AzureServiceBusTopicHealthCheckOptions(topicNameFactory(sp))
                {
                    FullyQualifiedNamespace = fullyQualifiedNamespaceFactory(sp),
                    Credential = tokenCredentialFactory(sp),
                };

                configure?.Invoke(options);
                return new AzureServiceBusTopicHealthCheck(options, new WebSocketServiceBusClientProvider());
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Azure Service Bus Subscription where the ServiceBus transport is set to AmqpOverWebSocket.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The Azure ServiceBus connection string to be used.</param>
    /// <param name="topicName">The name of the topic to check.</param>
    /// <param name="subscriptionName">The subscription name of the topic subscription to check.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusSubscriptionUsingWebSockets(
        this IHealthChecksBuilder builder,
        string connectionString,
        string topicName,
        string subscriptionName,
        Action<AzureServiceBusSubscriptionHealthCheckHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddAzureServiceBusSubscriptionUsingWebSockets(
            _ => connectionString,
            _ => topicName,
            _ => subscriptionName,
            configure,
            name,
            failureStatus,
            tags,
            timeout);
    }

    /// <summary>
    /// Add a health check for Azure Service Bus Subscription where the ServiceBus transport is set to AmqpOverWebSocket.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">A factory to build the Azure ServiceBus connection string to be used.</param>
    /// <param name="topicNameFactory">A factory to build the name of the topic to check.</param>
    /// <param name="subscriptionNameFactory">A factory to build the subscription name of the topic subscription to check.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusSubscriptionUsingWebSockets(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> connectionStringFactory,
        Func<IServiceProvider, string> topicNameFactory,
        Func<IServiceProvider, string> subscriptionNameFactory,
        Action<AzureServiceBusSubscriptionHealthCheckHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionStringFactory);
        Guard.ThrowIfNull(topicNameFactory);
        Guard.ThrowIfNull(subscriptionNameFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZURESUBSCRIPTION_NAME,
            sp =>
            {
                var options = new AzureServiceBusSubscriptionHealthCheckHealthCheckOptions(topicNameFactory(sp), subscriptionNameFactory(sp))
                {
                    ConnectionString = connectionStringFactory(sp),
                };

                configure?.Invoke(options);
                return new AzureServiceBusSubscriptionHealthCheck(options, new WebSocketServiceBusClientProvider());
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Azure Service Bus Subscription where the ServiceBus transport is set to AmqpOverWebSocket.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="fullyQualifiedNamespace">The azure service bus fully qualified namespace to be used, format sb://myservicebus.servicebus.windows.net/.</param>
    /// <param name="topicName">The name of the topic to check.</param>
    /// <param name="subscriptionName">The subscription name of the topic subscription to check.</param>
    /// <param name="tokenCredential">The token credential for authentication.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusSubscriptionUsingWebSockets(
        this IHealthChecksBuilder builder,
        string fullyQualifiedNamespace,
        string topicName,
        string subscriptionName,
        TokenCredential tokenCredential,
        Action<AzureServiceBusSubscriptionHealthCheckHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.AddAzureServiceBusSubscriptionUsingWebSockets(
            _ => fullyQualifiedNamespace,
            _ => topicName,
            _ => subscriptionName,
            _ => tokenCredential,
            configure,
            name,
            failureStatus,
            tags,
            timeout);
    }

    /// <summary>
    /// Add a health check for Azure Service Bus Subscription where the ServiceBus transport is set to AmqpOverWebSocket.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="fullyQualifiedNamespaceFactory">A factory to build the azure service bus fully qualified namespace to be used, format sb://myservicebus.servicebus.windows.net/.</param>
    /// <param name="topicNameFactory">A factory to build the name of the topic to check.</param>
    /// <param name="subscriptionNameFactory">A factory to build the subscription name of the topic subscription to check.</param>
    /// <param name="tokenCredentialFactory">A factory to build the token credential for authentication.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusSubscriptionUsingWebSockets(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> fullyQualifiedNamespaceFactory,
        Func<IServiceProvider, string> topicNameFactory,
        Func<IServiceProvider, string> subscriptionNameFactory,
        Func<IServiceProvider, TokenCredential> tokenCredentialFactory,
        Action<AzureServiceBusSubscriptionHealthCheckHealthCheckOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(fullyQualifiedNamespaceFactory);
        Guard.ThrowIfNull(topicNameFactory);
        Guard.ThrowIfNull(subscriptionNameFactory);
        Guard.ThrowIfNull(tokenCredentialFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZURESUBSCRIPTION_NAME,
            sp =>
            {
                var options = new AzureServiceBusSubscriptionHealthCheckHealthCheckOptions(topicNameFactory(sp), subscriptionNameFactory(sp))
                {
                    FullyQualifiedNamespace = fullyQualifiedNamespaceFactory(sp),
                    Credential = tokenCredentialFactory(sp),
                };

                configure?.Invoke(options);
                return new AzureServiceBusSubscriptionHealthCheck(options, new WebSocketServiceBusClientProvider());
            },
            failureStatus,
            tags,
            timeout));
    }

    private class Guard
    {
        /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
        /// <param name="argument">The reference type argument to validate as non-null.</param>
        /// <param name="throwOnEmptyString">Only applicable to strings.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        public static T ThrowIfNull<T>([NotNull] T? argument, bool throwOnEmptyString = false, [CallerArgumentExpression("argument")] string? paramName = null)
            where T : class
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(argument, paramName);
            if (throwOnEmptyString && argument is string s && string.IsNullOrEmpty(s))
                throw new ArgumentNullException(paramName);
#else
    if (argument is null || throwOnEmptyString && argument is string s && string.IsNullOrEmpty(s))
        throw new ArgumentNullException(paramName);
#endif
            return argument;
        }
    }
}
