namespace MeshNav.TraitInterfaces
{
    // Interface implemented by halfedges
    public interface IRayed
    {
        // A ray extending out from a border vertex
        bool IsRayed { get; }
    }
}
