﻿using System;

namespace MeshNav.TraitInterfaces
{
    public interface IPreviousEdge<T> where T : struct, IEquatable<T>, IFormattable
    {
        HalfEdge<T> PreviousEdgeAccessor { get; set; }
    }
}
