using System.Collections.ObjectModel;
using System.Text;

namespace YandexConstestsDotNet;

public class Program
{
    static void Main(string[] args)
    {
        Solutions.ShortestPath();
    }
}

public class Solutions()
{
    public static void Dfs()
    {
        var (size, edges) = ReadGraphByEdges();
        var graph = new Graph(edges, size);
        var result = graph.SearchElementByDfs(1);
        if (result.Count == 0)
        {
            Console.WriteLine(0);
            return;
        }
        var sb = new StringBuilder();
        foreach (var id in result) sb.Append(id).Append(' ');
        Console.WriteLine(result.Count);
        Console.WriteLine(sb.ToString().Trim());
    }

    public static void ConnectivityElement()
    {
        var (size, edges) = ReadGraphByEdges();
        var graph = new Graph(edges, size);
        var result = graph.FindComponents();
        Console.WriteLine(result.Count);
        if (result.Count == 0) return;
        var sb = new StringBuilder();
        foreach (var component in result)
        {
            sb.Append(component.Count).Append('\n');
            foreach (var id in component) sb.Append(id).Append(' ');
            sb.Remove(sb.Length - 1, 1).Append('\n');
        }
        Console.WriteLine(sb.ToString().Trim());
    }

    public static void Copycat()
    {
        var (size, edges) = ReadGraphByEdges();
        var graph = new Graph(edges, size);
        if (graph.IsBipartite()) Console.WriteLine("YES");
        else Console.WriteLine("NO");
    }

    public static void TopologicalSort()
    {
        var (size, edges) = ReadGraphByEdges();
        var graph = new OrientedGraph(edges, size);
        var result = graph.Topsort(out bool hasCycle);
        if (hasCycle)
        {
            Console.WriteLine(-1);
            return;
        }
        var sb = new StringBuilder();
        foreach (var id in result) sb.Append(id).Append(' ');
        Console.WriteLine(sb.ToString().Trim());
    }

    public static void ShortestPath()
    {
        var graph = new Graph(ReadGraphByAdjencyMatrix());
        var startEnd = Console.ReadLine()!.Split(' ');
        Console.WriteLine(graph.ShortestPath(int.Parse(startEnd[0]), int.Parse(startEnd[1])));
    }

    private static (int size, IEnumerable<Connection> edges) ReadGraphByEdges()
    {
        var dimensions = Console.ReadLine()!.Split(' ');
        var size = int.Parse(dimensions[0]);
        var edgeCount = int.Parse(dimensions[1]);
        var edges = new List<Connection>(edgeCount);
        for (int _ = 0; _ < edgeCount; _++)
        {
            var edge = Console.ReadLine()!.Split(' ');
            edges.Add(new Connection(int.Parse(edge[0]), int.Parse(edge[1])));
        }
        return (size, edges);
    }

    private static int[][] ReadGraphByAdjencyMatrix()
    {
        var size = int.Parse(Console.ReadLine()!);
        var adjencyMatrix = new int[size][];
        for (int i = 0; i < size; i++) adjencyMatrix[i] = new int[size];
        for (int i = 0; i < size; i++)
        {
            var values = Console.ReadLine()!.Split(' ').Select(int.Parse).ToArray();
            for (int j = 0; j < size; j++)
            {
                adjencyMatrix[i][j] = values[j];
                adjencyMatrix[j][i] = values[j];
            }
        }
        return adjencyMatrix;
    }
}

public static class GraphAlgorithms
{
    public static SortedSet<int> SearchElementByDfs(this Graph graph, int id)
    {
        SortedSet<int> result = [];
        var visited = new bool[graph.Count];
        var found = false;
        foreach (var node in graph.Nodes.Values)
        {
            dfs(node);
            if (found) return result;
            result = [];
        }
        return result;

        void dfs(Node current)
        {
            if (visited[current.Id - 1]) return;
            if (current.Id == id) found = true;
            visited[current.Id - 1] = true;
            result.Add(current.Id);
            foreach (var neigthbour in current.Neightbours) dfs(neigthbour);
        }
    }

    public static ICollection<SortedSet<int>> FindComponents(this Graph graph)
    {
        Collection<SortedSet<int>> result = [];
        SortedSet<int> component;
        var visited = new bool[graph.Count];
        foreach (var node in graph.Nodes.Values)
        {
            if (visited[node.Id - 1]) continue;
            component = [];
            dfs(node);
            result.Add(component);
        }
        return result;

        void dfs(Node current)
        {
            if (visited[current.Id - 1]) return;
            visited[current.Id - 1] = true;
            component.Add(current.Id);
            foreach (var neigthbour in current.Neightbours) dfs(neigthbour);
        }
    }

    public static bool IsBipartite(this Graph graph)
    {
        if (graph.Count <= 1) return false;
        var visited = new Dictionary<int, bool>(graph.Count);
        var result = true;

        foreach (var node in graph.Nodes.Values)
        {
            if (visited.ContainsKey(node.Id)) continue;
            result &= dfs(node);
            if (!result) return false;
        }

        return true;

        bool dfs(Node current, int from = -1, bool flag = false)
        {
            if (visited.TryGetValue(current.Id, out bool value)) return value == flag;
            visited[current.Id] = flag;
            foreach (var neigthbour in current.Neightbours)
            {
                if (neigthbour.Id == from) continue;
                if (!dfs(neigthbour, current.Id, !flag)) return false;
            }
            return true;
        }
    }

    public static ICollection<int> Topsort(this OrientedGraph graph, out bool hasCycle)
    {
        hasCycle = false;
        var visited = new bool[graph.Count];
        var finished = new bool[graph.Count];
        var order = new Stack<int>(graph.Count);

        foreach (var node in graph.Nodes.Values)
        {
            if (visited[node.Id - 1]) continue;
            if (!dfs(node))
            {
                hasCycle = true;
                return [];
            }
        }

        bool dfs(Node current)
        {
            if (visited[current.Id - 1])
            {
                if (finished[current.Id - 1]) return true;
                return false;
            }
            visited[current.Id - 1] = true;
            foreach (var neigthbour in current.Neightbours)
            {
                if (!dfs(neigthbour)) return false;
            }
            finished[current.Id - 1] = true;
            order.Push(current.Id);
            return true;
        }

        var result = new List<int>(order.Count);
        while(order.Count != 0) result.Add(order.Pop());
        return result;
    }

    public static int ShortestPath(this Graph graph, int start, int end)
    {
        if (start == end) return 0;
        var queue = new Queue<Node>();
        var visited = new HashSet<Node>();
        var levels = new Dictionary<Node, int>();
        levels[graph.Nodes[start]] = 0;
        queue.Enqueue(graph.Nodes[start]);

        while (queue.Count != 0)
        {
            var current = queue.Dequeue();
            if (visited.Contains(current)) continue;
            if (current.Id == end) return levels[current];
            foreach (var node in current.Neightbours)
            {
                queue.Enqueue(node);
                levels[node] = levels[current] + 1;
            }
            visited.Add(current);
        }

        return 0;
    }
}

public class Graph
{
    public int Count { get { return Nodes.Count; } }

    public Dictionary<int, Node> Nodes { get; }

    public Graph(IEnumerable<Connection> edges, int size)
    {
        Nodes = new(size);
        for (int i = 1; i <= size; i++) Add(i);
        foreach (var conn in edges) Connect(conn.Id1, conn.Id2);
    }

    public Graph(int[][] adjencyMatrix)
    {
        Nodes = new(adjencyMatrix.Length);
        for (int i = 1; i <= adjencyMatrix.Length; i++) Add(i);
        for (int i = 0; i < adjencyMatrix.Length; i++)
        {
            for (int j = 0; j < adjencyMatrix.Length; j++)
                if (adjencyMatrix[i][j] == 1) Connect(i + 1, j + 1);
        }
    }

    public void Add(int id)
    {
        if (Nodes.ContainsKey(id)) return;
        Nodes.Add(id, new Node(id));
    }

    public void Connect(int id1, int id2)
    {
        if (id1 == id2) return;
        if (!(Nodes.TryGetValue(id1, out Node? node1) && Nodes.TryGetValue(id2, out Node? node2))) return;
        node1.Neightbours.Add(node2);
        node2.Neightbours.Add(node1);
    }
}

public class OrientedGraph
{
    public int Count { get { return Nodes.Count; } }

    public Dictionary<int, Node> Nodes { get; }

    public OrientedGraph(IEnumerable<Connection> edges, int size)
    {
        Nodes = new(size);
        for (int i = 1; i <= size; i++) Add(i);
        foreach (var conn in edges) Connect(conn.Id1, conn.Id2);
    }

    public void Add(int id)
    {
        if (Nodes.ContainsKey(id)) return;
        Nodes.Add(id, new Node(id));
    }

    public void Connect(int id1, int id2)
    {
        if (id1 == id2) return;
        if (!(Nodes.TryGetValue(id1, out Node? node1) && Nodes.TryGetValue(id2, out Node? node2))) return;
        node1.Neightbours.Add(node2);
    }
}

public class Node(int id)
{
    public int Id { get; } = id;

    public HashSet<Node> Neightbours { get; } = [];

    public override int GetHashCode() => Id;

    public void AddNeightbours(params Node[] nodes)
    {
        foreach (var node in nodes) Neightbours.Add(node);
    }
}

public class SortedNode(int id) : IComparable
{
    public int Id { get; } = id;

    public SortedSet<SortedNode> Neightbours { get; } = [];

    public override int GetHashCode() => Id;

    public void AddNeightbours(params SortedNode[] nodes)
    {
        foreach (var node in nodes) Neightbours.Add(node);
    }

    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;

        if (obj is not SortedNode node) throw new ArgumentException("Object is not a Node.");
        return Id.CompareTo(node.Id);
    }
}

public readonly struct Connection(int id1, int id2)
{
    public int Id1 { get; } = id1;
    public int Id2 { get; } = id2;
}
