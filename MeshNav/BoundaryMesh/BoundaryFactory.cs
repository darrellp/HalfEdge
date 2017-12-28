namespace MeshNav.BoundaryMesh
{
    internal class BoundaryFactory : Factory
    {
        public BoundaryFactory(int dimension) : base(dimension) { }

        public override Face CreateFace()
        {
            return new BoundaryFace();
        }
    }

}
