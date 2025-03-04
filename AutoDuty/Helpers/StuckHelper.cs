﻿using AutoDuty.IPC;
using ECommons.DalamudServices;
using ECommons.Throttlers;
using System;
using System.Numerics;

namespace AutoDuty.Helpers
{
    internal static class StuckHelper
    {
        internal static Vector3 LastPosition = Vector3.Zero;
        internal static long LastPositionUpdate = 0;

        internal unsafe static bool IsStuck()
        {
            if (!Player.Available) return false;
            if (!VNavmesh_IPCSubscriber.Path_IsRunning())
            {
                LastPositionUpdate = Environment.TickCount64;
            }
            else
            {
                if (Vector3.Distance(LastPosition, Player.Position) > 0.5f)
                {
                    LastPositionUpdate = Environment.TickCount64;
                    LastPosition = Player.Position;
                }
            }

            if (Environment.TickCount64 - LastPositionUpdate > 500 && EzThrottler.Throttle("RequeueMoveTo", 1000))
            {
                Svc.Log.Debug($"Stuck pathfinding.");
                return true;
            }

            return false;
        }
    }
}
