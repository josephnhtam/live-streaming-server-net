using Microsoft.Extensions.Logging;
using System.Net;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Debug, "Candidate pair added (Identifier={Identifier}, Role={Role}, Local={LocalEndPoint}, Remote={RemoteEndPoint}, LocalType={LocalType}, RemoteType={RemoteType}, State={State}, Priority={Priority})")]
        public static partial void CandidatePairAdded(this ILogger logger, string identifier, IceRole role, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, IceCandidateType localType, IceCandidateType remoteType, IceCandidatePairState state, ulong priority);

        [LoggerMessage(LogLevel.Trace, "Sending connectivity check (Identifier={Identifier}, Role={Role}, Local={LocalEndPoint}, Remote={RemoteEndPoint}, Foundation={Foundation}, Nominating={IsNominating})")]
        public static partial void SendingConnectivityCheck(this ILogger logger, string identifier, IceRole role, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string foundation, bool isNominating);

        [LoggerMessage(LogLevel.Debug, "Connectivity check succeeded (Identifier={Identifier}, Role={Role}, Local={LocalEndPoint}, Remote={RemoteEndPoint}, Foundation={Foundation}, Nominating={IsNominating})")]
        public static partial void ConnectivityCheckSucceeded(this ILogger logger, string identifier, IceRole role, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string foundation, bool isNominating);

        [LoggerMessage(LogLevel.Debug, "Connectivity check failed (Identifier={Identifier}, Role={Role}, Local={LocalEndPoint}, Remote={RemoteEndPoint}, Foundation={Foundation}, Nominating={IsNominating}, Reason={Reason})")]
        public static partial void ConnectivityCheckFailed(this ILogger logger, string identifier, IceRole role, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string foundation, bool isNominating, string reason);

        [LoggerMessage(LogLevel.Debug, "Nominating pair (Identifier={Identifier}, Role={Role}, Local={LocalEndPoint}, Remote={RemoteEndPoint})")]
        public static partial void NominatingPair(this ILogger logger, string identifier, IceRole role, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint);

        [LoggerMessage(LogLevel.Debug, "Pair selected (Identifier={Identifier}, Role={Role}, Local={LocalEndPoint}, Remote={RemoteEndPoint})")]
        public static partial void PairSelected(this ILogger logger, string identifier, IceRole role, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint);

        [LoggerMessage(LogLevel.Trace, "GetNextPair (Identifier={Identifier}, Role={Role}, Local={LocalEndPoint}, Remote={RemoteEndPoint}, State={State}, NominationState={NominationState}, IsTriggered={IsTriggered}, TriggeredQueueCount={TriggeredQueueCount}, TotalPairs={TotalPairs})")]
        public static partial void GetNextPair(this ILogger logger, string identifier, IceRole role, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, IceCandidatePairState state, IceCandidateNominationState nominationState, bool isTriggered, int triggeredQueueCount, int totalPairs);

        [LoggerMessage(LogLevel.Trace, "TriggerCheck (Identifier={Identifier}, Role={Role}, Local={LocalEndPoint}, Remote={RemoteEndPoint}, State={State}, NominationState={NominationState}, TriggeredQueueCount={TriggeredQueueCount}, Reason={Reason})")]
        public static partial void TriggerCheck(this ILogger logger, string identifier, IceRole role, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, IceCandidatePairState state, IceCandidateNominationState nominationState, int triggeredQueueCount, string reason);

        [LoggerMessage(LogLevel.Debug, "State changed (Identifier={Identifier}, Role={Role}, NewState={NewState})")]
        public static partial void StateChanged(this ILogger logger, string identifier, IceRole role, IceConnectionState newState);

        [LoggerMessage(LogLevel.Debug, "Local candidate gathered (Identifier={Identifier}, Role={Role}, EndPoint={EndPoint}, Type={Type})")]
        public static partial void LocalCandidateGathered(this ILogger logger, string identifier, IceRole role, IPEndPoint? endPoint, IceCandidateType? type);

        [LoggerMessage(LogLevel.Debug, "Local gathering complete (Identifier={Identifier}, Role={Role})")]
        public static partial void LocalGatheringComplete(this ILogger logger, string identifier, IceRole role);

        [LoggerMessage(LogLevel.Debug, "Remote candidate added (Identifier={Identifier}, Role={Role}, EndPoint={EndPoint}, Type={Type})")]
        public static partial void RemoteCandidateAdded(this ILogger logger, string identifier, IceRole role, IPEndPoint endPoint, IceCandidateType type);

        [LoggerMessage(LogLevel.Debug, "Remote gathering complete (Identifier={Identifier}, Role={Role})")]
        public static partial void RemoteGatheringComplete(this ILogger logger, string identifier, IceRole role);

        [LoggerMessage(LogLevel.Error, "ICE agent error (Identifier={Identifier}, Role={Role})")]
        public static partial void IceAgentError(this ILogger logger, string identifier, IceRole role, Exception exception);

        [LoggerMessage(LogLevel.Debug, "ICE agent started (Identifier={Identifier}, Role={Role})")]
        public static partial void IceAgentStarted(this ILogger logger, string identifier, IceRole role);

        [LoggerMessage(LogLevel.Debug, "ICE agent stopped (Identifier={Identifier}, Role={Role})")]
        public static partial void IceAgentStopped(this ILogger logger, string identifier, IceRole role);

        [LoggerMessage(LogLevel.Debug, "Role conflict detected (Identifier={Identifier}, Role={Role}, RemoteTieBreaker={RemoteTieBreaker}, LocalTieBreaker={LocalTieBreaker})")]
        public static partial void RoleConflictDetected(this ILogger logger, string identifier, IceRole role, ulong remoteTieBreaker, ulong localTieBreaker);

        [LoggerMessage(LogLevel.Debug, "Role conflict error received (Identifier={Identifier}, Role={Role})")]
        public static partial void RoleConflictErrorReceived(this ILogger logger, string identifier, IceRole role);

        [LoggerMessage(LogLevel.Information, "Role switched (Identifier={Identifier}, OldRole={OldRole}, NewRole={NewRole})")]
        public static partial void RoleSwitched(this ILogger logger, string identifier, IceRole oldRole, IceRole newRole);

        [LoggerMessage(LogLevel.Trace, "Priority updated (Identifier={Identifier}, Role={Role}, Local={LocalEndPoint}, Remote={RemoteEndPoint}, OldPriority={OldPriority}, NewPriority={NewPriority})")]
        public static partial void PriorityUpdated(this ILogger logger, string identifier, IceRole role, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, ulong oldPriority, ulong newPriority);

        [LoggerMessage(LogLevel.Debug, "Use candidate received (Identifier={Identifier}, Role={Role}, Local={LocalEndPoint}, Remote={RemoteEndPoint})")]
        public static partial void UseCandidateReceived(this ILogger logger, string identifier, IceRole role, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint);

        // IceCandidateGatherer logging
        [LoggerMessage(LogLevel.Debug, "Gathering started (Identifier={Identifier})")]
        public static partial void GatheringStarted(this ILogger logger, string identifier);

        [LoggerMessage(LogLevel.Debug, "Host candidate created (Identifier={Identifier}, EndPoint={EndPoint})")]
        public static partial void HostCandidateCreated(this ILogger logger, string identifier, IPEndPoint endPoint);

        [LoggerMessage(LogLevel.Warning, "Failed to create host candidate (Identifier={Identifier}, Address={Address})")]
        public static partial void FailedToCreateHostCandidate(this ILogger logger, string identifier, IPAddress address, Exception exception);

        [LoggerMessage(LogLevel.Debug, "Server reflexive candidate created (Identifier={Identifier}, EndPoint={EndPoint}, BoundEndPoint={BoundEndPoint})")]
        public static partial void ServerReflexiveCandidateCreated(this ILogger logger, string identifier, IPEndPoint endPoint, IPEndPoint boundEndPoint);

        [LoggerMessage(LogLevel.Warning, "Failed to create server reflexive candidate (Identifier={Identifier}, BoundEndPoint={BoundEndPoint})")]
        public static partial void FailedToCreateServerReflexiveCandidate(this ILogger logger, string identifier, IPEndPoint boundEndPoint, Exception exception);

        [LoggerMessage(LogLevel.Debug, "STUN server resolved (Identifier={Identifier}, Server={Server}, EndPoint={EndPoint})")]
        public static partial void StunServerResolved(this ILogger logger, string identifier, string server, IPEndPoint endPoint);

        [LoggerMessage(LogLevel.Warning, "Failed to resolve STUN server (Identifier={Identifier}, Server={Server})")]
        public static partial void FailedToResolveStunServer(this ILogger logger, string identifier, string server, Exception exception);

        [LoggerMessage(LogLevel.Error, "Gathering error (Identifier={Identifier})")]
        public static partial void GatheringError(this ILogger logger, string identifier, Exception exception);

        [LoggerMessage(LogLevel.Debug, "Gathering complete (Identifier={Identifier})")]
        public static partial void GatheringComplete(this ILogger logger, string identifier);
    }
}
