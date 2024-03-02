namespace test;
using Xunit;
using Moq;
using server.models;
using Monaco.Services.Interfaces;
using Monaco.Services;

public class TransformTests
{
    private readonly ITransformService ts;

    public TransformTests()
    {
        ts = new TransformService();
    }

    public static TheoryData<Operation, Dictionary<int, List<Operation>>, List<Operation>> ResolveConflinctingRangeData = new TheoryData<Operation, Dictionary<int, List<Operation>>, List<Operation>> {
        {
            Op(1, 1, 1, 1, "1", 1, "1"), // "" - now revisionId 1
            new Dictionary<int, List<Operation>> {
                {0, [ Op(1, 1, 1, 1, "xxxxxxxxx", 0, "2") ]},
                {1, [ Op(1, 1, 1, 1, "cases", 1, "2") ]},
                {2, [ Op(5, 5, 1, 1, "B", 2, "2") ]},
                {3, [ Op(6, 6, 1, 1, "Q", 3, "2") ]},
            },
            [Op(8, 8, 1, 1, "1", 1, "1")] //"casesBQreso" - now revisionId 5
        },
        {
            Op(1, 12, 1, 1, "", 0, "2"), // "Lorem ipsum" - now revisionId 0
            new Dictionary<int, List<Operation>> {
                {1, [ Op(7, 12, 1, 1, "", 0, "3") ]}, // "Lorem "
                {2, [ Op(7, 7, 1, 1, "X", 1, "3") ]}, // "Lorem X"
            },
            [Op(1, 7, 1, 1, "", 1, "2")] //"X" - now revisionId 3
        },
    };

    [Theory(DisplayName = "Transform"), MemberData(nameof(ResolveConflinctingRangeData))]
    public void TransformTest(Operation prev, Dictionary<int, List<Operation>> history, List<Operation> expected)
    {
        Assert.Equivalent(ts.Transform(prev, history), expected, true);
    }

    private static Operation Op(int sc, int ec, int sl, int el, string text, int revisionId, string originiatorId)
    {
        return new Operation
        {
            StartColumn = sc,
            EndColumn = ec,
            StartLine = sl,
            EndLine = el,
            Text = text,
            RevisionId = revisionId,
            OriginatorId = originiatorId
        };
    }
}
