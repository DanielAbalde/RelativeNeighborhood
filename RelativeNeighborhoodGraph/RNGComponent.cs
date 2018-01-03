using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Data;
using System.Drawing;

namespace RelativeNeighborhoodGraph
{
    public class RNGComponent : GH_Component
    {
        public RNGComponent()
          : base("Relative Neighborhood", "RNG",
              "Computes the relative neighbourhood graph in a set of points. \nA point p is a relative neighbour of a point q if the distance between them is less than the distance from any other point r to p and q.",
              "Mesh", "Triangulation")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Set of points", GH_ParamAccess.list);
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Graph", "G", "Edges of the connectivity diagram", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Indices", "I", "Topological connectivity diagram", GH_ParamAccess.tree);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            try
            {
                List<Point3d> pts = new List<Point3d>();
                DA.GetDataList(0, pts);

                DataTree<Line> links = new DataTree<Line>();
                DataTree<int> indices = new DataTree<int>();

                for (int i = 0; i < pts.Count; i++)
                {
                    Point3d p0 = pts[i];
                    for (int j = i + 1; j < pts.Count; j++)
                    {
                        Point3d p1 = pts[j];
                        double d = p0.DistanceTo(p1);
                        bool linkus = true;
                        for (int k = 0; k < pts.Count; k++)
                        {
                            if (k != i && k != j)
                            {
                                Point3d p2 = pts[k];
                                if (p2.DistanceTo(p0) < d && p2.DistanceTo(p1) < d)
                                {
                                    linkus = false; break;
                                }
                            }
                        }
                        if (linkus)
                        {
                            links.Add(new Line(p0, p1), new GH_Path(i));
                            links.Add(new Line(p1, p0), new GH_Path(j));
                            indices.Add(j, new GH_Path(i));
                            indices.Add(i, new GH_Path(j));
                        }
                    }
                }
                //for(int i = 0;i < pts.Count;i++){
                System.Threading.Tasks.Parallel.For(0, pts.Count, i => {
                    GH_Path path = new GH_Path(i);
                    if (!links.PathExists(path))
                    {//Add nulls
                        links.AddRange(new List<Line>() { }, path);
                        indices.AddRange(new List<int>() { }, path);
                    }
                });

                DA.SetDataTree(0, links);
                DA.SetDataTree(1, indices);
            }
            catch (Exception e)
            {
                this.AddRuntimeMessage( GH_RuntimeMessageLevel.Error, e.ToString());
            }
        }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Icon;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("4371e1b7-2d56-4cf1-8693-7a9d6d21a6e4"); }
        }
    }

    public class RelativeNeighborhoodGraphInfo : GH_AssemblyInfo
    {
        public override string Name => "Relative Neighborhood";
        public override Bitmap Icon => null;
        public override string Description => "Computes the relative neighbourhood graph in a set of points.";
        public override Guid Id => new Guid("da74df27-8461-4145-84e9-64f788c67bbc");
        public override string AuthorName => "Daniel Abalde";
        public override string AuthorContact => "dga_3@hotmail.com";
    }
}
