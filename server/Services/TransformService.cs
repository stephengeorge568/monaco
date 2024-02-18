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
        throw new NotImplementedException();
    }

    public bool IsPreviousOperationRelevant(Operation prev, Operation next)
    {
        throw new NotImplementedException();
    }

    public bool IsSCWithinRange(Operation prev, Operation next)
    {
        throw new NotImplementedException();
    }

    public bool IsSimpleInsert(Operation op)
    {
        throw new NotImplementedException();
    }

    public List<Operation>ResolveConflictingRanges(Operation prev, Operation next)
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