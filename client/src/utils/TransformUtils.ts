import { Queue } from "../models/Queue";
import { Operation } from "../models/Operation";

export function transform(
  op: Operation,
  history: Map<number, Operation[]>
): Operation[] {
  let transformedRequests: Operation[] = [];

  let toTransformQueue: Queue<[Operation, number]> = new Queue();
  toTransformQueue.enqueue([op, -1]);

  // [operation, ]
  let currentRequest: [Operation, number] | undefined;

  while ((currentRequest = toTransformQueue.dequeue()) !== undefined) {
    let relevantHistory: Operation[] = getRelevantHistory(op.revisionId, history);

    for (let i = 0; i < relevantHistory.length; i++) {
      let historicalRequest: Operation = relevantHistory[i];

      if (op.originatorId !== historicalRequest.originatorId) {
        let pair: Operation[] = resolveConflictingRanges(historicalRequest, currentRequest[0]);

        if (currentRequest[1] < i) {
          currentRequest[0] = transformOperation(historicalRequest, pair[0]);
        }

        if (pair[1] != null) {
          toTransformQueue.enqueue([pair[1], i]);
        }
      }
    }

    for (var newHistoralRequest of transformedRequests) {
      if (isPreviousOperationRelevent(newHistoralRequest, currentRequest[0])) {
        currentRequest[0] = transformOperation(newHistoralRequest, currentRequest[0]);
      }
    }
    transformedRequests.push(currentRequest[0]);
  }
  
  return transformedRequests;
}

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
    if (isSimpleInsert(next)) {
      if (next.endColumn < prev.startColumn) return false;
    } else {
      if (next.endColumn <= prev.startColumn) return false;
    }
  }
  return true;
}

/**
 *  Is next SC within prev range
 * */
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

/**
 *  Is next EC within prev range
 * */
export function isECWithinRange(prev: Operation, next: Operation): boolean {
  if (next.endLine < prev.endLine && next.endLine > prev.startLine) return true;

  if (next.endLine === prev.endLine) {
    if (next.endLine === prev.startLine) {
      if (next.endColumn <= prev.startColumn) return false;
    }
    if (next.endColumn < prev.endColumn) return true;
  }

  if (next.endLine === prev.startLine && next.endLine !== prev.endLine) {
    if (next.endColumn > prev.startColumn) return true;
  }

  return false;
}

export function isSimpleInsert(op: Operation): boolean {
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
  let numberOfNewLinesInPrev: number =
    prev.text.split(new RegExp("\r\n|\r|\n")).length - 1;
  let prevTextLengthAfterLastNewLine: number =
    numberOfNewLinesInPrev > 0
      ? prev.text.length - prev.text.lastIndexOf("\n") - 1
      : prev.text.length;

  let netNewLineNumberChange: number =
    numberOfNewLinesInPrev - (prev.endLine - prev.startLine);

  if (isSimpleInsert(prev)) {
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
      let numberOfCharsDeletedOnPrevLine: number =
        prev.endColumn - prev.startColumn;
      if (next.startLine === prev.endLine) {
        newSC = newSC - numberOfCharsDeletedOnPrevLine + prev.text.length;
      } else {
        newSC = prev.startColumn + prev.text.length;
      }

      if (next.endLine === prev.endLine) {
        newEC = newEC - numberOfCharsDeletedOnPrevLine + prev.text.length;
      } else {
        if (isSimpleInsert(next)) {
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

export function resolveConflictingRanges(
  prev: Operation,
  next: Operation
): Operation[] {
  if (isSimpleInsert(prev) || isSimpleInsert(next)) {
    return [next];
  }

  if (isECWithinRange(next, prev) && isSCWithinRange(next, prev)) {
    let otherNext: Operation = {
      text: "",
      originatorId: next.originatorId,
      startColumn: next.startColumn,
      endColumn: next.endColumn,
      startLine: next.startLine,
      endLine: next.endLine,
      revisionId: next.revisionId,
    };

    //shift end of next to start of prev
    next.endColumn = prev.startColumn;
    next.endLine = prev.startLine;

    //shift start of otherNext to end of prev
    otherNext.startColumn = prev.endColumn;
    otherNext.startLine = prev.endLine;
    return [next, otherNext];
  }

  if (isSCWithinRange(next, prev)) {
    next.endLine = prev.startLine;
    next.endColumn = prev.startColumn;
  }

  if (isECWithinRange(next, prev)) {
    next.startLine = prev.endLine;
    next.startColumn = prev.endColumn;
  }

  return [next];
}
