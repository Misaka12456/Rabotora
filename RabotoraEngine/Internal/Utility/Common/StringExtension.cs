namespace RabotoraEngine.Internal.Utility.Common;

/// <summary>
/// Provides extension methods for the <see cref="string"/> class.
/// </summary>
public static class StringExtension
{
    private const int ALPHABET = 65536;
    private unsafe struct ACNode
    {
        public int* Next;
        public int Fail;
        public int WordId;
    }
    private unsafe struct ACAutomaton
    {
        public ACNode* Nodes;
        public int NodeCount;
        public string[] Keywords;
    }
    
    /// <summary>
    /// Replaces all occurrences of the specified characters in the source string with the given replacement character.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="keyChars">A collection of characters to be replaced.</param>
    /// <param name="replacement">The replacement character.</param>
    /// <returns>The modified string with all occurrences of the characters replaced.</returns>
    /// <seealso cref="ReplaceAll(string, IEnumerable{string}, string)"/>
    public static string ReplaceAll(this string source, IEnumerable<char> keyChars, char replacement)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keyChars);
        ArgumentNullException.ThrowIfNull(replacement);

        var keywords = new List<string>(keyChars.Select(c => c.ToString()));

        return ReplaceAll(source, keywords, replacement.ToString());
    }

    // ----- WTF Code region begin -----
    /// <summary>
    /// Replaces all occurrences of the specified keywords in the source string with the given replacement string using the Aho-Corasick algorithm.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="keywords">A collection of keywords to be replaced.</param>
    /// <param name="replacement">The replacement string.</param>
    /// <returns>The modified string with all occurrences of the keywords replaced.</returns>
    /// <seealso cref="ReplaceAll(string, IEnumerable{char}, char)"/>
    public static string ReplaceAll(this string source, IEnumerable<string> keywords, string replacement)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keywords);
        ArgumentNullException.ThrowIfNull(replacement);

        var keywordList = new List<string>();
        foreach (var k in keywords)
        {
            if (!string.IsNullOrEmpty(k))
            {
                keywordList.Add(k);
            }
        }

        if (keywordList.Count == 0)
        {
            return source;
        }

        ACAutomaton automaton = BuildAutomaton(keywordList);

        return ReplaceAllAhoUnsafe(source, replacement, automaton);
    }
    private static unsafe ACAutomaton BuildAutomaton(List<string> keywords)
    {
        int maxNodes = 1; // root
        foreach (var w in keywords)
        {
            maxNodes += w.Length;
        }

        var nodes = new ACNode[maxNodes];
        var nexts = new int[maxNodes * ALPHABET];
        for (int i = 0; i < maxNodes; i++)
        {
            nodes[i].Fail = 0;
            nodes[i].WordId = -1;
        }

        fixed (ACNode* pNodes = nodes)
        fixed (int* pNext = nexts)
        {
            for (int i = 0; i < maxNodes; i++)
            {
                pNodes[i].Next = pNext + i * ALPHABET;
            }

            int nodeCount = 1;

            for (int wId = 0; wId < keywords.Count; wId++)
            {
                string word = keywords[wId];
                int cur = 0;
                for (int j = 0; j < word.Length; j++)
                {
                    char c = word[j];
                    if (pNodes[cur].Next[c] == 0)
                    {
                        pNodes[cur].Next[c] = nodeCount;
                        pNodes[nodeCount].Fail = 0;
                        pNodes[nodeCount].WordId = -1;
                        nodeCount++;
                    }
                    cur = pNodes[cur].Next[c];
                }
                pNodes[cur].WordId = wId;
            }

            var queue = new Queue<int>();
            for (int c = 0; c < ALPHABET; c++)
            {
                int nxt = pNodes[0].Next[c];
                if (nxt != 0)
                {
                    queue.Enqueue(nxt);
                }
            }

            while (queue.Count > 0)
            {
                int u = queue.Dequeue();
                for (int c = 0; c < ALPHABET; c++)
                {
                    int v = pNodes[u].Next[c];
                    if (v != 0)
                    {
                        queue.Enqueue(v);
                        int f = pNodes[u].Fail;
                        while (f != 0 && pNodes[f].Next[c] == 0)
                        {
                            f = pNodes[f].Fail;
                        }
                        pNodes[v].Fail = pNodes[f].Next[c];
                        if (pNodes[pNodes[v].Fail].WordId != -1 && pNodes[v].WordId == -1)
                        {
                            pNodes[v].WordId = pNodes[pNodes[v].Fail].WordId;
                        }
                    }
                    else
                    {
                        pNodes[u].Next[c] = pNodes[pNodes[u].Fail].Next[c];
                    }
                }
            }

            return new ACAutomaton { Nodes = pNodes, NodeCount = nodeCount, Keywords = keywords.ToArray() };
        }
    }

    private static unsafe string ReplaceAllAhoUnsafe(string source, string replacement, ACAutomaton automaton)
    {
        if (string.IsNullOrEmpty(source)) return source;

        int srcLen = source.Length;
        bool[] mask = new bool[srcLen];
        int* matchedIndex = stackalloc int[srcLen];
        for (int i = 0; i < srcLen; i++)
        {
            matchedIndex[i] = -1;
        }

        fixed (char* pSrc = source)
        {
            int state = 0;
            for (int i = 0; i < srcLen; i++)
            {
                char c = pSrc[i];
                state = automaton.Nodes[state].Next[c];
                int wordId = automaton.Nodes[state].WordId;
                if (wordId != -1)
                {
                    int len = automaton.Keywords[wordId].Length;
                    int start = i - len + 1;
                    if (start >= 0)
                    {
                        for (int j = 0; j < len; j++)
                        {
                            mask[start + j] = true;
                            matchedIndex[start + j] = wordId;
                        }
                    }
                }
            }
        }

        var result = new char[srcLen * 2];
        int ri = 0;
        int si = 0;

        while (si < srcLen)
        {
            if (mask[si])
            {
                int wId = matchedIndex[si];
                int wLen = automaton.Keywords[wId].Length;
                for (int j = 0; j < replacement.Length; j++)
                {
                    result[ri++] = replacement[j];
                }
                si += wLen;
            }
            else
            {
                result[ri++] = source[si++];
            }
        }

        return new string(result, 0, ri);
    }
    // ----- WTF Code region end -----
}