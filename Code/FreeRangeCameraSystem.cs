// <copyright file="FreeRangeCameraSystem.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache Licence, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace FreeRangeCamera
{
    using System.Reflection;
    using Cinemachine;
    using Colossal.Logging;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Rendering;
    using HarmonyLib;
    using Unity.Entities;

    /// <summary>
    /// The 529 tile mod system.
    /// </summary>
    internal sealed partial class FreeRangeCameraSystem : GameSystemBase
    {
        private ILog _log;

        /// <summary>
        /// Called by the game when loading is complete.
        /// </summary>
        /// <param name="purpose">Loading purpose.</param>
        /// <param name="mode">Current game mode.</param>
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            _log.Info("game loading complete");
            base.OnGameLoadingComplete(purpose, mode);

            if (World.GetOrCreateSystemManaged<CameraUpdateSystem>().gamePlayController is CameraController cameraController)
            {
                // Set camera far clip plane.
                CinemachineVirtualCamera virtualCamera = cameraController.GetComponent<CinemachineVirtualCamera>();
                if (virtualCamera is not null)
                {
                    virtualCamera.m_Lens.FarClipPlane = 150000f;
                }

                // Set stored initial far clip plane.
                if (AccessTools.Field(typeof(CameraController), "m_InitialFarClip") is FieldInfo farClipField)
                {
                    farClipField.SetValue(cameraController, 150000f);
                }
            }
        }

        /// <summary>
        /// Called when the system is created.
        /// </summary>
        protected override void OnCreate()
        {
            base.OnCreate();

            // Set log.
            _log = Mod.Instance.Log;
        }

        /// <summary>
        /// Called every update.
        /// </summary>
        protected override void OnUpdate()
        {
        }
    }
}
