namespace test;
using Xunit;
using server.models;
using Monaco.Utility;
using System.Text;

public class ModelUtilityTests
{

    public static TheoryData<string, Operation, string> UpdateModelData = new TheoryData<string, Operation, string> {
        { "", Op(1, 1, 1, 1, "abc"), "abc" },
        { "abc", Op(1, 1, 1, 1, "abc"), "abcabc" },
        { "abc", Op(1, 2, 1, 1, "abc"), "abcbc" },
    };

    [Theory(DisplayName = "UpdateModelData"), MemberData(nameof(UpdateModelData))]
    public void UpdateModelTest(string model, Operation op, string expected)
    {
        Assert.Equal(ModelUtility.UpdateModel(model, op), expected);
    }

    [Fact]
    public void UpdateModel_Series()
    {
        var model = "";
        model = ModelUtility.UpdateModel(model, Op(1, 1, 1, 1, "a")); // a
        model = ModelUtility.UpdateModel(model, Op(1, 1, 1, 1, "b")); // ba
        model = ModelUtility.UpdateModel(model, Op(2, 2, 1, 1, "s")); // bsa
        model = ModelUtility.UpdateModel(model, Op(2, 2, 1, 1, "a")); // basa
        Assert.Equal("basa", model);
        model = ModelUtility.UpdateModel(model, Op(2, 3, 1, 1, "1")); // b1sa
        model = ModelUtility.UpdateModel(model, Op(1, 1, 1, 1, "34\n9")); // b1sa
        Assert.Equal("34\n9b1sa", model);
        model = "b1sa";
        model = ModelUtility.UpdateModel(model, Op(1, 1, 1, 1, "34\r\n9"));
        Assert.Equal("34\r\n9b1sa", model);
    }

    public static TheoryData<string, string , int, int, string> ReplaceSubstringData = new TheoryData<string, string , int, int, string> {
        { "abcd", "12", 1, 2, "a12cd" },
    };

    [Theory(DisplayName = "ReplaceSubstringData"), MemberData(nameof(ReplaceSubstringData))]
    public void ReplaceSubstringTest(string model, string incoming, int start, int end, string expected)
    {
        StringBuilder a = new StringBuilder(model);
        Assert.Equal(ModelUtility.ReplaceSubstring(model, incoming, start, end), expected);
    }

    public static TheoryData<string, int, int, int> GetIndexData = new TheoryData<string, int, int, int> {
        { "abcd", 1, 1, 0 },
        { "ab\ncd", 1, 2, 3 },
        { "ab\r\ncd", 1, 2, 3 },
    };

    [Theory(DisplayName = "GetIndexDataData"), MemberData(nameof(GetIndexData))]
    public void GetIndexTest(string model, int col, int line, int expected)
    {
        Assert.Equal(expected, ModelUtility.GetIndex(model, col, line));
    }

    private static Operation Op(int sc, int ec, int sl, int el, string text)
    {
        return new Operation
        {
            Text = text,
            StartColumn = sc,
            EndColumn = ec,
            StartLine = sl,
            EndLine = el
        };
    }
}
