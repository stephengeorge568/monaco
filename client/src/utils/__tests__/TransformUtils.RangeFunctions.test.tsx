import { Operation } from "../../models/Operation";
import { isECWithinRange, isSCWithinRange } from "../TransformUtils";

type CaseInput = {
  prev: Operation,
  next: Operation,
  expected: boolean
};

const casesSC: CaseInput[] = [
  {prev: op(1, 3, 1, 1, "1"), next: op(1, 1, 1, 1, ""), expected: true},
  {prev: op(1, 3, 1, 3, "2"), next: op(2, 2, 3, 3, ""), expected: true},
  {prev: op(1, 2, 1, 2, "3"), next: op(4, 6, 1, 2, ""), expected: true},
  {prev: op(1, 2, 1, 3, "4"), next: op(4, 6, 1, 2, ""), expected: true},

  {prev: op(1, 1, 1, 1, "5"), next: op(1, 1, 1, 1, ""), expected: false},
  {prev: op(1, 1, 1, 1, "6"), next: op(2, 2, 1, 1, ""), expected: false},
  {prev: op(1, 3, 1, 1, "7"), next: op(1, 1, 4, 4, ""), expected: false},
  {prev: op(1, 3, 1, 1, "8"), next: op(3, 3, 1, 1, ""), expected: false},
];

describe("isSCWithinRange", () => {
  test.each(casesSC)('given case $prev.originatorId, it should return $expected', ({prev, next, expected}) => {
    expect(isSCWithinRange(prev, next)).toBe(expected);
  });
});

const casesEC: CaseInput[] = [
  {prev: op(2, 5, 1, 1, "1"), next: op(2, 4, 1, 1, ""), expected: true},
  {prev: op(4, 8, 1, 2, "2"), next: op(1, 5, 1, 2, ""), expected: true},

  {prev: op(1, 1, 1, 1, "3"), next: op(1, 1, 1, 1, ""), expected: false},
  {prev: op(4, 8, 1, 1, "4"), next: op(1, 4, 1, 1, ""), expected: false},
];

describe("isECWithinRange", () => {
  test.each(casesEC)('given case $prev.originatorId, it should return $expected', ({prev, next, expected}) => {
    expect(isECWithinRange(prev, next)).toBe(expected);
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
