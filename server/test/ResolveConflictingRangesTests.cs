namespace test;
using Xunit;
using Moq;
using server.models;
using Monaco.Services.Interfaces;
using Monaco.Services;

public class ResolveConflinctingRangesTests
{
    private readonly ITransformService ts;

    public ResolveConflinctingRangesTests()
    {
        ts = new TransformService();
    }

    public static TheoryData<Operation, Operation, List<Operation>> ResolveConflinctingRangeData = new TheoryData<Operation, Operation, List<Operation>> {
        { Op(1, 1, 1, 1, "1"), Op(1, 1, 1, 1, "2"), [Op(1, 1, 1, 1, "2")] },
        { Op(1, 4, 1, 1, "1"), Op(2, 6, 1, 1, "2"), [Op(4, 6, 1, 1, "2")] },
        { Op(6, 10, 1, 1, "1"), Op(8, 8, 1, 1, "2"), [Op(8, 8, 1, 1, "2")] },
        { Op(10, 10, 1, 1, "1"), Op(2, 14, 1, 1, "2"), [Op(2, 14, 1, 1, "2")] },
        { Op(6, 11, 1, 1, "1"), Op(2, 14, 1, 1, "2"), [Op(2, 6, 1, 1, "2"), Op(11, 14, 1, 1, "")] },
        { Op(6, 11, 1, 1, "1"), Op(14, 18, 1, 1, "2"), [Op(14, 18, 1, 1, "2")] },
        { Op(6, 11, 1, 1, "1"), Op(7, 18, 2, 2, "2"), [Op(7, 18, 2, 2, "2")] },
        { Op(1, 5, 1, 1, "1"), Op(4, 18, 1, 1, "2"), [Op(5, 18, 1, 1, "2")] },
        { Op(7, 12, 1, 1, "1"), Op(1, 12, 1, 1, "2"), [Op(1, 7, 1, 1, "2")] },
    };

    [Theory(DisplayName = "ResolveConflinctingRanges"), MemberData(nameof(ResolveConflinctingRangeData))]
    public void ResolveConflinctingRangeTests(Operation prev, Operation next, List<Operation> expected)
    {
        Assert.Equivalent(ts.ResolveConflictingRanges(prev, next), expected, true);
    }

    private static Operation Op(int sc, int ec, int sl, int el, string text)
    {
        return new Operation
        {
            StartColumn = sc,
            EndColumn = ec,
            StartLine = sl,
            EndLine = el,
            Text = text,
            RevisionId = -1,
            OriginatorId = "id"
        };
    }
}
