namespace test;
using Xunit;
using server.models;
using Monaco.Utility;

public class TransformTests
{
    public static TheoryData<Operation, Dictionary<int, List<Operation>>, List<Operation>> ResolveConflinctingRangeData = new TheoryData<Operation, Dictionary<int, List<Operation>>, List<Operation>> {
        {
            Op(1, 1, 1, 1, "1", 1, "1"), // "" - now revisionId 1
            new Dictionary<int, List<Operation>> {
                {0, [ Op(1, 1, 1, 1, "xxxxxxxxx", 0, "2") ]},
                {2, [ Op(1, 1, 1, 1, "cases", 2, "2") ]},
                {3, [ Op(5, 5, 1, 1, "B", 3, "2") ]},
                {4, [ Op(6, 6, 1, 1, "Q", 4, "2") ]},
            },
            [Op(8, 8, 1, 1, "1", 1, "1")] //"casesBQreso" - now revisionId 4
        },
        {
            Op(1, 12, 1, 1, "", 1, "2"), // "Lorem ipsum" - now revisionId 0
            new Dictionary<int, List<Operation>> {
                {2, [ Op(7, 12, 1, 1, "", 2, "3") ]}, // "Lorem "
                {3, [ Op(7, 7, 1, 1, "X", 3, "3") ]}, // "Lorem X"
            },
            [Op(1, 7, 1, 1, "", 1, "2")] //"X" - now revisionId 3
        },
        {
            Op(2, 2, 1, 1, "a", 0, "z"), //
            new Dictionary<int, List<Operation>> {
                {1, [  Op(1, 1, 1, 1, "b", 1, "z") ]}, // 
            },
            [Op(2, 2, 1, 1, "a", 0, "z")] //
        },
    };

    [Theory(DisplayName = "Transform"), MemberData(nameof(ResolveConflinctingRangeData))]
    public void TransformTest(Operation prev, Dictionary<int, List<Operation>> history, List<Operation> expected)
    {
        var x = Transformer.Transform(prev, history); 
        Assert.Equivalent(x, expected, true);
    }

    private static Operation Op(int sc, int ec, int sl, int el, string text, int revisionId, string originiatorId)
    {
        return new Operation
        {
            Id = "Id_Not_Relevant",
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
