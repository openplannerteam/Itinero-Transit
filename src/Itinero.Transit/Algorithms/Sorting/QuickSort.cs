// The MIT License (MIT)

// Copyright (c) 2018 Anyways B.V.B.A.

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Itinero.Transit.Tests")]
[assembly: InternalsVisibleTo("Itinero.Transit.Tests.Benchmarks")]
namespace Itinero.Transit.Algorithms.Sorting
{
    /// <summary>
    /// An implementation of the quicksort algorithm.
    /// </summary>
    internal static class QuickSort
    {
        /// <summary>
        /// Executes a quicksort algorithm given the value and swap methods.
        /// </summary>
        public static void Sort(Func<long, long> value, Action<long, long> swap, long left, long right)
        {
            if (left < right)
            {
                var stack = new System.Collections.Generic.Stack<Pair>();
                stack.Push(new Pair(left, right));
                while (stack.Count > 0)
                {
                    var pair = stack.Pop();
                    var pivot = QuickSort.Partition(value, swap, pair.Left, pair.Right);
                    if (pair.Left < pivot)
                    {
                        stack.Push(new Pair(pair.Left, pivot - 1));
                    }
                    if (pivot < pair.Right)
                    {
                        stack.Push(new Pair(pivot + 1, pair.Right));
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the given range is sorted.
        /// </summary>
        public static bool IsSorted(Func<long, long> value, long left, long right)
        {
            var previous = value(left);
            for (var i = left + 1; i <= right; i++)
            {
                var val = value(i);
                if (previous > val)
                {
                    return false;
                }
                previous = val;
            }
            return true;
        }

        private struct Pair
        {
            public Pair(long left, long right)
                : this()
            {
                this.Left = left;
                this.Right = right;
            }

            public long Left { get; set; }
            public long Right { get; set; }
        }

        /// <summary>
        /// Partion the based on the pivot value.
        /// </summary>
        /// <return>The new left.</return>
        private static long Partition(Func<long, long> value, Action<long, long> swap, long left, long right)
        { // get the pivot value.
            if (left > right) { throw new ArgumentException("left should be smaller than or equal to right."); }
            if (left == right)
            { // sorting just one item results in that item being sorted already and a pivot equal to that item itself.
                return right;
            }

            // select the middle one as the pivot value.
            var pivot = (left + right) / (long)2;
            if (pivot != left)
            { // switch.
                swap(pivot, left);
            }

            // start with the left as pivot value.
            pivot = left;
            var pivotValue = value(pivot);

            while (true)
            {
                // move the left to the right until the first value bigger than pivot.
                var leftValue = value(left + 1);
                while (leftValue <= pivotValue)
                {
                    left++;
                    if (left == right)
                    {
                        break;
                    }
                    leftValue = value(left + 1);
                }

                // move the right to left until the first value smaller than pivot.
                if (left != right)
                {
                    var rightValue = value(right);
                    while (rightValue > pivotValue)
                    {
                        right--;
                        if (left == right)
                        {
                            break;
                        }
                        rightValue = value(right);
                    }
                }

                if (left == right)
                { // we are done searching, left == right.
                    if (pivot != left)
                    { // make sure the pivot value is where it is supposed to be.
                        swap(pivot, left);
                    }
                    return left;
                }

                // swith left<->right.
                swap(left + 1, right);
            }
        }

        /// <summary>
        /// Partitions everything between left and right in three partitions, smaller than, equal to and larger than pivot.
        /// </summary>
        /// <remarks>Reference : https://en.wikipedia.org/wiki/Dutch_national_flag_problem </remarks>
        public static void ThreewayPartition(Func<long, long> value, Action<long, long> swap, long left, long right,
            out long highestLowest, out long lowestHighest)
        {
            QuickSort.ThreewayPartition(value, swap, left, right, left, out highestLowest, out lowestHighest); // default, the left a pivot.
        }

        /// <summary>
        /// Partitions everything between left and right in three partitions, smaller than, equal to and larger than pivot.
        /// </summary>
        /// <remarks>Reference : https://en.wikipedia.org/wiki/Dutch_national_flag_problem </remarks>
        public static void ThreewayPartition(Func<long, long> value, Action<long, long> swap, long left, long right, long pivot,
            out long highestLowest, out long lowestHighest)
        {
            if (left > right) { throw new ArgumentException("left should be smaller than or equal to right."); }
            if (left == right)
            { // sorting just one item results in that item being sorted already and a pivot equal to that item itself.
                highestLowest = right;
                lowestHighest = right;
                return;
            }

            // get pivot value.
            var pivotValue = value(pivot);

            var i = left;
            var j = left;
            var n = right;

            while (j <= n)
            {
                var valueJ = value(j);
                if (valueJ < pivotValue)
                {
                    swap(i, j);
                    i++;
                    j++;
                }
                else if (valueJ > pivotValue)
                {
                    swap(j, n);
                    n--;
                }
                else
                {
                    j++;
                }
            }
            highestLowest = i - 1;
            lowestHighest = n + 1;
        }
    }
}