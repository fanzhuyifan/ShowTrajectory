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
        private static float g = 32.7435F;

        private LineRenderer LR, LR1, LR2;
        private CanonBlock BB;
        public MToggle PredictCannon;
        private bool PredictCannonEnabled = false;
        public MToggle ShowError;
        private bool ShowErrorEnabled = false;
        private bool isFirstFrame = true;
        private float speed;
        private float totalTime;
        private float stepSize;
        private int numSamples = 256;

        private void Awake()
        {

            BB = (CanonBlock)GetComponent<BlockBehaviour>();

            // Adding toggle box
            PredictCannon = BB.AddToggle("Predict Cannon", "PredictCannon", PredictCannonEnabled);
            PredictCannon.Toggled += (bool value) => { PredictCannonEnabled = value; };
            ShowError = BB.AddToggle("Show Error", "ShowError", ShowErrorEnabled);
            ShowError.Toggled += (bool value) => { ShowErrorEnabled = value; };

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

            /// <summary>
            /// Uses the numerical solver to calculate when the cannon ball will
            /// hit ground
            /// </summary>
            float getTotalTime(float v0)
            {
                Vector3 initialSpeed = transform.TransformVector(
                    BB.boltSpawnRot * Vector3.up) * v0;
                float vy = initialSpeed.y;
                // We want the trajectory to go below ground a bit
                float y0 = boltSpawnPos.y + 5;

                float f(float time)
                {
                    float oneMinusECT = (float)(1 - Math.Exp(-c * time));
                    return y0 + (vy + g / c) * oneMinusECT / c - g * time / c;
                }
                float fPrime(float time)
                {
                    return (float)((vy + g / c) * Math.Exp(-c * time) - g / c);
                }
                float fPrime2(float time)
                {
                    return (float)(-c * (vy + g / c) * Math.Exp(-c * time));
                }
                // calculate time to reach ground assuming there is no friction
                float guess = (float)(vy + Math.Sqrt(vy * vy + 2 * y0 * g)) / g;
                return MathUtils.Newton(f, guess, fprime: fPrime, fprime2: fPrime2);
            }
            /// We want to ensure all three trajectories hit ground
            totalTime = Math.Max(getTotalTime(speed - 0.15F),
                getTotalTime(speed + 0.15F)) + 0.01F;
            // We are dividing by numSamples - 1 since we want the last data point
            // to be below ground
            stepSize = totalTime / (numSamples - 1);

            /// <summary>
            /// Uses lineRenderer to draw the trajectory given initial speed v0.
            /// </summary>
            void drawTrajectory(float v0, LineRenderer lineRenderer)
            {
                Vector3 initialSpeed = transform.TransformVector(
                    BB.boltSpawnRot * Vector3.up) * v0;
                float vy = initialSpeed.y;

                float t;
                int i;
                for (i = 0, t = 0; i < numSamples; ++i, t += stepSize)
                {
                    float oneMinusECT = (float)(1 - Math.Exp(-c * t));
                    lineRenderer.SetPosition(i, boltSpawnPos +
                        new Vector3(initialSpeed.x * oneMinusECT / c,
                        (vy + g / c) * oneMinusECT / c - g * t / c,
                        initialSpeed.z * oneMinusECT / c));
                }
            }

            drawTrajectory(speed, LR);
            if (ShowErrorEnabled)
            {
                drawTrajectory(speed - 1.5F, LR1);
                drawTrajectory(speed + 1.5F, LR2);
            }
        }

        /// <summary>
        /// Creates LineRenderers when simulation starts
        /// </summary> 
        public void OnSimulateStart_PredictEnabled()
        {
            // a wiki says the speed = strength * 60 - (random number from 0 - 3)
            speed = BB.StrengthSlider.Value * 60 - 1.5F;
#if DEBUG
            ConsoleController.ShowMessage("cannon strength is " + BB.StrengthSlider.Value);
#endif
            void initializeLR(string name, out LineRenderer lineRenderer)
            {
                GameObject go = new GameObject("name");
                go.transform.SetParent(BB.transform);
                lineRenderer = go.GetComponent<LineRenderer>() ?? go.AddComponent<LineRenderer>();
                lineRenderer.useWorldSpace = true;
                lineRenderer.SetVertexCount(numSamples);
                lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
                lineRenderer.SetColors(Color.red, Color.yellow);
                lineRenderer.SetWidth(0.5f, 0.5f);
                lineRenderer.enabled = true;
            }

            initializeLR("Cannon Trajectory", out LR);

            if (ShowErrorEnabled)
            {
                initializeLR("Cannon Trajectory Near", out LR1);
                initializeLR("Cannon Trajectory Far", out LR2);
            }
        }
    }
}