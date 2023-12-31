﻿using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Handshakes
{
    internal class DiffieHellmanKeyExchange
    {
        private readonly DHParameters _dhParams;

        public DHPublicKeyParameters PublicKey { get; }
        public DHPrivateKeyParameters PrivateKey { get; }

        public DiffieHellmanKeyExchange()
        {
            var generator = new DHParametersGenerator();
            generator.Init(1024, 20, new SecureRandom());

            _dhParams = generator.GenerateParameters();
            var keyGenParams = new DHKeyGenerationParameters(new SecureRandom(), _dhParams);
            var keyGen = new DHKeyPairGenerator();
            keyGen.Init(keyGenParams);

            var keyPair = keyGen.GenerateKeyPair();
            PublicKey = (DHPublicKeyParameters)keyPair.Public;
            PrivateKey = (DHPrivateKeyParameters)keyPair.Private;
        }

        public byte[] ComputeSharedKey(byte[] clientPublicKey)
        {
            var dhPubKey = new DHPublicKeyParameters(new BigInteger(1, clientPublicKey), _dhParams);

            var dhAgree = new DHBasicAgreement();
            dhAgree.Init(PrivateKey);

            var agreement = dhAgree.CalculateAgreement(dhPubKey);

            return agreement.ToByteArrayUnsigned();
        }
    }
}
