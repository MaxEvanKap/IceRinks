using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace IceRinks
{
    internal class Program
    {
        //t = m * n + 1, s = m*n
        static void Main(string[] args)
        {
            
            // read input
            int m = int.Parse(Console.ReadLine());
            int n = int.Parse(Console.ReadLine());
            int mode = int.Parse(Console.ReadLine());
            int[,] thick = new int[m, n];
            for (int i = 0; i < m; i++)
            {
                string[] row = Console.ReadLine().Split(' ');
                for (int j = 0; j < n; j++) {
                    thick[i, j] = int.Parse(row[j]);
                }
            }

            // construct graph mn nodes + s and t
            LinkedList<Node>[] graph = new LinkedList<Node>[m * n + 2];
            for (int k = 0; k < graph.Length; k++)
            {
                graph[k] = new LinkedList<Node>();
            }
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    AddNeighbours(i, j, thick, graph);
                }
            }
            /*
            for (int i = 0; i < graph.Length; i++)
            {
                Console.WriteLine($"{i}: ");
                foreach (Node a in graph[i])
                {
                    Console.Write(a);
                }
            }*/

            //create first residual graph as copy of original graph
            LinkedList<Node>[] residual = CreateResidualGraph(graph);
            EdmondsKarp(residual);
            int[] pred = BFS(residual, m * n);
            List<int> reachableS = Reachable(pred);
            /*for (int i = 0; i < graph.Length; i++)
            {
                Console.WriteLine($"{i}: ");
                foreach (Node a in graph[i])
                {
                    Console.Write(a);
                }
            }*/

            int penalty = 0;
            
            foreach (int t in reachableS)
            {
                foreach (Node a in graph[t])
                {
                    if (!reachableS.Contains(a.Pos))
                        penalty += a.Capacity;
                }
            }
            Console.WriteLine(penalty);
            if(mode == 1)
                // do not want to print source so i < count - 1
                for (int i = 0; i < reachableS.Count - 1; i++)
                {
                    Console.WriteLine(reachableS[i]/n + " " + reachableS[i]%n);
                }
       
            
        }

        // adds positions of all nodes with a predesessor
        static List<int> Reachable(int[] pred)
        {
            List<int> res = new List<int>();
            for (int i = 0; i < pred.Length; i++)
            {
                if (pred[i] > 0) res.Add(i);
            }
            res.Add(pred.Length - 2);
            return res;
        }

        //Alters the residual graph until there is no more path from s to t (maximum flow)
        static void EdmondsKarp(LinkedList<Node>[] graph)
        {
            while (true)
            {
                LinkedList<Node> path = ToPath(graph,BFS(graph, graph.Length - 2));
                if (path == null) break;
                AlterResidual(graph, path);
            }
        }

        //create the original residual graph
        static LinkedList<Node>[] CreateResidualGraph(LinkedList<Node>[] graph)
        {
            LinkedList<Node>[] residual = new LinkedList<Node>[graph.Length];
            for (int i = 0; i < graph.Length; i++)
            {
                residual[i] = new LinkedList<Node>();
                foreach (var v in graph[i])
                {
                    residual[i].AddFirst(new Node(v.Pos, v.Capacity, v.Forward));
                }
            }

            return residual;
        }

        static void AlterResidual(LinkedList<Node>[] residual, LinkedList<Node> path)
        {
            int mc = minCapacity(path);
            LinkedListNode<Node> current = path.First;
            int pred = residual.Length - 2;
            while (current != null)
            {
                if (current.Value.Capacity - mc == 0) residual[pred].Remove(current.Value);
                else current.Value.Capacity -= mc;
                LinkedListNode<Node> a = residual[current.Value.Pos].Find(new Node(pred, -1, !current.Value.Forward));
                if (a == null) residual[current.Value.Pos].AddFirst(new Node(pred, mc, !current.Value.Forward));
                else a.Value.Capacity += mc;
                pred = current.Value.Pos;
                current = current.Next;
            }

        }

        static void AddNeighbours(int i, int j, int[,] thick, LinkedList<Node>[] graph)
        {
            (int, int)[] directions = { (-1,-1), (-1,0), (-1,1)
                                            , (0,-1), (0,1),
                                            (1,-1), (1,0), (1,1)};
            int m = thick.GetLength(0);
            int n = thick.GetLength(1);
            
            //add neighbours to adjacency list
            foreach (var (di, dj) in directions)
            {
                int ni = i + di;
                int nj = j + dj;
                if (ni >= 0 && ni < m && nj >= 0 && nj < n)
                {
                    Node nghb = new Node(ni * n + nj, 32 - Math.Abs(thick[i, j] - thick[ni,nj]), true);
                    graph[i * n + j].AddFirst(nghb);
                }
            }
            Node toT = new Node(m*n + 1, 32 - thick[i, j], true); // add sink to adjacency list
            graph[i * n + j].AddFirst(toT);
            
            Node fromS = new Node(i * n + j, thick[i, j], true); // add this node to the source's adjacency list
            graph[m * n].AddFirst(fromS);
        }


        // returns linkedlist representing the found shortest path from s to t, returns null if no path exists
        static int[] BFS(LinkedList<Node>[] graph, int node)
        {
            
            int[] pred = new int[graph.Length];

            pred[node] = -1;
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(node);
            while (queue.Count > 0)
            {
                int j = queue.Dequeue();
                LinkedListNode<Node> next = graph[j].First;
                while (next != null)
                {
                    if (pred[next.Value.Pos] == 0)
                    {
                        pred[next.Value.Pos] = j + 1; // predecessor + 1 because of int not nullable
                        queue.Enqueue(next.Value.Pos);
                    }
                    next = next.Next;
                }
            }
            return pred;
        }

        static LinkedList<Node> ToPath(LinkedList<Node>[] graph, int[] pred)
        {
            int i = pred.Length - 1;
            LinkedList<Node> Path = new();
            if (pred[i] == 0) return null;
            while (pred[i] != -1)
            {
                Path.AddFirst(graph[pred[i] - 1].Find(new Node(i, 0, true)).Value);
                i = pred[i] - 1;
            }
            return Path;
        }
        static int minCapacity(LinkedList<Node> path)
        {
            int t = 32;
            foreach(Node a in path) 
                if(t > a.Capacity) t = a.Capacity;
            return t;
        }
    }



    // A linked list node representing a neighbour in the flow network, the capacity and flow on the edge to that neighbour
    // and the next neigbour in the linked list
    public class Node
    {
        public int Pos;
        public int Flow;
        public int Capacity;
        public bool Forward;

        public Node(int Pos, int Capacity, bool Forward)
        {
            this.Pos = Pos;
            this.Flow = 0;
            this.Capacity = Capacity;
            this.Forward = Forward;
        }

        public override bool Equals(object obj)
        {
            var item = obj as Node;
            if (item.Capacity == -1) return item.Pos == this.Pos && item.Forward == this.Forward;
            return item.Pos == this.Pos;

        }

        public override string ToString()
        {
            return $"({Pos}, {Capacity})";
        }
    }

    
}
