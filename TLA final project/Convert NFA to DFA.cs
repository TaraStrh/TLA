using System;
using static System.Console;
using System.Linq;
using System.Collections.Generic;

class TLA
{
    static Dictionary<string, Dictionary<string, HashSet<string>>> NFA;
    static Dictionary<string, HashSet<string>> eClosure;

    static string StatesToString(HashSet<string> states)
    {
        return string.Join('-', states.OrderBy(x => x));
    }

    static HashSet<string> StringToStates(string states)
    {
        return new HashSet<string>(states.Split('-'));
    }

    static void ComputeEClosure()
    {
        eClosure = new Dictionary<string, HashSet<string>>();

        foreach (string state in NFA.Keys)
        {
            var closure = new HashSet<string> { state };

            if (NFA[state].ContainsKey("$"))
            {
                closure.UnionWith(NFA[state]["$"]);
            }

            eClosure[state] = closure;
        }
    }

    static string GetNextState(HashSet<string> startState, string symbol, out bool isTrapState)
    {
        var reachableStates = new HashSet<string>();
        isTrapState = false;

        foreach (string state in startState)
        {
            bool hasTransition = false;

            foreach (string eState in eClosure[state])
            {
                if (NFA[eState].TryGetValue(symbol, out var nextStates))
                {
                    reachableStates.UnionWith(nextStates);
                    hasTransition = true;
                }
            }

            if (!hasTransition)
            {
                isTrapState = true;
            }
        }

        var fullReachableStates = new HashSet<string>();
        foreach (string state in reachableStates)
        {
            fullReachableStates.UnionWith(eClosure[state]);
        }

        return StatesToString(fullReachableStates);
    }

    static int ConvertNFAtoDFA(string startState, string[] alphabet)
    {
        var allStates = new HashSet<string> { StatesToString(eClosure[startState]) };
        var queue = new Queue<string>();
        queue.Enqueue(StatesToString(eClosure[startState]));

        bool trapStateExists = false;

        while (queue.Count > 0)
        {
            string currentState = queue.Dequeue();

            foreach (string symbol in alphabet)
            {
                string nextState = GetNextState(StringToStates(currentState), symbol, out bool isTrapState);
                if (isTrapState)
                {
                    trapStateExists = true;
                }

                if (!string.IsNullOrEmpty(nextState) && allStates.Add(nextState))
                {
                    queue.Enqueue(nextState);
                }
            }
        }

        return trapStateExists ? allStates.Count + 1 : allStates.Count;
    }

    public static void Main()
    {
        int stateCount = int.Parse(ReadLine());
        string[] states = ReadLine().Split();
        int alphabetSize = int.Parse(ReadLine());
        string[] alphabet = ReadLine().Split();
        int finalStateCount = int.Parse(ReadLine());
        string[] finalStates = ReadLine().Split();
        int transitionCount = int.Parse(ReadLine());

        NFA = new Dictionary<string, Dictionary<string, HashSet<string>>>();
        foreach (string state in states)
        {
            NFA[state] = new Dictionary<string, HashSet<string>>();
        }

        for (int i = 0; i < transitionCount; i++)
        {
            string[] transition = ReadLine().Split(',');
            if (!NFA[transition[0]].ContainsKey(transition[1]))
            {
                NFA[transition[0]][transition[1]] = new HashSet<string>();
            }
            NFA[transition[0]][transition[1]].Add(transition[2]);
        }

        ComputeEClosure();

        WriteLine(ConvertNFAtoDFA(states[0], alphabet));
    }
}
