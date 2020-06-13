/*
 * A ModdedBlockController is created when the mod is loaded, and then when this
 * is initialized it adds trigers to add custom code to all blocks of interest. 
 * The custom code we add is ModdedCannonBehaviour, of type MonoBehaviour
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
    public class ModdedBlockController : SingleInstance<ModdedBlockController>
    {
        public override string Name { get; } =
            "Cannon Trajectory Predict Controller";

        private void Awake()
        {
            Events.OnBlockInit += AddCustomCode;
        }

        /// <summary>Whether has been modded</summary>
        public static bool HasPredictCannon(BlockBehaviour block)
        {
            return block.MapperTypes.Exists(match => match.Key == "PredictCannon");
        }

        /// <summary>Add mods for each block</summary>
        private void AddCustomCode(Block block)
        {
#if DEBUG
            ConsoleController.ShowMessage("on block init");
#endif
            BlockBehaviour blockbehaviour = block.BuildingBlock.InternalObject;
            if (!HasPredictCannon(blockbehaviour))
                AddCustomCode(blockbehaviour);
        }
        /// <summary>Add mods for each block </summary>
        private void AddCustomCode(Transform block)
        {
            BlockBehaviour blockbehaviour = block.GetComponent<BlockBehaviour>();
            if (!HasPredictCannon(blockbehaviour))
                AddCustomCode(blockbehaviour);
        }
        /// <summary>Add mods for each block </summary>
        private void AddCustomCode(BlockBehaviour block)
        {
#if DEBUG
            ConsoleController.ShowMessage(string.Format("Block ID: {0}", block.BlockID.ToString()));
#endif

            if (dic_EnhancementBlock.ContainsKey(block.BlockID))
            {
                Type EB = dic_EnhancementBlock[block.BlockID];

                if (block.GetComponent(EB) == null)
                {
#if DEBUG
                    ConsoleController.ShowMessage(string.Format("Adding for Block ID: {0}", block.BlockID.ToString()));
#endif
                    block.gameObject.AddComponent(EB);
                }
            }
        }

        /// <summary>A dictionary of all custom mods for blocks to load</summary>
        public Dictionary<int, Type> dic_EnhancementBlock = new Dictionary<int, Type>
        {
            {(int)BlockType.Cannon, typeof(ModdedCannonBehaviour) }
        };
    }
}
