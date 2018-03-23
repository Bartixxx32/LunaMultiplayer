﻿using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaCommon.Message.Server
{
    public class ShareProgressSrvMsg : SrvMsgBase<ShareProgressBaseMsgData>
    {
        /// <inheritdoc />
        internal ShareProgressSrvMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(ShareProgressSrvMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)ShareProgressMessageType.FundsUpdate] = typeof(ShareProgressFundsMsgData),
            [(ushort)ShareProgressMessageType.ScienceUpdate] = typeof(ShareProgressScienceMsgData),
            [(ushort)ShareProgressMessageType.ReputationUpdate] = typeof(ShareProgressReputationMsgData),
            [(ushort)ShareProgressMessageType.TechnologyUpdate] = typeof(ShareProgressTechnologyMsgData),
            [(ushort)ShareProgressMessageType.ContractUpdate] = typeof(ShareProgressContractMsgData),
            [(ushort)ShareProgressMessageType.MilestoneUpdate] = typeof(ShareProgressMilestoneMsgData),
        };

        public override ServerMessageType MessageType => ServerMessageType.ShareProgress;

        protected override int DefaultChannel => 19;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}