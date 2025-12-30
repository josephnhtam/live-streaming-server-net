using LiveStreamingServerNet.WebRTC.Stun.Internal;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal partial class IceAgent
    {
        private class StunMessageHandler : IStunMessageHandler
        {
            private readonly IceAgent _agent;
            private readonly IceCredentials _credential;
            private readonly string _expectedUsername;

            public StunMessageHandler(IceAgent agent)
            {
                _agent = agent;
                _credential = agent._credentials;
                _expectedUsername = $"{_credential.UFragLocal}:{_credential.UFragRemote}";
            }

            public async ValueTask<StunMessage?> HandleRequestAsync(
                StunMessage message,
                UnknownAttributes? unknownAttributes,
                IPEndPoint remoteEndPoint,
                CancellationToken cancellation = default)
            {
                return message.Method switch
                {
                    StunConstants.BindingRequestMethod =>
                        await HandleBindingRequestAsync(message, remoteEndPoint).ConfigureAwait(false),

                    _ => null
                };
            }

            private ValueTask<StunMessage?> HandleBindingRequestAsync(
                StunMessage request,
                IPEndPoint remoteEndPoint)
            {
                if (!VerifyRequest(request))
                {
                    return ValueTask.FromResult<StunMessage?>(
                        CreateErrorResponse(request, 400, "Bad Request"));
                }

                var response = new StunMessage(
                    request.TransactionId,
                    StunClass.SuccessResponse,
                    request.Method,
                    [new XorMappedAddressAttribute(remoteEndPoint)]);

                response = request.Attributes.OfType<MessageIntegritySha256Attribute>().Any()
                    ? response.WithMessageIntegritySha256(_credential.PwdLocalBytes)
                    : response.WithMessageIntegrity(_credential.PwdLocalBytes);

                return ValueTask.FromResult<StunMessage?>(response.WithFingerprint());
            }

            private bool VerifyRequest(StunMessage request)
            {
                try
                {
                    var usernameAttribute = request.Attributes
                        .OfType<UsernameAttribute>()
                        .FirstOrDefault();

                    if (usernameAttribute == null || usernameAttribute.Username != _expectedUsername)
                    {
                        return false;
                    }

                    var messageIntegrityAttribute = request.Attributes
                        .OfType<MessageIntegrityAttribute>()
                        .FirstOrDefault();

                    var messageIntegritySha256Attribute = request.Attributes
                        .OfType<MessageIntegritySha256Attribute>()
                        .FirstOrDefault();

                    if (messageIntegrityAttribute == null && messageIntegritySha256Attribute == null)
                    {
                        return false;
                    }

                    if (messageIntegrityAttribute != null &&
                        !messageIntegrityAttribute.Verify(_credential.PwdLocalBytes))
                    {
                        return false;
                    }

                    if (messageIntegritySha256Attribute != null &&
                        !messageIntegritySha256Attribute.Verify(_credential.PwdLocalBytes))
                    {
                        return false;
                    }

                    var fingerprintAttribute = request.Attributes
                        .OfType<FingerprintAttribute>()
                        .FirstOrDefault();

                    if (fingerprintAttribute?.Verify() != true)
                    {
                        return false;
                    }

                    return true;
                }
                catch (Exception)
                {
                    // todo: add logs
                    return false;
                }
            }

            private StunMessage CreateErrorResponse(
                StunMessage request,
                ushort errorCode,
                string reason)
            {
                return new StunMessage(
                        request.TransactionId,
                        StunClass.ErrorResponse,
                        request.Method,
                        [new ErrorCodeAttribute(errorCode, reason)])
                    .WithFingerprint();
            }

            public ValueTask HandleIndicationAsync(
                StunMessage message,
                UnknownAttributes? unknownAttributes,
                IPEndPoint remoteEndPoint,
                CancellationToken cancellation = default)
            {
                return ValueTask.CompletedTask;
            }
        }
    }
}
