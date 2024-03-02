using System.Globalization;
using server.models;

namespace Monaco.Services.Interfaces;

public interface ITransformService
{
    List<Operation> GetRelevantHistory(int revisionId, Dictionary<int, List<Operation>> history);
    bool IsECWithinRange(Operation prev, Operation next);
    bool IsSCWithinRange(Operation prev, Operation next);
    bool IsPreviousOperationRelevant(Operation prev, Operation next);
    List<Operation> ResolveConflictingRanges(Operation prev, Operation next);
    Operation TransformOperation(Operation prev, Operation next);
    List<Operation> Transform(Operation op, Dictionary<int, List<Operation>> history);
    bool IsSimpleInsert(Operation op);
}