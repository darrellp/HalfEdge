namespace MeshNav.TraitInterfaces
{
    // Interface implemented by halfedges and vertices
    public interface IRayed
    {
        // A ray extending out from a border vertex
        bool IsRayed { get; set; }
    }
}
