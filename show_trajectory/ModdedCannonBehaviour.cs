/*
 * Adds a toggle box, draws a trajectory if enabled.
 */
using System;
using UnityEngine;
using Modding;
using System.Text;
using System.Linq;

namespace ShowTrajectory
{
    public class ModdedCannonBehaviour : MonoBehaviour
    {
        // The drag of a cannon ball is 0.2;
        // the acceleration due to gravity is 32.8
        private static float c = 0.2F;
        private static float g = 32.8F;

        private LineRenderer LR;
        private CanonBlock BB;
        public MToggle PredictCannon;
        private bool isFirstFrame = true;
        private bool PredictCannonEnabled = false;
        private float speed;
        private float totalTime = 10;
        private float stepSize = 0.1F;
        private int numSamples;

        private void Awake()
        {

            BB = (CanonBlock)GetComponent<BlockBehaviour>();

            // Adding toggle box
            PredictCannon = BB.AddToggle("Predict Cannon", "PredictCannon", PredictCannonEnabled);
            PredictCannon.Toggled += (bool value) => { PredictCannonEnabled = value; };
        }

        void Update()
        {
            if (BB.SimPhysics)
            {
                if (isFirstFrame)
                {
                    isFirstFrame = false;
                    if (PredictCannonEnabled) { OnSimulateStart_PredictEnabled(); }
                }

                if (!PredictCannonEnabled) return;

                SimulateUpdateAlways_PredictEnable();
            }
            else
            {
                isFirstFrame = true;
            }
        }

        /// <summary>
        /// Draws the trajectory
        /// </summary>
        public void SimulateUpdateAlways_PredictEnable()
        {
            Vector3 boltSpawnPos = transform.TransformPoint(BB.boltSpawnPos);
            Vector3 initialSpeed = transform.TransformVector(
                BB.boltSpawnRot * Vector3.up) * speed;
            float t;
            int i;
            for (i = 0, t = 0; i < numSamples; ++i, t += stepSize)
            {
                float oneMinusECT = (float)(1 - Math.Exp(-c * t));
                LR.SetPosition(i, boltSpawnPos +
                    new Vector3(initialSpeed.x * oneMinusECT / c,
                    (initialSpeed.y + g / c) * oneMinusECT / c - g * t / c,
                    initialSpeed.z * oneMinusECT / c));
            }
        }

        /// <summary>
        /// Creates a LineRenderer when simulation starts
        /// </summary> 
        public void OnSimulateStart_PredictEnabled()
        {
            // a wiki says the speed = strength * 60 - (random number from 0 - 3)
            speed = BB.StrengthSlider.Value * 60 - 1.5F;
#if DEBUG
            ConsoleController.ShowMessage("cannon strength is " + BB.StrengthSlider.Value);

#endif
            GameObject go = new GameObject("Cannon Trajectory");
            go.transform.SetParent(BB.transform);
            LR = go.GetComponent<LineRenderer>() ?? go.AddComponent<LineRenderer>();
            LR.useWorldSpace = true;
            numSamples = (int)(totalTime / stepSize);
            LR.SetVertexCount(numSamples);
            LR.material = new Material(Shader.Find("Particles/Additive"));
            LR.SetColors(Color.red, Color.yellow);
            LR.SetWidth(0.5f, 0.5f);
            LR.enabled = true;
        }

    }
}