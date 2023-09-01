using System;
using System.Text;
using UnityEngine;

public class CircularQueue<T> where T : class
{
    private T[] m_Array;
    private int m_Capacity;
    private int m_Count = 0;
    private int m_Head = 0;
    private int m_Tail = -1;

    public CircularQueue(int capacity = 10)
    {
        m_Array = new T[capacity];
        m_Capacity = capacity;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder(m_Capacity * 6);
        for (int i = 0; i < m_Capacity; i++)
        {
            sb.Append(m_Array[i]);
            sb.Append(',');
            sb.Append(' ');
        }
        return sb.ToString();
    }

    public bool IsEmpty()
    {
        return m_Count == 0;
    }

    public bool IsFull()
    {
        return m_Count == m_Capacity;
    }

    public T Peek()
    {
        if (IsEmpty())
        {
            throw new InvalidOperationException("Queue is empty");
        }

        return m_Array[m_Head];
    }

    public void Enqueue(T t)
    {
        if (IsFull())
        {
            var newarray = new T[m_Capacity + m_Capacity];

            int index = 0;
            for (int i = m_Head; i < m_Capacity; i++)
            {
                newarray[index++] = m_Array[i];
            }

            if (m_Head != 0)
            {
                for (int i = 0; i < m_Tail; i++)
                {
                    newarray[index++] = m_Array[i];
                }
            }
            m_Array = newarray;
            m_Head = 0;
            m_Tail = m_Capacity - 1;
            m_Capacity += m_Capacity;
        }

        if (m_Tail + 1 == m_Capacity)
        {
            m_Tail = 0;
        }
        else
        {
            m_Tail++;
        }
        m_Array[m_Tail] = t;

        m_Count++;
    }

    public T Dequeue()
    {
        if (IsEmpty())
        {
            throw new InvalidOperationException("Queue is empty");
        }

        T value = m_Array[m_Head];
        m_Array[m_Head] = null;

        if (m_Head == m_Tail)
        {
            m_Head = 0;
            m_Tail = -1;
        }
        else
        {
            if (m_Head + 1 == m_Capacity)
            {
                m_Head = 0;
            }
            else
            {
                m_Head++;
            }
        }
        m_Count--;

        return value;
    }
}