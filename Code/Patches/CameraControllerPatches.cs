// <copyright file="CameraControllerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache Licence, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace FreeRangeCamera
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using Colossal.Mathematics;
    using Game;
    using Game.Simulation;
    using HarmonyLib;

    /// <summary>
    /// Harmony patches for <see cref="CameraController"/> to implement per-tile cost limits.
    /// </summary>
    [HarmonyPatch(typeof(CameraController))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    internal static class CameraControllerPatches
    {
        /// <summary>
        /// Harmony transpiler for <see cref="CameraController.UpdateCamera"/> to override map edge bounds check.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("UpdateCamera")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UpdateStatusTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            Mod.Instance.Log.Info("transpiling " + original.DeclaringType + '.' + original.Name);

            // Look for call to this.
            MethodInfo getBounds = AccessTools.Method(typeof(TerrainUtils), nameof(TerrainUtils.GetBounds));

            // Parse instructions.
            IEnumerator<CodeInstruction> instructionEnumerator = instructions.GetEnumerator();
            while (instructionEnumerator.MoveNext())
            {
                CodeInstruction instruction = instructionEnumerator.Current;

                // Look for call to TerrainUtils.GetBounds.
                if (instruction.Calls(getBounds))
                {
                    Mod.Instance.Log.Debug("found call to TerrainUtils.GetBounds");

                    // Drop instruction argument and duplicate previous value on stack (m_Pivot).
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CameraControllerPatches), nameof(GetCustomBounds)));

                    continue;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony prefix to <c>CameraController.zoomRange</c> to implement expanded zoom range.
        /// Done via prefix instead of using reflection to change backing fields due to in-lining.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        /// <returns>Always <c>false</c> (never original method).</returns>
        [HarmonyPatch("zoomRange", MethodType.Getter)]
        [HarmonyPrefix]
        internal static bool ZoomRangePrefix(ref Bounds1 __result)
        {
            __result = new Bounds1(1f, 80000f);
            return false;
        }

        /// <summary>
        /// Returns custom map bounds to repace the game default.
        /// </summary>
        /// <param name="data">Terrain height data reference.</param>
        /// <returns>Custom camera bounds.</returns>
        private static Bounds3 GetCustomBounds(ref TerrainHeightData data) => new Bounds3(-data.offset, ((data.resolution - 1) / data.scale) - data.offset) * 8f;
    }
}
