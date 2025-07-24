using System;
using Unity.Netcode;
using UnityEngine;

namespace _Main.Scripts.Units.UnitCommands
{
    [Serializable]
    public struct UnitCommandData : INetworkSerializable
    {
        public UnitCommandType UnitCommandType;
        public ulong UnitId;
        public Vector3 TargetPosition;
        public ulong TargetUnit;

        public UnitCommandData(UnitCommandType unitCommandType, ulong unitId, Vector3 targetPosition, ulong targetUnit)
        {
            UnitCommandType = unitCommandType;
            UnitId = unitId;
            TargetPosition = targetPosition;
            TargetUnit = targetUnit;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref UnitCommandType);
            serializer.SerializeValue(ref UnitId);
            serializer.SerializeValue(ref TargetPosition);
            serializer.SerializeValue(ref TargetUnit);
        }
    }
}