using UnityEngine;
using System;
using System.Collections.Generic;

public class MiniGameArrowsQTE : MonoBehaviour
{
    public enum Arrow { Left, Up, Right, Down }

    private List<Arrow> sequence = new List<Arrow>();
    private int index;
    private Action<bool> onComplete;
    private Action<string> onTextChanged;

    private static readonly Arrow[] pool = new[] { Arrow.Left, Arrow.Up, Arrow.Right }; 
    // If you want Down included, add Arrow.Down to the pool above.

    /// <summary>
    /// Run the mini-game.
    /// </summary>
    public void Run(int length, Action<string> onTextChanged, Action<bool> onComplete)
    {
        this.onComplete = onComplete;
        this.onTextChanged = onTextChanged;

        GenerateSequence(length);
        index = 0;
        UpdateInstruction();
    }

    void Update()
    {
        if (onComplete == null) return; // not running

        Arrow? input = ReadArrowInput();
        if (input == null) return;

        if (input.Value == sequence[index])
        {
            index++;
            UpdateInstruction();

            if (index >= sequence.Count)
            {
                Finish(true);
            }
        }
        else
        {
            // Wrong key — reset progress (or you could just decrement)
            index = 0;
            UpdateInstruction();
        }
    }

    private void Finish(bool success)
    {
        var cb = onComplete;
        onComplete = null; // prevent re-entry
        cb?.Invoke(success);
    }

    private void GenerateSequence(int length)
    {
        sequence.Clear();
        for (int i = 0; i < length; i++)
        {
            sequence.Add(pool[UnityEngine.Random.Range(0, pool.Length)]);
        }
    }

    private void UpdateInstruction()
    {
        string seq = string.Join(", ", sequence.ConvertAll(ToWord));
        string cursor = new string('_', Mathf.Clamp(sequence.Count - index, 0, sequence.Count)); // simple progress indicator
        string text = $"Press {seq} to say trick-or-treat!\nProgress: {index}/{sequence.Count}";
        onTextChanged?.Invoke(text);
    }

    private Arrow? ReadArrowInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))  return Arrow.Left;
        if (Input.GetKeyDown(KeyCode.UpArrow))    return Arrow.Up;
        if (Input.GetKeyDown(KeyCode.RightArrow)) return Arrow.Right;
        if (Input.GetKeyDown(KeyCode.DownArrow))  return Arrow.Down;
        return null;
    }

    private static string ToWord(Arrow a)
    {
        switch (a)
        {
            case Arrow.Left:  return "LEFT";
            case Arrow.Up:    return "UP";
            case Arrow.Right: return "RIGHT";
            case Arrow.Down:  return "DOWN";
        }
        return "?";
    }
}
