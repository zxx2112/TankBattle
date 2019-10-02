using System.Collections.Generic;

public class Keylist<TKey, TValue>
{
    //
    // Proterties
    //

    public List<TKey> Keys
    { get; protected set; }

    public List<List<TValue>> Values
    { get; protected set; }

    //
    // Properties (complex)
    //

    public int KeyCount
    {
        get { return Keys.Count; }
    }

    public int ValueCount
    {
        get { return ValuveCountInternal(); }
    }

    public TKey this[int k]
    {
        get { return Keys[k]; }
    }

    public TValue this[int k, int v]
    {
        get { return Values[k][v]; }
    }

    //
    // Constructor
    //

    public Keylist()
    {
        Keys = new List<TKey>();
        Values = new List<List<TValue>>();
    }

    //
    // Methods
    //

    public void Add(TKey key, TValue value)
    {
        if (false == Keys.Contains(key))
        {
            Keys.Add(key);
            Values.Add(new List<TValue>());
        }

        for (int i = 0; i < Keys.Count; i++)
        {
            if (Keys[i].Equals(key))
            {
                Values[i].Add(value);
            }
        }
    }

    int ValuveCountInternal()
    {
        int result = 0;

        for (int i = 0; i < Values.Count; i++)
        {
            result += Values[i].Count;
        }

        return result;
    }

    public TValue[] ToArray()
    {
        List<TValue> result = new List<TValue>();

        for(int i = 0; i < Values.Count; i++)
        {
            for(int j = 0; j < Values[i].Count; j++)
            {
                result.Add(Values[i][j]);
            }
        }

        return result.ToArray();
    }
}
