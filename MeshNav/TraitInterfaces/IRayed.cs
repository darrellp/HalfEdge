namespace MeshNav.TraitInterfaces
{
    // Interface implemented by halfedges
    public interface IRayed
    {
        // A ray extending out from a border vertex
        bool IsRayed { get; }
    }

    // Interface implemented by halfedges, faces and vertices at infinity
    public interface IAtInfinity
    {
        // This is the face at infinity
        bool IsAtInfinity { get; }
    }
}
