namespace MeshNav.TraitInterfaces
{
    // Interface implemented by halfedges
    public interface IRayed
    {
        // A ray extending out from a border vertex
        bool IsRayed { get; }

        // An edge connecting two rayed edges
        bool IsInfinite { get; }
    }

    // Interface implemented by faces
    public interface IAtInfinity
    {
        // This is the face at infinity
        bool IsFaceAtInfinity { get; }
    }
}
