using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ShowTrajectory;

namespace testShowTrajectory
{
    [TestClass]
    public class TestSolver
    {
        [TestMethod]
        public void TestMethod1()
        {
            float g = 32.80F;
            float c = 2.0F;
            float vy = 100;
            float y0 = 10;
            float f(float t)
            {
                float oneMinusECT = (float)(1 - Math.Exp(-c * t));
                return y0 + (vy + g / c) * oneMinusECT / c - g * t / c;
            }
            float fPrime(float t)
            {
                return (float)((vy + g / c) * Math.Exp(-c * t) - g / c);
            }
            float fPrime2(float t)
            {
                return (float)(-c * (vy + g / c) * Math.Exp(-c * t));
            }
            // calculate time to reach ground assuming there is no friction
            float guess = (float)(vy + Math.Sqrt(vy * vy + 2 * y0 * g)) / g;
            float totalTime0 = MathUtils.Newton(f, guess);
            Assert.IsTrue(Math.Abs(f(totalTime0)) < 0.000001);
            Assert.IsTrue(totalTime0 > 0);
            float totalTime1 = MathUtils.Newton(f, guess, fprime: fPrime);
            Assert.IsTrue(Math.Abs(totalTime0 - totalTime1) < 0.000001);
            float totalTime2 = MathUtils.Newton(f, guess, fprime: fPrime, fprime2: fPrime2);
            Assert.IsTrue(Math.Abs(totalTime2 - totalTime0) < 0.000001);
        }
    }
}
