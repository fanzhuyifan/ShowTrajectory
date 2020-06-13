using System;
namespace ShowTrajectory
{
    public class MathUtils
    {

        public static bool WithinTol(float x, float y, float atol, float rtol)
        {
            return Math.Abs(x - y) <= atol + rtol * Math.Abs(y);
        }

        // Find a zero of a real or complex function using the Newton-Raphson
        // (or secant or Halley’s) method.
        //
        // MathUtils.Newton(func, x0, fprime=null, tol=1.48e-08, maxiter=50, fprime2=null,
        // x1, rtol=0.0, full_output=false)
        //
        // Mimicks scipy's implementation of scipy.optimize.newton
        // 
        // Parameters
        // func: function
        //      The function whose zero is wanted. It must be a function of a
        //      single variable
        // x0: float
        //      An initial estimate of the zero that should be somewhere near
        //     the actual zero.
        // fprime : function, optional
        //      The derivative of the function when available and convenient. If it
        //      is null (default), then the secant method is used.
        // tol : float, optional
        //      The allowable error of the zero value.
        // maxiter : int, optional
        //      Maximum number of iterations.
        // fprime2 : function, optional
        //      The second order derivative of the function when available and
        //      convenient. If it is null (default), then the normal Newton-Raphson
        //      or the secant method is used. If it is not null, then Halley's method
        //      is used.
        // x1 : float, optional
        //      Another estimate of the zero that should be somewhere near the
        //      actual zero. Used if `fprime` is not provided.
        // rtol : float, optional
        //      Tolerance (relative) for termination.
        // full_output : bool, optional
        //      If `full_output` is false (default), the root is returned.
        //      If true, the dictionary {{"root": root}, {"converged": true/false},
        //      {"iter": numIter}} is returned.
        public static float Newton(Func<float, float> func, float x0,
            Func<float, float> fprime = null, float tol = 1.48e-08F,
            int maxiter = 50, Func<float, float> fprime2 = null, float x1 = 0,
            bool x1Provided = false, float rtol = 0)
        {
            if (tol <= 0)
                throw new Exception("tol too small (" + tol + " <= 0)");
            if (maxiter < 1)
                throw new Exception("maxiter must be greater than 0");
            float p0 = x0;
            long itr = 0;
            float p = 0;
            if (fprime != null)
            {
                // Newton - Raphson method
                for (; itr < maxiter; ++itr)
                {
                    // first evaluate fval
                    float fval = func(p0);
                    // if fval is 0, a root has been found, then terminate
                    if (fval == 0)
                        return p0;
                    float fder = fprime(p0);
                    // stop iterating if the derivative is zero
                    if (fder == 0)
                        return p0;

                    // Newton step
                    float newton_step = fval / fder;
                    if (fprime2 != null)
                    {
                        float fder2 = fprime2(p0);
                        // Halley's method:
                        // newton_step /= (1.0 - 0.5 * newton_step * fder2 / fder)
                        // Only do it if denominator stays close enough to 1
                        // Rationale:  If 1-adj < 0, then Halley sends x in the
                        // opposite direction to Newton.  Doesn't happen if x is close
                        // enough to root.
                        float adj = newton_step * fder2 / fder / 2;
                        if (Math.Abs(adj) < 1)
                            newton_step /= 1.0F - adj;
                    }
                    p = p0 - newton_step;
                    if (WithinTol(p, p0, atol: tol, rtol: rtol))
                        return p;
                    p0 = p;
                }
            }
            else
            {
                // secant method
                float p1, q0, q1;
                if (x1Provided)
                {
                    if (x1 == x0)
                        throw new Exception("x1 and x0 must be different");
                    p1 = x1;
                }
                else
                {
                    float eps = 1e-4F;
                    p1 = x0 * (1 + eps);
                    p1 += (p1 >= 0 ? eps : -eps);
                }
                q0 = func(p0);
                q1 = func(p1);
                if (Math.Abs(q1) < Math.Abs(q0))
                {
                    float temp = q1;
                    q1 = q0;
                    q0 = temp;

                    temp = p0;
                    p0 = p1;
                    p1 = temp;
                }
                for (; itr < maxiter; ++itr)
                {
                    if (q0 == q1)
                    {
                        p = (p1 + p0) / 2.0F;
                        if (p1 != p0)
                            return p;
                        else
                            return p;

                    }
                    else
                    {
                        // Secant Step
                        if (Math.Abs(q1) > Math.Abs(q0))
                            p = (-q0 / q1 * p1 + p0) / (1.0F - q0 / q1);
                        else
                            p = (-q1 / q0 * p0 + p1) / (1.0F - q1 / q0);
                    }
                    if (WithinTol(p, p1, atol: tol, rtol: rtol))
                        return p;

                    p0 = p1;
                    q0 = q1;
                    p1 = p;
                    q1 = func(p1);
                }
            }
            return p;
        }
    }
}
