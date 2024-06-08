using server.models;

namespace Monaco.Utility;

public static class ModelUtility
{
    public static string UpdateModel(string model, Operation op)
    {
        if (op.IsSimpleInsert)
        {
            return model.Insert(GetIndex(model, op.StartColumn, op.StartLine), op.Text);
        }
        else
        {
            return ReplaceSubstring(model, op.Text, GetIndex(model, op.StartColumn, op.StartLine), GetIndex(model, op.EndColumn, op.EndLine));
        }
    }

    public static int GetSIndex(string model, int col, int line)
    {
        int index = 0;
        int newLineCount = 0;
        int colCount = 0;

        // TODO just use model string array unnecessary lmao
        char[] arr = model.ToCharArray();
        while (index < arr.Count() && newLineCount < line - 1) {
            if (arr[index] == '\n') newLineCount++;
            index++;
        }
        while (index < arr.Count() && colCount < col - 1) {
            colCount++;
            index++;
        }

        return index;
    }

    public static int GetIndex(string input, int column, int line)
    {
        int currentLine = 1;
        int currentIndex = 0;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (currentLine == line && column == 1)
                return currentIndex;

            if (c == '\n' || (c == '\r' && i + 1 < input.Length && input[i + 1] == '\n'))
            {
                currentLine++;
                if (currentLine > line)
                    break;
                if (c == '\r') // Handle \r\n
                    i++;
            }
            else if (currentLine == line)
            {
                column--;
                if (column == 0)
                    return currentIndex;
            }

            currentIndex++;
        }
        return currentIndex;
    }

    public static string ReplaceSubstring(string model, string incoming, int start, int end)
    {
        return model.Substring(0, start) + incoming + model.Substring(end, model.Length - end);
    }
}