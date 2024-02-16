import { Operation } from "../models/Operation";

// export function transform(
//   op: Operation,
//   history: Map<number, Operation[]>
// ): Operation {}

// Relies on assumption that revisionIds have already been considered
export function isPreviousOperationRelevent(
  prev: Operation,
  next: Operation
): boolean {
  if (prev.originatorId === next.originatorId) return false;

  let isPrevStartLineAfterNextEndLine: boolean = prev.startLine > next.endLine;
  let isSameLine: boolean = prev.startLine === next.endLine;

  if (isPrevStartLineAfterNextEndLine) return false;
  if (isSameLine) {
    if (isInsert(next)) {
      if (next.endColumn < prev.startColumn) return false;
    } else {
      if (next.endColumn <= prev.startColumn) return false;
    }
  }
  return true;
}

// Is next SC within prev range
export function isSCWithinRange(prev: Operation, next: Operation): boolean {
  if (next.startLine > prev.startLine && next.startLine < prev.endLine)
    return true;

  if (next.startLine === prev.startLine) {
    if (next.startLine === prev.endLine) {
      if (!(next.startColumn < prev.endColumn)) return false;
    }
    if (next.startColumn >= prev.startColumn) return true;
  }

  if (next.startLine === prev.endLine && next.startLine !== prev.startLine) {
    if (next.startColumn < prev.endColumn) return true;
  }
  return false;
}

// Is next EC within prev range
export function isECWithinRange(prev: Operation, next: Operation): boolean {
  if (next.endLine < prev.endLine && next.endLine > prev.startLine) return true;

  if (next.endLine === prev.endLine) {
    if (next.endLine === prev.startLine) {
      if (!(next.endColumn > prev.startColumn)) return false;
    }
    if (next.endColumn <= prev.endColumn) return true;
  }

  if (next.endLine === prev.startLine && next.endLine !== prev.endLine) {
    if (next.endColumn > prev.startColumn) return true;
  }

  return false;
}

export function isInsert(op: Operation): boolean {
  return op.endColumn === op.startColumn && op.endLine === op.startLine;
}

export function getRelevantHistory(
  revId: number,
  history: Map<number, Operation[]>
): Operation[] {
  let relevantRequests: Operation[] = [];
  history.forEach((list, id) => {
    if (id > revId) {
      relevantRequests = [...relevantRequests, ...list];
    }
  });
  return relevantRequests;
}

export function transformOperation(
  prev: Operation,
  next: Operation
): Operation {
  if (!isPreviousOperationRelevent(prev, next)) return next;

  let newSC: number = next.startColumn;
  let newEC: number = next.endColumn;
  let newSL: number = next.startLine;
  let newEL: number = next.endLine;
  let numberOfNewLinesInPrev: number = prev.text.split(new RegExp('\r\n|\r|\n')).length - 1;
  let prevTextLengthAfterLastNewLine: number = numberOfNewLinesInPrev > 0 ? prev.text.length - prev.text.lastIndexOf("\n") - 1 : prev.text.length;

  let netNewLineNumberChange: number = numberOfNewLinesInPrev - (prev.endLine - prev.startLine);

  if (isInsert(prev)) {
    if (numberOfNewLinesInPrev > 0) {
      if (next.startLine === prev.endLine) {
        newSC = newSC - prev.endColumn + prevTextLengthAfterLastNewLine + 1;
      }
      if (next.endLine === prev.endLine) {
        newEC = newEC - prev.endColumn + prevTextLengthAfterLastNewLine + 1;
      }
    } else {
      if (next.startLine === prev.endLine) {
        newSC = newSC + prevTextLengthAfterLastNewLine;
      }
      if (next.endLine === prev.endLine) {
        newEC = newEC + prevTextLengthAfterLastNewLine;
      }
    }
  } else {
    if (numberOfNewLinesInPrev > 0) {
      if (next.startLine === prev.endLine) {
        newSC = newSC - prev.endColumn + prevTextLengthAfterLastNewLine + 1; // do i need +1?
      }
      if (next.endLine === prev.endLine) {
        newEC = newEC - prev.endColumn + prevTextLengthAfterLastNewLine + 1;
      }
    } else {
      let numberOfCharsDeletedOnPrevLine: number = prev.endColumn - prev.startColumn;
      if (next.startLine === prev.endLine) {
        newSC = newSC - numberOfCharsDeletedOnPrevLine + prev.text.length;
      } else {
        newSC = prev.startColumn + prev.text.length;
      }

      if (next.endLine === prev.endLine) {
        newEC = newEC - numberOfCharsDeletedOnPrevLine + prev.text.length;
      } else {
        if (isInsert(next)) {
          newEC = newSC;
        }
      }
    }
  }

  if (isSCWithinRange(prev, next)) {
    newSL = prev.startLine + numberOfNewLinesInPrev;
  } else {
    newSL += netNewLineNumberChange;
  }

  if (isECWithinRange(prev, next)) {
    newSL = prev.startLine + numberOfNewLinesInPrev;
  } else {
    newEL += netNewLineNumberChange;
  }

  next.startColumn = newSC;
  next.endColumn = newEC;
  next.startLine = newSL;
  next.endLine = newEL;
  return next;
}
