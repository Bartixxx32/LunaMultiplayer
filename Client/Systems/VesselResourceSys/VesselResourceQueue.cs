﻿using LunaClient.Base;
using LunaCommon.Message.Data.Vessel;

namespace LunaClient.Systems.VesselResourceSys
{
    public class VesselResourceQueue : CachedConcurrentQueue<VesselResource, VesselResourceMsgData>
    {
        protected override void AssignFromMessage(VesselResource value, VesselResourceMsgData msgData)
        {
            value.GameTime = msgData.GameTime;
            value.VesselId = msgData.VesselId;
            value.VesselPersistentId = msgData.VesselPersistentId;

            value.ResourcesCount = msgData.ResourcesCount;
            if (value.Resources.Length < msgData.ResourcesCount)
                value.Resources = new VesselResourceInfo[msgData.ResourcesCount];

            for (var i = 0; i < msgData.ResourcesCount; i++)
            {
                if (value.Resources[i] == null)
                    value.Resources[i] = new VesselResourceInfo();

                value.Resources[i].Amount = msgData.Resources[i].Amount;
                value.Resources[i].FlowState = msgData.Resources[i].FlowState;
                value.Resources[i].PartFlightId = msgData.Resources[i].PartFlightId;
                value.Resources[i].PartPersistentId = msgData.Resources[i].PartPersistentId;
                value.Resources[i].ResourceName = msgData.Resources[i].ResourceName.Clone() as string;
            }
        }
    }
}
