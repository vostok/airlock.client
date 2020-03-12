using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Commons.Collections;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Queries;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Internal;
using Vostok.Hercules.Client.Serialization.Readers;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    /// <inheritdoc />
    [PublicAPI]
    public class HerculesStreamClient<T> : IHerculesStreamClient<T>
    {
        private readonly HerculesStreamClientSettings<T> settings;
        private readonly BufferPool bufferPool;
        private readonly ILog log;
        private readonly StreamApiRequestSender client;

        public HerculesStreamClient([NotNull] HerculesStreamClientSettings<T> settings, [CanBeNull] ILog log)
        {
            this.settings = settings;
            this.log = log = (log ?? LogProvider.Get()).ForContext<HerculesStreamClient>();

            bufferPool = new BufferPool(settings.MaxPooledBufferSize, settings.MaxPooledBuffersPerBucket);

            client = new StreamApiRequestSender(settings.Cluster, log, bufferPool, settings.AdditionalSetup);
        }

        /// <inheritdoc />
        public async Task<ReadStreamResult<T>> ReadAsync(ReadStreamQuery query, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            var result = await client.ReadAsync(query, settings.ApiKeyProvider(), timeout, cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccessful)
                return new ReadStreamResult<T>(result.Status, null, result.ErrorDetails);

            var payload = result.Payload;

            try
            {
                var events = EventsBinaryReader.Read(payload.Content, settings.EventBuilderProvider, log);
                return new ReadStreamResult<T>(result.Status, new ReadStreamPayload<T>(events, payload.Next));
            }
            catch (Exception error)
            {
                log.Warn(error);
                return new ReadStreamResult<T>(HerculesStatus.UnknownError, null, error.Message);
            }
            finally
            {
                payload.Dispose();
            }
        }

        public Task<SeekToEndStreamResult> SeekToEndAsync(SeekToEndStreamQuery query, TimeSpan timeout, CancellationToken cancellationToken = new CancellationToken()) =>
            client.SeekToEndAsync(query, timeout, cancellationToken);
    }
}