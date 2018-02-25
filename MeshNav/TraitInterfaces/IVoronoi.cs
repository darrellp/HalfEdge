namespace MeshNav.TraitInterfaces
{
	// Implemented by voronoi faces to keep track of their seed point
	interface IVoronoi
	{
		Vector VoronoiPoint { get; set; }
	}
}
