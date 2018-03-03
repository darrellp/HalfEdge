HalfEdge data structure

Supports any number of dimensions, reads from 3D OBJ files, creates voronoi diagrams with 2D points, does 2D point location via 
Trapezoidal map and can serialize/deserialize the resulting tree, easy creation - just add points and then add the polygons
which reference those points, allows for 3 types of border configuration and very customizable.  A work in progress but does everything
mentioned here.  Still integrating voronoi code which I wrote a zillion years ago using a winged edge data structure though it seems
to be working in at least the basic case right now.
