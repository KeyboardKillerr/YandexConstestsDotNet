using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



public class Program
{
    static void Main(string[] args)
    {
        SolutionsPrograms.RunFifth();
    }
}

public static class SolutionsPrograms
{
    public static void RunFirst()
    {
        Console.WriteLine(Solutions.Sequence(Console.ReadLine()!));
    }

    public static void RunSecond()
    {
        var days = int.Parse(Console.ReadLine()!);
        var measurments = Console.ReadLine()!;

        var levels = new List<int>(days);
        var start = 0;
        var day = 0;
        measurments += ' ';
        for (int i = 0; i < measurments.Length; i++)
        {
            if (measurments[i] != ' ') continue;

            levels.Add(int.Parse(measurments.Substring(start, i - start)));
            start = i + 1;
            day++;
        }

        var result = Solutions.Repair(levels);
        if (result == "") Console.WriteLine("NO");
        else
        {
            Console.WriteLine("YES");
            Console.WriteLine(result);
        }
    }

    public static void RunThird()
    {
        var keylog = Console.ReadLine()!;
        var keyset = Console.ReadLine()!;
        var len = int.Parse(Console.ReadLine()!);
        var result = Solutions.FindPassword(keylog, keyset, len);
        if (result == "") Console.WriteLine(-1);
        else Console.WriteLine(result);
    }

    public static void RunFifth()
    {
        var start = toSeconds(Console.ReadLine()!);
        var count = int.Parse(Console.ReadLine()!);
        var responses = new List<Response>(count);
        for (int i = 0; i < count; i++)
        {
            var response = new Response();
            var args = Console.ReadLine()!.Split(' ');
            var time = toSeconds(args[1]);
            if (time < start) time += 86400;

            response.Time = (time - start) / 60;
            response.Team = args[0];
            response.Server = args[2].ToCharArray()[0];
            response.Status = args[3];

            responses.Add(response);
        }

        var results = Solutions.Ctf(responses);

        var k = 1;
        var lastk = 1;
        TeamResult? last = null;
        foreach (var result in results)
        {
            if (last is null) last = result;
            if (result.Time == last.Time && result.Servers == last.Servers)
            {
                Console.WriteLine($"{lastk} {result.Team} {result.Servers} {result.Time}");
            }
            else
            {
                lastk = k;
                Console.WriteLine($"{k} {result.Team} {result.Servers} {result.Time}");
            }
            k++;
        }

        int toSeconds(string time)
        {
            var tmp = TimeOnly.Parse(time);
            return (tmp.Hour * 60 + tmp.Minute) * 60;
        }
    }

    public static void RunSixth()
    {
        var count = int.Parse(Console.ReadLine()!);
        var tasks = new List<NodeRepr>();
        for (int i = 1; i <= count; i++)
        {
            var task = Console.ReadLine()!.Split(' ').Select(int.Parse).ToArray();

            var neighbours = new List<int>();
            for (int j = 1; j < task.Length; j++) neighbours.Add(task[j]);
            var nodeRepr = new NodeRepr(i, task[0], neighbours);
            tasks.Add(nodeRepr);
        }
        var graph = new OrientedGraph(tasks);
        Console.WriteLine(Solutions.Threads(graph));
    }
}

public static class Solutions
{
    public static IEnumerable<TeamResult> Ctf(IEnumerable<Response> responses)
    {
        var results = new Dictionary<string, Dictionary<char, int>>();
        var table = new Dictionary<string, TeamResult>();
        foreach (var response in responses)
        {
            if (response.Status == "DENIED" || response.Status == "FORBIDEN" || response.Status == "ACCESSED")
            {
                var time = 20;
                if (response.Status == "ACCESSED") time = response.Time;
                if (results.TryGetValue(response.Team, out Dictionary<char, int>? value))
                {
                    if (value.ContainsKey(response.Server))
                        value[response.Server] += time;
                    else
                        value[response.Server] = time;
                }
                else
                {
                    results[response.Team] = new();
                    results[response.Team][response.Server] = time;
                }
            }

            if (response.Status == "ACCESSED")
            {
                if (!table.ContainsKey(response.Team))
                {
                    table[response.Team] = new() { Team = response.Team };
                }
                var result = table[response.Team];
                result.Time += results[response.Team][response.Server];
                result.Servers++;
            }
        }

        var ordered = table.Values;
        return ordered.OrderBy(x => x.Servers).ThenBy(x => x.Time).ThenBy(x => x.Team);
    }

    public static string Repair(IEnumerable<int> levels)
    {
        var skip = 0;
        var lastValid = 0;
        var result = new StringBuilder();
        foreach (var level in levels)
        {
            if (level == -1)
            {
                skip++;
                continue;
            }
            if (skip != 0)
            {
                if (level <= lastValid + skip) return "";
                var last = lastValid;
                while (skip != -1)
                {
                    if (skip == 0) result.Append(level - last).Append(' ');
                    else
                    {
                        last++;
                        result.Append(1).Append(' ');
                    }
                    skip--;
                }
                skip = 0;
            }
            else
            {
                result.Append(level - lastValid).Append(' ');
                lastValid = level;
            }
        }

        while (skip != 0)
        {
            result.Append(1).Append(' ');
            skip--;
        }
        return result.ToString().Trim();
    }

    public static string FindPassword(string keylog, string keyset, int maxLength)
    {
        var result = "";
        var keys = new Dictionary<char, int>();
        foreach (var key in keyset) keys.Add(key, 0);

        var left = 0;
        var right = -1;
        var len = 0;
        var max = 0;
        var window = new Queue<char>(maxLength);
        var hadReset = false;
        foreach (var c in keylog)
        {
            if (!keys.ContainsKey(c))
            {
                right++;
                if (!hadReset) resetAll();
                hadReset = true;
                continue;
            }
            if (hadReset)
            {
                left = right + 1;
                hadReset = false;
            }
            if (right != keylog.Length - 1)
            {
                right++;
                if (right - left > maxLength - 1) left++;
            }
            len = right - left + 1;
            if (window.Count == maxLength)
            {
                var removed = window.Dequeue();
                keys[removed]--;
            }
            window.Enqueue(c);
            keys[c]++;

            if (!containsAllKeys()) continue;
            if (len < max) continue;

            result = keylog.Substring(left, len);
            max = len;
        }

        return result;

        bool containsAllKeys()
        {
            foreach (var count in keys.Values) if (count == 0) return false;
            return true;
        }

        void resetAll()
        {
            foreach (var key in keys.Keys) keys[key] = 0;
            while (window.Count != 0) window.Dequeue();
        }

    }

    public static string Sequence(string input)
    {
        if (input.Length == 1) return input;

        var result = new StringBuilder();
        var left = -1;
        var start = 0;
        input += ',';
        for (int i = 1; i < input.Length; i++)
        {
            if (input[i] == '-')
            {
                left = int.Parse(input.Substring(start, i - start));
                start = i + 1;
            }
            else if (input[i] == ',')
            {
                if (left != - 1)
                {
                    var right = int.Parse(input.Substring(start, i - start));
                    for (int j = left; j <= right; j++) result.Append(j).Append(' ');
                    left = -1;
                }
                else result.Append(int.Parse(input.Substring(start, i - start))).Append(' ');
                start = i + 1;
            }
        }

        return result.ToString().Trim();
    }

    public static int Threads(OrientedGraph tasks)
    {
        var max = 0;
        var time = 0;
        var visited = new bool[tasks.Count];

        foreach (var task in tasks.Nodes.Values)
        {
            time = calculateCost(task);
            if (time > max) max = time;
        }
        
        int calculateCost(Node task)
        {
            if (visited[task.Id - 1]) return task.Cost;

            var max = 0;
            foreach (var neightbour in task.Neightbours)
            {
                var cost = calculateCost(neightbour);
                if (cost > max) max = cost;
            }

            task.Cost += max;
            visited[task.Id - 1] = true;
            return task.Cost;
        }

        return max;
    }
}

public class OrientedGraph
{
    public int Count { get { return Nodes.Count; } }

    public Dictionary<int, Node> Nodes { get; }

    public OrientedGraph(ICollection<NodeRepr> nodes)
    {
        Nodes = new(nodes.Count);
        foreach (var node in nodes)
            Add(node.Id, node.Cost);
        foreach (var node in nodes)
            foreach (var neightbourId in node.Neightbours) Connect(node.Id, neightbourId);
    }

    public void Add(int id, int cost)
    {
        if (Nodes.ContainsKey(id)) return;
        Nodes.Add(id, new Node(id, cost));
    }

    public void Connect(int id1, int id2)
    {
        if (id1 == id2) return;
        if (!(Nodes.TryGetValue(id1, out Node? node1) && Nodes.TryGetValue(id2, out Node? node2))) return;
        node1.Neightbours.Add(node2);
    }
}

public class Response
{
    public string Team { get; set; } = null!;
    public char Server { get; set; }
    public string Status { get; set; } = null!;
    public int Time { get; set; }
}

public class TeamResult
{
    public string Team { get; set; } = null!;
    public int Time { get; set; } = 0;
    public int Servers { get; set; } = 0;
}

public class Node
{
    public Node(int id, int cost)
    {
        Id = id;
        Cost = cost;
    }

    public int Id { get; }

    public int Cost { get; set; }

    public HashSet<Node> Neightbours { get; } = new();

    public override int GetHashCode() => Id;

    public void AddNeightbours(params Node[] nodes)
    {
        foreach (var node in nodes) Neightbours.Add(node);
    }
}

public readonly struct NodeRepr
{
    public NodeRepr(int id, int cost, IEnumerable<int> neightbours)
    {
        Id = id;
        Cost = cost;
        Neightbours = neightbours;
    }

    public int Id { get; }
    public int Cost { get; }
    public IEnumerable<int> Neightbours { get; }
}
