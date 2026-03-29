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
            //create first residual graph as copy of original graph
            LinkedList<Node>[] residual = CreateResidualGraph(graph);



            LinkedList<int> pred = BFS(graph, m * n);
            foreach (int t in pred) Console.WriteLine(t);

            
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
                    residual[i].AddFirst(v);
                }
            }

            return residual;
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
                    Node nghb = new Node(ni * n + nj, 32 - Math.Abs(thick[i, j] - thick[ni,nj]));
                    graph[i * n + j].AddFirst(nghb);
                }
            }
            Node toT = new Node(m*n + 1, 32 - thick[i, j]); // add sink to adjacency list
            graph[i * n + j].AddFirst(toT);
            
            Node fromS = new Node(i * n + j, thick[i, j]); // add this node to the source's adjacency list
            graph[m * n].AddFirst(fromS);
        }

        static LinkedList<int> BFS(LinkedList<Node>[] graph, int node)
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
            int i = pred.Length - 1;
            LinkedList<int> Path = new(); Path.AddFirst(i);
            while (pred[i] != -1)
            {
                Path.AddFirst(pred[i] -1);
                i = pred[i] - 1;
            }
            return Path; // returns start of path from s to t

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

        public Node(int Value, int Capacity)
        {
            this.Pos = Value;
            this.Flow = 0;
            this.Capacity = Capacity;
            this.Forward = true;
        }

        public override bool Equals(object obj)
        {
            var item = obj as Node;
            return item.Pos == this.Pos;

        }

        public override string ToString()
        {
            return $"{Pos}, {Capacity}";
        }
    }

    
}
