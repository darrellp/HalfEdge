﻿namespace MeshNav.TraitInterfaces
{
    public interface IBoundary
    {
        bool IsBoundaryAccessor { get; set; }
        bool IsOuterBoundary { get; }
    }
}
