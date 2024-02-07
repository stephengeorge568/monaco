import { Operation } from "../../models/Operation";
import { isPreviousOperationRelevent } from "../TransformUtils";

type CaseInput = {prev: Operation, next: Operation, expected: boolean};

const cases: CaseInput[] = [
  {prev: op(1, 1, 1, 1, "1"), next: op(1, 1, 1, 1, ""), expected: true},
  {prev: op(1, 1, 1, 1, "2"), next: op(2, 2, 1, 1, ""), expected: true},
  {prev: op(1, 1, 2, 2, "3"), next: op(1, 4, 1, 2, ""), expected: true},
  {prev: op(1, 3, 1, 2, "4"), next: op(2, 4, 2, 2, ""), expected: true},
  {prev: op(2, 2, 1, 1, "5"), next: op(9, 12, 1, 4, ""), expected: true},
  {prev: op(3, 3, 1, 1, "6"), next: op(2, 2, 1, 1, ""), expected: false},
  {prev: op(1, 1, 6, 9, "7"), next: op(2, 2, 2, 5, ""), expected: false},
  {prev: op(3, 3, 3, 3, "8"), next: op(1, 3, 3, 3, ""), expected: false},
];

describe("isPreviousOperationRelevent", () => {
  test.each(cases)('given case $prev.originatorId, it should return $expected', ({prev, next, expected}) => {
    expect(isPreviousOperationRelevent(prev, next)).toBe(expected);
  });
});

function op(
  startColumn: number,
  endColumn: number,
  startLine: number,
  endLine: number,
  originatorId: string,
): Operation {
  return {
    text: "irrelevant",
    endColumn: endColumn,
    startColumn: startColumn,
    endLine: endLine,
    startLine: startLine,
    originatorId: originatorId,
  };
}
