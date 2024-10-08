﻿/* ***************************************************************************
 * This file is part of SharpNEAT - Evolution of Neural Networks.
 * 
 * Copyright 2004-2016 Colin Green (sharpneat@gmail.com)
 *
 * SharpNEAT is free software; you can redistribute it and/or modify
 * it under the terms of The MIT License (MIT).
 *
 * You should have received a copy of the MIT License
 * along with SharpNEAT; if not, see https://opensource.org/licenses/MIT.
 */
using System;
using Redzen.Random;
using Redzen.Random.Double;
using System.Runtime.CompilerServices;

namespace SharpNeat.Network
{
    public class TanH : IActivationFunction
    {
        /// <summary>
        /// Default instance provided as a public static field.
        /// </summary>
        public static readonly IActivationFunction __DefaultInstance = new TanH();

        public string FunctionId => this.GetType().Name;

        public string FunctionString => "";

        public string FunctionDescription => this.GetType().Name;

        public bool AcceptsAuxArgs => false;

        public double Calculate(double x, double[] auxArgs)
        {
      var value = Math.Tanh(x);
      return value;
        }

        public float Calculate(float x, float[] auxArgs)
        {
            return (float)Math.Tanh(x);
        }

        public double[] GetRandomAuxArgs(IRandomSource rng, double connectionWeightRange)
        {
            throw new SharpNeatException("GetRandomAuxArgs() called on activation function that does not use auxiliary arguments.");
        }

        public void MutateAuxArgs(double[] auxArgs, IRandomSource rng, ZigguratGaussianDistribution gaussianSampler, double connectionWeightRange)
        {
            throw new SharpNeatException("MutateAuxArgs() called on activation function that does not use auxiliary arguments.");
        }

        #region Private Static Methods

        /// <summary>
        /// Hyperbolic Area Sine
        /// </summary>
        /// <param name="value">The real value.</param>
        /// <returns>The hyperbolic angle, i.e. the area of its hyperbolic sector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Asinh(double value)
        {
            return Math.Log(value + Math.Sqrt((value * value) + 1), Math.E);
        }

        #endregion
    }
}
