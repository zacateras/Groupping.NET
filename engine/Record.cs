using System;
using System.Collections.Generic;
using Groupping.NET.Algorithms;

namespace Groupping.NET
{
    public class Record
    {
        public int Index { get; set; }

        public double[] Attributes { get; set; }

        public override string ToString() => $"{Index} [{string.Join(", ", Attributes)}]";
    }
}