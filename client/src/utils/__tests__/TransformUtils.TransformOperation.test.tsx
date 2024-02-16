import { Operation } from "../../models/Operation";
import { transformOperation } from "../TransformUtils";

type CaseInput = {
  prev: Operation,
  next: Operation,
  expected: Operation
};

const casesSC: CaseInput[] = [
  {prev: op(1, 1, 1, 1, "1", "1"), next: op(1, 1, 1, 1, "2", ""), expected: op(2, 2, 1, 1, "2", "")},
  {prev: op(1, 1, 1, 1, "1a", "2"), next: op(1, 1, 1, 1, "2", ""), expected: op(3, 3, 1, 1, "2", "")},
  {prev: op(1, 1, 1, 1, "1a", "3"), next: op(5, 5, 1, 1, "2", ""), expected: op(7, 7, 1, 1, "2", "")},
  {prev: op(1, 1, 1, 1, "1a", "4"), next: op(5, 5, 3, 3, "2", ""), expected: op(5, 5, 3, 3, "2", "")},
  {prev: op(1, 1, 1, 1, "1a", "5"), next: op(5, 5, 3, 3, "2", ""), expected: op(5, 5, 3, 3, "2", "")},
  {prev: op(2, 2, 1, 1, "1a", "6"), next: op(1, 1, 1, 1, "2", ""), expected: op(1, 1, 1, 1, "2", "")},
  {prev: op(1, 1, 1, 1, "34\n", "7"), next: op(1, 1, 1, 1, "2", ""), expected: op(1, 1, 2, 2, "2", "")},
  {prev: op(1, 1, 1, 1, "34\n234", "8"), next: op(1, 1, 1, 1, "2", ""), expected: op(4, 4, 2, 2, "2", "")},
  {prev: op(1, 1, 1, 1, "34\n234", "9"), next: op(1, 1, 1, 1, "2", ""), expected: op(4, 4, 2, 2, "2", "")},
  {prev: op(1, 1, 1, 1, "34\n234\n2", "10"), next: op(1, 1, 1, 1, "2", ""), expected: op(2, 2, 3, 3, "2", "")},
  {prev: op(2, 2, 1, 1, "34\n234\n\n2", "11"), next: op(1, 1, 1, 1, "2", ""), expected: op(1, 1, 1, 1, "2", "")},
  {prev: op(1, 1, 1, 1, "34\n234\n\n2", "12"), next: op(4, 4, 4, 8, "2", ""), expected: op(4, 4, 7, 11, "2", "")},
  {prev: op(1, 1, 1, 1, "34\r\n234\r\n\r\n2", "13"), next: op(4, 4, 4, 8, "2", ""), expected: op(4, 4, 7, 11, "2", "")},
  {prev: op(1, 1, 1, 3, "34\r\n234\r\n\r\n2", "14"), next: op(4, 4, 4, 8, "2", ""), expected: op(4, 4, 5, 9, "2", "")},
];

describe("transformOperation", () => {
  test.each(casesSC)('case $prev.originatorId', ({prev, next, expected}) => {
    expect(transformOperation(prev, next)).toStrictEqual(expected);
  });
});

function op(
  startColumn: number,
  endColumn: number,
  startLine: number,
  endLine: number,
  text: string,
  originatorId: string
): Operation {
  return {
    text: text,
    endColumn: endColumn,
    startColumn: startColumn,
    endLine: endLine,
    startLine: startLine,
    originatorId: originatorId
  };
}
