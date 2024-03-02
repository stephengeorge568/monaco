using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
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
            var otherNext = new Operation()
            {
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

    public List<Operation> Transform(Operation op, Dictionary<int, List<Operation>> history)
    {
        var transformedRequests = new List<Operation>();

        var toTransformQueue = new Queue<(Operation Operation, int RevisionId)>();
        toTransformQueue.Enqueue((op, -1));

        while (toTransformQueue.TryDequeue(out var currentRequest))
        {
            var relevantHistory = GetRelevantHistory(op.RevisionId, history);

            for (int i = 0; i < relevantHistory.Count; i++)
            {
                var historicalRequest = relevantHistory[i];

                if (op.OriginatorId != historicalRequest.OriginatorId)
                {
                    var pair = ResolveConflictingRanges(historicalRequest, currentRequest.Operation);

                    if (currentRequest.RevisionId < i)
                    {
                        currentRequest.Operation = TransformOperation(historicalRequest, pair[0]);
                    }

                    if (pair.Count == 2)
                    {
                        toTransformQueue.Enqueue((pair[1], i));
                    }
                }
            }

            foreach (var newHistoralRequest in transformedRequests)
            {
                if (IsPreviousOperationRelevant(newHistoralRequest, currentRequest.Operation))
                {
                    currentRequest.Operation = TransformOperation(newHistoralRequest, currentRequest.Operation);
                }
            }
            transformedRequests.Add(currentRequest.Operation);
        }
        return transformedRequests;
    }

    public Operation TransformOperation(Operation prev, Operation next)
    {
        if (!IsPreviousOperationRelevant(prev, next)) return next;

        var newSC = next.StartColumn;
        var newEC = next.EndColumn;
        var newSL = next.StartLine;
        var newEL = next.EndLine;
        var numberOfNewLinesInPrev = Regex.Split(prev.Text, @"\r\n|\r|\n").Length - 1;
        var prevTextLengthAfterLastNewLine = numberOfNewLinesInPrev > 0 ? prev.Text.Length - prev.Text.LastIndexOf("\n") - 1 : prev.Text.Length;

        var netNewLineNumberChange = numberOfNewLinesInPrev - (prev.EndLine - prev.StartLine);

        if (IsSimpleInsert(prev))
        {
            if (numberOfNewLinesInPrev > 0)
            {
                if (next.StartLine == prev.EndLine)
                {
                    newSC = newSC - prev.EndColumn + prevTextLengthAfterLastNewLine + 1;
                }
                if (next.EndLine == prev.EndLine)
                {
                    newEC = newEC - prev.EndColumn + prevTextLengthAfterLastNewLine + 1;
                }
            }
            else
            {
                if (next.StartLine == prev.EndLine)
                {
                    newSC = newSC + prevTextLengthAfterLastNewLine;
                }
                if (next.EndLine == prev.EndLine)
                {
                    newEC = newEC + prevTextLengthAfterLastNewLine;
                }
            }
        }
        else
        {
            if (numberOfNewLinesInPrev > 0)
            {
                if (next.StartLine == prev.EndLine)
                {
                    newSC = newSC - prev.EndColumn + prevTextLengthAfterLastNewLine + 1; // do i need +1?
                }
                if (next.EndLine == prev.EndLine)
                {
                    newEC = newEC - prev.EndColumn + prevTextLengthAfterLastNewLine + 1;
                }
            }
            else
            {
                var numberOfCharsDeletedOnPrevLine = prev.EndColumn - prev.StartColumn;
                if (next.StartLine == prev.EndLine)
                {
                    newSC = newSC - numberOfCharsDeletedOnPrevLine + prev.Text.Length;
                }
                else
                {
                    newSC = prev.StartColumn + prev.Text.Length;
                }

                if (next.EndLine == prev.EndLine)
                {
                    newEC = newEC - numberOfCharsDeletedOnPrevLine + prev.Text.Length;
                }
                else
                {
                    if (IsSimpleInsert(next))
                    {
                        newEC = newSC;
                    }
                }
            }
        }

        if (IsSCWithinRange(prev, next))
        {
            newSL = prev.StartLine + numberOfNewLinesInPrev;
        }
        else
        {
            newSL += netNewLineNumberChange;
        }

        if (IsECWithinRange(prev, next))
        {
            newSL = prev.StartLine + numberOfNewLinesInPrev;
        }
        else
        {
            newEL += netNewLineNumberChange;
        }

        next.StartColumn = newSC;
        next.EndColumn = newEC;
        next.StartLine = newSL;
        next.EndLine = newEL;
        return next;
    }

    public bool IsSimpleInsert(Operation op)
    {
        return op.EndColumn == op.StartColumn && op.EndLine == op.StartLine;
    }
}