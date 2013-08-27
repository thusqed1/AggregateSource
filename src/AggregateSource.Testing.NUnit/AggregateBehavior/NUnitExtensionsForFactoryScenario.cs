﻿using System;
using System.IO;
using System.Linq;

namespace AggregateSource.Testing.AggregateBehavior
{
    public static class NUnitExtensionsForFactoryScenario
    {
        public static void Assert(this IEventCentricAggregateFactoryTestSpecificationBuilder builder,
            IEventComparer comparer)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            if (comparer == null) throw new ArgumentNullException("comparer");
            var specification = builder.Build();
            var runner = new EventCentricAggregateFactoryTestRunner(comparer);
            var result = runner.Run(specification);
            if (result.Failed)
            {
                if (result.ButException.HasValue)
                {
                    using (var writer = new StringWriter())
                    {
                        writer.WriteLine("  Expected: {0} events,", result.Specification.Thens.Length);
                        writer.WriteLine("  But was:  {0}", result.ButException.Value);

                        throw new NUnit.Framework.AssertionException(writer.ToString());
                    }
                }
                if (result.ButEvents.HasValue)
                {
                    if (result.ButEvents.Value.Length != result.Specification.Thens.Length)
                    {
                        using (var writer = new StringWriter())
                        {
                            writer.WriteLine("  Expected: {0} events ({1}),",
                                result.Specification.Thens.Length,
                                String.Join(",", result.Specification.Thens.Select(_ => _.GetType().Name).ToArray()));
                            writer.WriteLine("  But was:  {0} events ({1})",
                                result.ButEvents.Value.Length,
                                String.Join(",", result.ButEvents.Value.Select(_ => _.GetType().Name).ToArray()));

                            throw new NUnit.Framework.AssertionException(writer.ToString());
                        }
                    }
                    using (var writer = new StringWriter())
                    {
                        writer.WriteLine("  Expected: {0} events ({1}),",
                                result.Specification.Thens.Length,
                                String.Join(",", result.Specification.Thens.Select(_ => _.GetType().Name).ToArray()));
                        writer.WriteLine("  But found the following differences:");
                        foreach (var difference in
                            result.Specification.Thens.
                            Zip(result.ButEvents.Value,
                                (expected, actual) => new Tuple<object, object>(expected, actual)).
                            SelectMany(_ => comparer.Compare(_.Item1, _.Item2)))
                        {
                            writer.WriteLine("    {0}", difference.Message);
                        }

                        throw new NUnit.Framework.AssertionException(writer.ToString());
                    }
                }
            }
        }

        public static void Assert(this IExceptionCentricAggregateFactoryTestSpecificationBuilder builder,
            IExceptionComparer comparer)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            if (comparer == null) throw new ArgumentNullException("comparer");
            var specification = builder.Build();
            var runner = new ExceptionCentricAggregateFactoryTestRunner(comparer);
            var result = runner.Run(specification);
            if (result.Failed)
            {
                if (result.ButException.HasValue)
                {
                    using (var writer = new StringWriter())
                    {
                        writer.WriteLine("  Expected: {0},", result.Specification.Throws);
                        writer.WriteLine("  But was:  {0}", result.ButException.Value);

                        throw new NUnit.Framework.AssertionException(writer.ToString());
                    }
                }
                if (result.ButEvents.HasValue)
                {
                    using (var writer = new StringWriter())
                    {
                        writer.WriteLine("  Expected: {0},", result.Specification.Throws);
                        writer.WriteLine("  But was:  {0} events ({1})",
                            result.ButEvents.Value.Length,
                            String.Join(",", result.ButEvents.Value.Select(_ => _.GetType().Name).ToArray()));

                        throw new NUnit.Framework.AssertionException(writer.ToString());
                    }
                }
            }
        }
    }


    //TODO: Same thing for command, query (requires IResultComparer), constructor

    
    //class NUnitEventComparerDiscovery
    //{
    //    public static readonly NUnitEventComparerDiscovery Instance = new NUnitEventComparerDiscovery();

    //    public IEqualityComparer<object> Discover(IEqualityComparer<object> comparer)
    //    {
    //        if (comparer != null)
    //            return comparer;
    //        var testContext = NUnit.Framework.TestContext.CurrentContext;
    //        if (testContext != null && testContext.Test != null && testContext.Test.Properties != null)
    //        {
    //            if (testContext.Test.Properties.Contains("TODO"))
    //            {
    //                comparer = testContext.Test.Properties["TODO"] as IEqualityComparer<object>;
    //                if (comparer != null)
    //                    return comparer;
    //            }
    //        }
    //        return new CompareObjectsBasedEventComparer();
    //    }
    //}

    //public class CompareObjectsBasedEventComparer : IEqualityComparer<object>
    //{
    //    readonly CompareObjects _compareObjects;

    //    public CompareObjectsBasedEventComparer()
    //    {
    //        _compareObjects = new CompareObjects();
    //    }

    //    public bool Equals(object x, object y)
    //    {
    //        return _compareObjects.Compare(x, y);
    //    }

    //    public int GetHashCode(object obj)
    //    {
    //        throw new NotSupportedException();
    //    }
    //}
}