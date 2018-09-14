﻿using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.TimeSyncer;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;

namespace LunaClient.Systems.VesselDockSys
{
    public class VesselDockMessageSender : SubSystem<VesselDockSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg)));
        }

        public void SendDockInformation(Guid weakVesselId, uint weakVesselPersistentId, Vessel dominantVessel, int subspaceId)
        {
            var vesselBytes = VesselSerializer.SerializeVessel(dominantVessel.BackupVessel());
            if (vesselBytes.Length > 0)
            {
                CreateAndSendDockMessage(weakVesselId, weakVesselPersistentId, dominantVessel.id, dominantVessel.persistentId, subspaceId, vesselBytes);
            }
        }

        public void SendDockInformation(Guid weakVesselId, uint weakVesselPersistentId, Vessel dominantVessel, int subspaceId, ProtoVessel finalDominantVesselProto)
        {
            if (finalDominantVesselProto != null)
            {
                var vesselBytes = VesselSerializer.SerializeVessel(finalDominantVesselProto);
                if (vesselBytes.Length > 0)
                {
                    CreateAndSendDockMessage(weakVesselId, weakVesselPersistentId, dominantVessel.id, dominantVessel.persistentId, subspaceId, vesselBytes);
                }
            }
            else
            {
                SendDockInformation(weakVesselId, weakVesselPersistentId, dominantVessel, subspaceId);
            }
        }

        private void CreateAndSendDockMessage(Guid weakVesselId, uint weakPersistentId, Guid dominantVesselId, uint dominantPersistentId, int subspaceId, byte[] vesselBytes)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselDockMsgData>();
            msgData.GameTime = TimeSyncerSystem.UniversalTime;
            msgData.WeakVesselId = weakVesselId;
            msgData.WeakVesselPersistentId = weakPersistentId;
            msgData.DominantVesselId = dominantVesselId;
            msgData.DominantVesselPersistentId = dominantPersistentId;

            msgData.FinalVesselData = vesselBytes;
            msgData.NumBytes = vesselBytes.Length;
            msgData.SubspaceId = subspaceId;

            SendMessage(msgData);
        }
    }
}
