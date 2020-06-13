/*
 * This file is the Mod Entry Point. So basically the OnLoad here creates a 
 * ModdedBlockController, which adds triggers for modifying the CanonBlock when
 * it is initialized.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;
using System.Collections;

namespace ShowTrajectory
{
    public class Mod : ModEntryPoint
    {
        GameObject mod;
        public override void OnLoad()
        {
            // Called when the mod is loaded.
            mod = new GameObject("Predict Cannon Trajectory Mod");
            UnityEngine.Object.DontDestroyOnLoad(mod);
            mod.AddComponent<ModdedBlockController>();
#if DEBUG
            ConsoleController.ShowMessage("ShowTrajectory mod loaded");
#endif
        }

    }
}