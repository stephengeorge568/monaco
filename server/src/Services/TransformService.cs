using System.Globalization;
using Monaco.Services.Interfaces;
using server.models;

namespace Monaco.Services;

public class TransformService : ITransformService
{
    private int RevisionId;
    private Dictionary<NumberFormatInfo, List<Operation>> history;

    public TransformService()
    {
        RevisionId = 0;
        history = new();
    }

    public List<Operation> GetRelevantHistory(int revisionId, Dictionary<NumberFormatInfo, List<Operation>> history)
    {
        throw new NotImplementedException();
    }

    public bool IsECWithinRange(Operation prev, Operation next)
    {
        if (next.EndLine < prev.EndLine && next.EndLine > prev.StartLine) return true;

        if (next.EndLine == prev.EndLine)
        {
            if (next.EndLine == prev.StartLine)
            {
                if (next.EndColumn <= prev.StartColumn) return false;
            }
            if (next.EndColumn < prev.EndColumn) return true;
        }

        if (next.EndLine == prev.StartLine && next.EndLine != prev.EndLine)
        {
            if (next.EndColumn > prev.StartColumn) return true;
        }

        return false;
    }

    public bool IsPreviousOperationRelevant(Operation prev, Operation next)
    {
        throw new NotImplementedException();
    }

    public bool IsSCWithinRange(Operation prev, Operation next)
    {
        if (next.StartLine > prev.StartLine && next.StartLine < prev.EndLine)
        {
            return true;
        }

        if (next.StartLine == prev.StartLine)
        {
            if (next.StartLine == prev.EndLine)
            {
                if (!(next.StartColumn < prev.EndColumn)) return false;
            }
            if (next.StartColumn >= prev.StartColumn) return true;
        }

        if (next.StartLine == prev.EndLine && next.StartLine != prev.StartLine)
        {
            if (next.StartColumn < prev.EndColumn) return true;
        }
        return false;
    }

    public List<Operation> ResolveConflictingRanges(Operation prev, Operation next)
    {
        throw new NotImplementedException();
    }

    public List<Operation> Transform(Operation op, Dictionary<NumberFormatInfo, List<Operation>> history)
    {
        throw new NotImplementedException();
    }

    public Operation TransformOperation(Operation prev, Operation next)
    {
        throw new NotImplementedException();
    }
}