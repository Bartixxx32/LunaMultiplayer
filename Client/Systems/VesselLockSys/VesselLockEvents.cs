﻿using LunaClient.Base;
using LunaClient.Extensions;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselFlightStateSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.VesselUtilities;
using LunaCommon.Locks;

namespace LunaClient.Systems.VesselLockSys
{
    public class VesselLockEvents : SubSystem<VesselLockSystem>
    {
        /// <summary>
        /// This event is called after a vessel has changed. Also called when starting a flight
        /// </summary>
        public void OnVesselChange(Vessel vessel)
        {
            //Safety check
            if (vessel == null) return;

            //In case we are reloading our current own vessel we DON'T want to release our locks
            //As that would mean that an spectator could get the control of our vessel while we are reloading it.
            //Therefore we just ignore this whole thing to avoid releasing our locks.
            //Reloading our own current vessel is a bad practice so this case should not happen anyway...
            if (LockSystem.LockQuery.GetControlLockOwner(vessel.id) == SettingsSystem.CurrentSettings.PlayerName)
                return;

            //Release all vessel CONTROL locks as we are switching to a NEW vessel.
            LockSystem.Singleton.ReleasePlayerLocks(LockType.Control);

            if (LockSystem.LockQuery.ControlLockExists(vessel.id) && !LockSystem.LockQuery.ControlLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName))
            {
                //We switched to a vessel that is controlled by another player so start spectating
                System.StartSpectating(vessel.id);
            }
            else
            {
                LockSystem.Singleton.AcquireControlLock(vessel.persistentId, vessel.id);

                //Send the vessel that we switched to. It might be a kerbal going eva for example and the other players won't have it
                VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel, false);
            }
        }

        /// <summary>
        /// Event called when switching scene and before reaching the other scene
        /// </summary>
        internal void OnSceneRequested(GameScenes requestedScene)
        {
            if (requestedScene != GameScenes.FLIGHT)
            {
                InputLockManager.RemoveControlLock(VesselLockSystem.SpectateLock);
                VesselLockSystem.Singleton.StopSpectating();
            }
        }

        /// <summary>
        /// Be extra sure that we remove the spectate lock
        /// </summary>
        public void LevelLoaded(GameScenes data)
        {
            if (data != GameScenes.FLIGHT)
            {
                InputLockManager.RemoveControlLock(VesselLockSystem.SpectateLock);
                VesselLockSystem.Singleton.StopSpectating();
            }
        }

        /// <summary>
        /// When a vessel gets loaded try to acquire it's update lock if we can
        /// </summary>
        public void VesselLoaded(Vessel vessel)
        {
            if (!LockSystem.LockQuery.UpdateLockExists(vessel.id))
            {
                LockSystem.Singleton.AcquireUpdateLock(vessel.persistentId, vessel.id);
            }
        }

        /// <summary>
        /// If we get the Update lock, force the getting of the unloaded update lock.
        /// If we get a control lock, force getting the update and unloaded update
        /// </summary>
        public void LockAcquire(LockDefinition lockDefinition)
        {
            if (lockDefinition.PlayerName != SettingsSystem.CurrentSettings.PlayerName)
                return;

            switch (lockDefinition.Type)
            {
                case LockType.Control:
                    VesselLockSystem.Singleton.StopSpectating();
                    LockSystem.Singleton.AcquireUpdateLock(lockDefinition.VesselPersistentId, lockDefinition.VesselId, true);
                    LockSystem.Singleton.AcquireUnloadedUpdateLock(lockDefinition.VesselPersistentId, lockDefinition.VesselId, true);
                    LockSystem.Singleton.AcquireKerbalLock(lockDefinition.VesselPersistentId, lockDefinition.VesselId, true);

                    //As we got control of that vessel, remove its FS and position updates
                    VesselCommon.RemoveVesselFromSystems(lockDefinition.VesselId);
                    break;
                case LockType.Update:
                    LockSystem.Singleton.AcquireUnloadedUpdateLock(lockDefinition.VesselPersistentId, lockDefinition.VesselId, true);
                    LockSystem.Singleton.AcquireKerbalLock(lockDefinition.VesselPersistentId, lockDefinition.VesselId, true);
                    break;
            }
        }

        /// <summary>
        /// If a player releases an update or unloadedupdate lock try to get it.
        /// If he releases a control lock and we are spectating try to get the current vessel control lock
        /// </summary>
        public void LockReleased(LockDefinition lockDefinition)
        {
            switch (lockDefinition.Type)
            {
                case LockType.Control:
                    //Someone stopped controlling a vessel so remove it from the flight state system
                    VesselFlightStateSystem.Singleton.RemoveVessel(lockDefinition.VesselId);

                    if (FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.id == lockDefinition.VesselId)
                    {
                        LockSystem.Singleton.AcquireControlLock(lockDefinition.VesselPersistentId, lockDefinition.VesselId);
                    }
                    break;
                case LockType.UnloadedUpdate:
                case LockType.Update:
                    var vessel = FlightGlobals.fetch.FindVessel(lockDefinition.VesselPersistentId, lockDefinition.VesselId);
                    if (vessel != null)
                    {
                        switch (lockDefinition.Type)
                        {
                            case LockType.Update:
                                LockSystem.Singleton.AcquireUpdateLock(lockDefinition.VesselPersistentId, lockDefinition.VesselId);
                                break;
                            case LockType.UnloadedUpdate:
                                LockSystem.Singleton.AcquireUnloadedUpdateLock(lockDefinition.VesselPersistentId, lockDefinition.VesselId);
                                break;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// When a vessel is being unloaded, release it's update lock
        /// </summary>
        public void VesselUnloading(Vessel vessel)
        {
            if (!LockSystem.LockQuery.UpdateLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName))
                LockSystem.Singleton.ReleaseUpdateLock(vessel.id, vessel.persistentId);
        }
    }
}
