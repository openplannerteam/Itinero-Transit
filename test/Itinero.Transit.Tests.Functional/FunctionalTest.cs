using System;
using System.Collections;
using System.Collections.Generic;
using Itinero.Transit.Tests.Functional.Performance;

// ReSharper disable UnusedMember.Global
namespace Itinero.Transit.Tests.Functional
{
    
    /// <summary>
    /// Abstract definition of a functional test.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <typeparam name="TIn"></typeparam>
    public abstract class FunctionalTest<TOut, TIn>
    {
        /// <summary>
        /// Gets the name of this test.
        /// </summary>
        protected virtual string Name => GetType().Name;

        /// <summary>
        /// Gets or sets the track performance track.
        /// </summary>
        public bool TrackPerformance { get; set; } = true;

        /// <summary>
        /// Gets or sets the logging flag.
        /// </summary>
        public bool Log { get; set; } = true;

        /// <summary>
        /// Executes this test.
        /// </summary>
        /// <returns>The output.</returns>
        public virtual TOut Run()
        {
            return Run(default(TIn));
        }

        /// <summary>
        /// Executes this test for the given input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The output.</returns>
        public virtual TOut Run(TIn input)
        {
            try
            {

                return TrackPerformance ? RunPerformance(input) : Execute(input);
            }
            catch (Exception)
            {
                Serilog.Log.Error($"Running {Name} with inputs {input} failed");

                throw;
            }
        }

        /// <summary>
        /// Executes this test for the given input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="count">The # of times to repeat the test.</param>
        /// <returns>The output.</returns>
        public virtual TOut RunPerformance(TIn input, int count = 1)
        {
            Func<TIn, PerformanceTestResult<TOut>>
                executeFunc = (i) => new PerformanceTestResult<TOut>(Execute(i));
            return executeFunc.TestPerf(Name, input, count);
        }

        /// <summary>
        /// Executes this test for the given input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The output.</returns>
        protected abstract TOut Execute(TIn input);

        /// <summary>
        /// Asserts that the given value is true.
        /// </summary>
        /// <param name="value">The value to verify.</param>
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
        protected void True(bool value)
        {
            if (!value)
            {
                throw new Exception("Assertion failed, expected true");
            }
        }

        public void NotNull(object o)
        {
            if (o == null)
            {
                throw new ArgumentNullException(nameof(o));
            }
        }
        
        public void NotNull(object o, string message)
        {
            if (o == null)
            {
                throw new ArgumentException("Null detected: "+message);
            }
        }


        public void AssertContains( object o, IEnumerable xs)
        {
            foreach (var x in xs)
            {
                if (x.Equals(o))
                {
                    return;
                }
            }
            
            throw new Exception($"Element {o} was not found");
        }
        
        /// <summary>
        /// Write a log event with the Informational level.
        /// </summary>
        /// <param name="message">The log message.</param>
        protected void Information(string message)
        {
            if (!Log) return;
            Serilog.Log.Information(message);
        }
    }

    public static class Helpers
    {
        public static List<T> Lst<T>(this T t)
        {
            return new List<T> {t};
        }
    }
}