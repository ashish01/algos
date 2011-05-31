using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace tp
{
    public class Edge
    {
        public int Y
        {
            get;
            set;
        }

        public double Weight
        {
            get;
            set;
        }
    }

    public class Graph
    {
        private Dictionary<int, List<Edge>> adjList;

        public Graph()
        {
            adjList = new Dictionary<int, List<Edge>>();
        }

        public int E { get; private set; }
        
        public int V
        {
            get
            {
                return adjList.Keys.Count;
            }
        }
        
        public bool IsDirected { get; private set; }
        
        public void InsertEdge(int x, int y, bool IsDirected)
        {
            List<Edge> edges = adjList[x];
            edges.Add(new Edge()
            {
                Y = y
            });

            E++;

            if (!IsDirected)
                InsertEdge(y, x, true);
        }

        public void PrintDot(TextWriter writer)
        {
            writer.WriteLine("{0} {1} {{", IsDirected ? "digraph" : "graph", this.GetHashCode());
            foreach (var entry in adjList)
            {
                writer.WriteLine("{0};", entry.Key);
                foreach (Edge edge in entry.Value)
                {
                    writer.WriteLine("{0} {1} {2};", entry.Key, IsDirected ? "->" : "--", edge.Y);
                }
            }
            writer.WriteLine("}");
        }

        public void CreateVertex(int x)
        {
            if (!adjList.ContainsKey(x))
            {
                adjList[x] = new List<Edge>();
            }
        }

        public List<Edge> this[int i]
        {
            get
            {
                return adjList[i];
            }
        }

        public static Graph RandGraph(int V, int E, int seed = -1)
        {
            if (seed == -1)
                seed = DateTime.Now.Millisecond;
            Graph G = new Graph();
            G.IsDirected = false;

            Random random = new Random(seed);
            double p = 2 * E / V / (V - 1.0);
            for (int i = 0; i < V; i++)
            {
                G.CreateVertex(i);
                for (int j = 0; j < i; j++)
                    if (random.NextDouble() < p)
                        G.InsertEdge(i, j, G.IsDirected);
            }

            return G;
        }
    
        public static Graph Read(TextReader reader)
        {
            Graph G = new Graph();
            string line = reader.ReadLine();

            for (int i = 0; i < int.Parse(line); i++)
                G.CreateVertex(i);

            
            while ((line = reader.ReadLine()) != null)
            {
                string[] tokens = line.Split(',');
                G.InsertEdge(int.Parse(tokens[0]) - 1, int.Parse(tokens[1]) - 1, false);
            }

            return G;
        }
    }

    public abstract class GraphTraversal
    {
        public enum State
        {
            Undiscovered,
            Discovered,
            Processed
        }

        public delegate void ProcessEdge(Graph G, int SourceVertex, Edge E);
        public delegate void PreProcessvertex(Graph G, int Vertex);

        public State[] VertexState { get; private set; }
        public int[] Parent { get; private set; }

        protected Graph G;

        public GraphTraversal(Graph G)
        {
            this.G = G;
            VertexState = new State[G.V];
            Parent = new int[G.V];

            for (int i = 0; i < G.V; i++)
                VertexState[i] = State.Undiscovered;
        }

        public abstract void Traverse(int Source, ProcessEdge callback, PreProcessvertex preProcessVertex);
    }

    public class BFS : GraphTraversal
    {

        public BFS(Graph G) : base(G)
        {
        }

        public override void Traverse(int Source, ProcessEdge callback, PreProcessvertex preProcessVertex)
        {
            if (VertexState[Source] == State.Undiscovered)
            {
                Queue<int> queue = new Queue<int>();

                queue.Enqueue(Source);
                VertexState[Source] = State.Discovered;
                Parent[Source] = -1;

                while (queue.Count > 0)
                {
                    int vertex = queue.Dequeue();
                    preProcessVertex(G, vertex);
                    foreach (Edge edge in G[vertex])
                    {
                        if (VertexState[edge.Y] != State.Processed || G.IsDirected)
                            callback(G, vertex, edge);

                        if (VertexState[edge.Y] == State.Undiscovered)
                        {
                            queue.Enqueue(edge.Y);
                            VertexState[edge.Y] = State.Discovered;
                            Parent[edge.Y] = vertex;
                        }
                    }
                    VertexState[vertex] = State.Processed;
                }
            }
        }

        public void FindPath(int Source, int End, TextWriter writer)
        {
            if (Source == End || End == -1) // seems End == -1 is a redundant condition
                writer.WriteLine(Source);
            else
            {
                FindPath(Source, Parent[End], writer);
                writer.WriteLine(End);
            }
        }
    }

    public class DFS : GraphTraversal
    {
        public DFS(Graph G) : base(G)
        {
            EntryTime = new int[G.V];
            ExitTime = new int[G.V];
            time = 0;
        }

        private int time;
        public int[] EntryTime
        {
            get;
            private set;
        }

        public int[] ExitTime
        {
            get;
            private set;
        }

        public override void Traverse(int Source, ProcessEdge callback, PreProcessvertex preProcessVertex)
        {
            if (VertexState[Source] == State.Undiscovered)
            {
                VertexState[Source] = State.Discovered;
                time++;
                EntryTime[Source] = time;
                preProcessVertex(G, Source);

                foreach (Edge edge in G[Source])
                {
                    if (VertexState[edge.Y] == State.Undiscovered)
                    {
                        Parent[edge.Y] = Source;
                        callback(G, Source, edge);
                        Traverse(edge.Y, callback, preProcessVertex);
                    }
                    else if (VertexState[edge.Y] != State.Processed || G.IsDirected)
                        if (Parent[Source] != edge.Y)
                            callback(G, Source, edge);
                }
                VertexState[Source] = State.Processed;
                time++;
                ExitTime[Source] = time;
            }
        }
    }

    public class ConnectedComponents
    {
        private Graph G;

        public ConnectedComponents(Graph G)
        {
            this.G = G;
        }

        public void Print(TextWriter writer)
        {
            BFS bfs = new BFS(G);

            for (int i = 0; i < G.V; i++)
            {
                List<int> connected = new List<int>();
                if (bfs.VertexState[i] == BFS.State.Undiscovered)
                {
                    bfs.Traverse(i, 
                        (g, v, e) => { },
                        (g, v) => { connected.Add(v); }
                        );
                }
                writer.WriteLine(String.Join(" ", connected));
            }
        }
    }

    public class PrintTraversal
    {
        public static void PrintDot (Graph graph, TextWriter writer)
        {
            GraphTraversal traversal = new BFS(graph);
            writer.WriteLine("{0} {1} {{", graph.IsDirected ? "digraph" : "graph", graph.GetHashCode());
            for (int i = 0; i < graph.V; i++)
            {
                traversal.Traverse(i, 
                    (G, V, E) => 
                    {
                        writer.WriteLine("{0} {1} {2};", V, G.IsDirected ? "->" : "--", E.Y);
                    },
                    (G, V) => 
                    {
                        writer.WriteLine("{0};", V);
                    });
            }
            writer.WriteLine("}");
        }
    }
}
