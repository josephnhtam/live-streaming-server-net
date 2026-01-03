using Microsoft.Extensions.Logging;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal.Logging
{
  internal static partial class LoggerExtensions
  {
    [LoggerMessage(LogLevel.Debug, "Candidate pair added (Local={LocalEndPoint}, Remote={RemoteEndPoint}, LocalType={LocalType}, RemoteType={RemoteType}, State={State}, Priority={Priority})")]
    public static partial void CandidatePairAdded(this ILogger logger, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, IceCandidateType localType, IceCandidateType remoteType, IceCandidatePairState state, ulong priority);

    [LoggerMessage(LogLevel.Debug, "Connectivity check succeeded (Local={LocalEndPoint}, Remote={RemoteEndPoint}, Nominating={IsNominating})")]
    public static partial void ConnectivityCheckSucceeded(this ILogger logger, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, bool isNominating);

    [LoggerMessage(LogLevel.Debug, "Connectivity check failed (Local={LocalEndPoint}, Remote={RemoteEndPoint}, Nominating={IsNominating}, Reason={Reason})")]
    public static partial void ConnectivityCheckFailed(this ILogger logger, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, bool isNominating, string reason);

    [LoggerMessage(LogLevel.Debug, "Nominating pair (Local={LocalEndPoint}, Remote={RemoteEndPoint})")]
    public static partial void NominatingPair(this ILogger logger, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint);

    [LoggerMessage(LogLevel.Debug, "Pair selected (Local={LocalEndPoint}, Remote={RemoteEndPoint})")]
    public static partial void PairSelected(this ILogger logger, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint);

    [LoggerMessage(LogLevel.Trace, "GetNextPair (Local={LocalEndPoint}, Remote={RemoteEndPoint}, State={State}, NominationState={NominationState}, IsTriggered={IsTriggered}, TriggeredQueueCount={TriggeredQueueCount}, TotalPairs={TotalPairs})")]
    public static partial void GetNextPair(this ILogger logger, IPEndPoint? localEndPoint, IPEndPoint? remoteEndPoint, IceCandidatePairState? state, IceCandidateNominationState? nominationState, bool isTriggered, int triggeredQueueCount, int totalPairs);

    [LoggerMessage(LogLevel.Trace, "TriggerCheck (Local={LocalEndPoint}, Remote={RemoteEndPoint}, State={State}, NominationState={NominationState}, TriggeredQueueCount={TriggeredQueueCount}, Reason={Reason})")]
    public static partial void TriggerCheck(this ILogger logger, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, IceCandidatePairState state, IceCandidateNominationState nominationState, int triggeredQueueCount, string reason);

    [LoggerMessage(LogLevel.Debug, "State changed (NewState={NewState})")]
    public static partial void StateChanged(this ILogger logger, IceConnectionState newState);

    [LoggerMessage(LogLevel.Debug, "Local candidate gathered (EndPoint={EndPoint}, Type={Type})")]
    public static partial void LocalCandidateGathered(this ILogger logger, IPEndPoint? endPoint, IceCandidateType? type);

    [LoggerMessage(LogLevel.Debug, "Remote candidate added (EndPoint={EndPoint}, Type={Type})")]
    public static partial void RemoteCandidateAdded(this ILogger logger, IPEndPoint? endPoint, IceCandidateType? type);

    [LoggerMessage(LogLevel.Error, "ICE agent error")]
    public static partial void IceAgentError(this ILogger logger, Exception exception);
  }
}

