using Microsoft.Glee.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FormUI
{
    public partial class MainForm : MetroFramework.Forms.MetroForm
    {
        Microsoft.Glee.Drawing.Graph graph;
        static int[,] weight;
        List<Edge> edgeList;
        List<Edge> minEdges;
        Graph g;

        public MainForm()
        {
            InitializeComponent();
        }

        public struct Edge
        {
            public int u, v, w;
        }

        public void addEdge(int u, int v, int w)
        {
            Edge edge = new Edge();
            edge.u = u; edge.v = v; edge.w = w;
            edgeList.Add(edge);
        }

        public void delEdge(int u, int v)
        {
            for (int i = 0; i < edgeList.Count; i++)
            {
                if (edgeList[i].u == u && edgeList[i].v == v)
                    edgeList.Remove(edgeList[i]);
            }
        }

        private void readfileBtn_Click(object sender, EventArgs e)
        {
            minEdges = new List<Edge>();
            ofd.FileName = "";
            ofd.Filter = "Text document (*.txt) | *.txt";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                graphRb.Checked = true;
                graph = new Microsoft.Glee.Drawing.Graph("graph");
                fileTb.Clear();
                resTb.Clear();
                string fileArr;
                fileLbl.Text = ofd.SafeFileName;
                fileArr = File.ReadAllText(ofd.FileName);
                int size = fileArr.Split('\n').Length;
                weight = new int[size, size];
                int i = 0, j = 0;
                foreach (var row in fileArr.Split('\n'))
                {
                    j = 0;
                    foreach (var col in row.Trim().Split(' '))
                    {
                        weight[i, j] = int.Parse(col.Trim());
                        j++;
                    }
                    i++;
                }
                g = new Graph(size, weight);
                i = 0; j = 0;

                foreach (var row in fileArr.Split('\n'))
                {
                    string strrow = row.Trim();
                    fileTb.AppendText(strrow + Environment.NewLine);
                }

                int n = weight.GetLength(0);

                edgeList = new List<Edge>();
                for (i = 0; i < n; i++)
                {
                    for (j = i + 1; j < n; j++)
                    {
                        if (weight[i, j] != 0)
                        {
                            addEdge(i, j, weight[i, j]);
                            g.AddEdge(i, j);
                        }

                    }
                }

                resTb.AppendText("Node and weight: (u, v) : w" + Environment.NewLine);

                foreach (Edge edge in edgeList)
                {
                    graph.AddEdge(edge.u.ToString(), weight[edge.u, edge.v].ToString(), edge.v.ToString());
                    string str = "(" + edge.u + ", " + edge.v + ") : " + edge.w;
                    resTb.AppendText(str + Environment.NewLine);
                }

                foreach (var edge in graph.Edges)
                {
                    edge.Attr.ArrowHeadAtTarget = ArrowStyle.None;
                    edge.Attr.LineWidth = 2;
                }

                graph.GraphAttr.Orientation = Microsoft.Glee.Drawing.Orientation.Landscape;
                graph.GraphAttr.LayerDirection = LayerDirection.LR;
                graphViewer.OutsideAreaBrush = Brushes.White;
                graphViewer.Graph = graph;

                resTb.AppendText(Environment.NewLine);
            }
        }

        private void createRouteBtn_Click(object sender, EventArgs e)
        {
            g.Test(resTb);
            g.PrintEulerTour(resTb);
            minEdges = g.GetMin();
            graphrbMp.Enabled = true;
        }

        private void graphRb_CheckedChanged(object sender, EventArgs e)
        {
            if (graphRb.Checked)
            {
                foreach (var edge in graph.Edges)
                {
                    edge.Attr.Color = Microsoft.Glee.Drawing.Color.Black;
                    edge.TargetNode.Attr.Color = Microsoft.Glee.Drawing.Color.Black;
                    edge.SourceNode.Attr.Color = Microsoft.Glee.Drawing.Color.Black;
                    edge.Attr.Fontcolor = Microsoft.Glee.Drawing.Color.Black;
                }

                graphViewer.Refresh();
            }
        }

        private void graph_minRb_CheckedChanged(object sender, EventArgs e)
        {
            if (graph_minRb.Checked)
            {
                foreach (var edge in graph.Edges)
                {
                    edge.Attr.Color = Microsoft.Glee.Drawing.Color.Black;
                    edge.TargetNode.Attr.Color = Microsoft.Glee.Drawing.Color.Black;
                    edge.SourceNode.Attr.Color = Microsoft.Glee.Drawing.Color.Black;
                    edge.Attr.Fontcolor = Microsoft.Glee.Drawing.Color.Black;
                }


                if (minEdges.Count > 0)
                {
                    foreach (var edge in graph.Edges)
                    {
                        foreach (var min_e in minEdges)
                        {
                            if ((edge.Source == min_e.u.ToString() && edge.Target == min_e.v.ToString()) 
                                || (edge.Source == min_e.v.ToString() && edge.Target == min_e.u.ToString()))
                            {
                                edge.Attr.Color = Microsoft.Glee.Drawing.Color.Green;
                                edge.Attr.Fontcolor = Microsoft.Glee.Drawing.Color.Green;
                                edge.TargetNode.Attr.Color = Microsoft.Glee.Drawing.Color.Green;
                                edge.SourceNode.Attr.Color = Microsoft.Glee.Drawing.Color.Green;
                            }
                        }
                    }
                }

            }
            graphViewer.Refresh();
        }
    }

    public class Graph
    {
        private int V;
        int[,] weight;
        int count = 0;
        private List<List<int>> adj;
        List<MainForm.Edge> minEdges = new List<MainForm.Edge>();

        public Graph(int v, int[,] w)
        {
            V = v;
            weight = w;
            adj = new List<List<int>>(v);

            for (int i = 0; i < v; i++)
            {
                adj.Add(new List<int>());
            }
        }

        public void AddMinEdge(int u, int v, int w)
        {
            MainForm.Edge edge = new MainForm.Edge();
            edge.u = u; edge.v = v; edge.w = w;
            minEdges.Add(edge);
        }

        public void AddEdge(int v, int w)
        {
            adj[v].Add(w);
            adj[w].Add(v);
        }

        public List<MainForm.Edge> GetMin()
        {
            return minEdges;
        }

        public void RemoveEdge(int u, int v)
        {
            adj[u].Remove(v);
            adj[v].Remove(u);
        }

        int DFSCount(int v, bool[] visited)
        {
            int count = 1;
            visited[v] = true;
            adj[v].ForEach(x =>
            {
                if (visited[x] == false)
                {
                    count += weight[v, x];
                    count += DFSCount(x, visited);
                }
            });
            return count;
        }

        void DFSUtil(int v, bool[] visited)
        {
            visited[v] = true;
            adj[v].ForEach(x =>
            {
                if (!visited[x])
                {
                    DFSUtil(x, visited);
                }
            });
        }

        bool IsConnected()
        {
            bool[] visited = new bool[V];
            int i;
            for (i = 0; i < V; i++)
            {
                visited[i] = false;
            }

            for (i = 0; i < V; i++)
            {
                if (adj[i].Count != 0)
                {
                    break;
                }
            }
            if (i == V)
            {
                return true;
            }
            DFSUtil(i, visited);

            for (i = 0; i < V; i++)
            {
                if (visited[i] == false && adj[i].Count > 0)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsValidNextEdge(int u, int v)
        {
            if (adj[u].Count == 1)
            {
                return true;
            }
            else
            {
                bool[] visited = new bool[V];
                for (int i = 0; i < visited.Length; i++)
                {
                    visited[i] = false;
                }

                var count1 = DFSCount(u, visited);

                RemoveEdge(u, v);
                bool[] visted2 = new bool[V];
                for (int i = 0; i < visted2.Length; i++)
                {
                    visted2[i] = false;
                }
                var count2 = DFSCount(u, visted2);
                AddEdge(u, v);
                return (count1 > count2) ? false : true;
            }
        }

        public void PrintEulerUtil(int u, RichTextBox Tb)
        {
            try
            {
                foreach (var x in adj[u])
                {
                    if (IsValidNextEdge(u, x))
                    {
                        Tb.AppendText(u + " - " + x + " : " + weight[u, x] + Environment.NewLine);
                        AddMinEdge(u, x, weight[u, x]);
                        count += weight[u, x];
                        RemoveEdge(u, x);
                        PrintEulerUtil(x, Tb);
                    }
                }
                Tb.AppendText("Price: " + count + Environment.NewLine);
            }
            catch (Exception) { }
        }

        public void PrintEulerTour(RichTextBox Tb)
        {
            int u = 0;
            for (int i = 0; i < V; i++)
            {
                if (adj[i].Count % 2 != 0)
                {
                    u = i;
                    break;
                }
            }
            Tb.AppendText(Environment.NewLine);
            this.PrintEulerUtil(u, Tb);
        }

        int isEulerian()
        {
            if (IsConnected() == false)
            {
                return 0;
            }

            int odd = 0;
            for (int i = 0; i < V; i++)
            {
                if (adj[i].Count % 2 != 0)
                {
                    odd++;
                }
            }

            if (odd > 2)
            {
                return 0;
            }
            return (odd == 2) ? 1 : 2;
        }

        public void Test(RichTextBox Tb)
        {
            int res = isEulerian();
            if (res == 0)
            {
                Tb.AppendText("The graph is not an Eulerian.");
            }
            else if (res == 1)
            {
                Tb.AppendText("The graph has an Eulerian path.");
            }
            else
            {
                Tb.AppendText("The graph has an Eulerian cycle.");
            }
        }
    }

}
