﻿using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System;

namespace LmpCommon.Message.Data.CraftLibrary
{
    public abstract class CraftLibraryBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal CraftLibraryBaseMsgData() { }
        public override bool CompressCondition => false;
        public override ushort SubType => (ushort)(int)CraftMessageType;
        public virtual CraftMessageType CraftMessageType => throw new NotImplementedException();

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            //Nothing to implement here
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            //Nothing to implement here
        }

        internal override int InternalGetMessageSize()
        {
            return 0;
        }
    }
}
