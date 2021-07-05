using System;
using Microsoft.Msagl.Core;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Miscellaneous;

namespace DrawIoSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var drawingGraph = new Graph();
            drawingGraph.AddNode("TEST 1");
            drawingGraph.AddNode("TEST 2");
            drawingGraph.AddNode("TEST 3");
            drawingGraph.AddNode("TEST 4");
            drawingGraph.AddNode("TEST 5");
            drawingGraph.AddEdge("TEST 1", "TEST 2");
            drawingGraph.AddEdge("TEST 2", "TEST 3");
            drawingGraph.AddEdge("TEST 1", "TEST 4");
            drawingGraph.AddEdge("TEST 3", "TEST 5");

            drawingGraph.CreateGeometryGraph();
            
            foreach (var node in drawingGraph.Nodes)
            {
                node.GeometryNode.BoundaryCurve = 
                    CurveFactory.CreateRectangle(150, 50, new Point(0, 0));
            }

            var routingSettings = new Microsoft.Msagl.Core.Routing.EdgeRoutingSettings {
                UseObstacleRectangles = true,
                BendPenalty = 100,
                EdgeRoutingMode = Microsoft.Msagl.Core.Routing.EdgeRoutingMode.StraightLine
            };
            
            var settings = new SugiyamaLayoutSettings {
                ClusterMargin = 50,
                PackingAspectRatio = 3,
                PackingMethod = Microsoft.Msagl.Core.Layout.PackingMethod.Columns,
                RepetitionCoefficientForOrdering = 0,
                EdgeRoutingSettings = routingSettings,
                NodeSeparation = 50,
                LayerSeparation = 150
            };
            
            LayoutHelpers.CalculateLayout(drawingGraph.GeometryGraph, settings, null);
            
            foreach (var node in drawingGraph.GeometryGraph.Nodes)
            {
                Console.WriteLine($"{node.Width}, {node.Height}, {node.BoundingBox}");
            }
            
            foreach (var edge in drawingGraph.GeometryGraph.Edges)
            {
            }
            
        }
    }
}