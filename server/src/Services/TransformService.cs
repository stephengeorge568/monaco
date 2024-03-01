using System.Globalization;
using Monaco.Services.Interfaces;
using server.models;

namespace Monaco.Services;

// Could be static class in current state, but would likely require
// integration of other async services later down the line.
public class TransformService : ITransformService
{
    private int RevisionId;
    private Dictionary<NumberFormatInfo, List<Operation>> history;

    public TransformService()
    {
        RevisionId = 0;
        history = new();
    }

    public List<Operation> GetRelevantHistory(int revisionId, Dictionary<int, List<Operation>> history)
    {
        var relevantRequests = new List<Operation>();
        foreach (var pair in history)
        {
            if (pair.Key >= revisionId)
            {
                relevantRequests.AddRange(pair.Value);
            }
        }
        return relevantRequests;
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
        if (prev.OriginatorId == next.OriginatorId) return false;

        var isPrevStartLineAfterNextEndLine = prev.StartLine > next.EndLine;
        var isSameLine = prev.StartLine == next.EndLine;

        if (isPrevStartLineAfterNextEndLine) return false;
        if (isSameLine)
        {
            if (IsSimpleInsert(next))
            {
                if (next.EndColumn < prev.StartColumn) return false;
            }
            else
            {
                if (next.EndColumn <= prev.StartColumn) return false;
            }
        }
        return true;
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
        if (IsSimpleInsert(prev) || IsSimpleInsert(next))
        {
            return [next];
        }

        if (IsECWithinRange(next, prev) && IsSCWithinRange(next, prev))
        {
            var otherNext = new Operation() {
                Text = "",
                OriginatorId = next.OriginatorId,
                StartColumn = next.StartColumn,
                EndColumn = next.EndColumn,
                StartLine = next.StartLine,
                EndLine = next.EndLine,
                RevisionId = next.RevisionId,
            };

            //shift end of next to start of prev
            next.EndColumn = prev.StartColumn;
            next.EndLine = prev.StartLine;

            //shift start of otherNext to end of prev
            otherNext.StartColumn = prev.EndColumn;
            otherNext.StartLine = prev.EndLine;
            return [next, otherNext];
        }

        if (IsSCWithinRange(next, prev))
        {
            next.EndLine = prev.StartLine;
            next.EndColumn = prev.StartColumn;
        }

        if (IsECWithinRange(next, prev))
        {
            next.StartLine = prev.EndLine;
            next.StartColumn = prev.EndColumn;
        }

        return [next];
    }

    public List<Operation> Transform(Operation op, Dictionary<NumberFormatInfo, List<Operation>> history)
    {
        throw new NotImplementedException();
    }

    public Operation TransformOperation(Operation prev, Operation next)
    {
        throw new NotImplementedException();
    }

    public bool IsSimpleInsert(Operation op)
    {
        return op.EndColumn == op.StartColumn && op.EndLine == op.StartLine;
    }
}