import Editor, { Monaco } from "@monaco-editor/react";
import * as monaco from "monaco-editor/esm/vs/editor/editor.api";
import Connector from "../../utils/SignalrConnection";
import { useCallback, useEffect, useRef } from "react";
import { Operation, copy, fromEvent } from "../../models/Operation";
import { transform } from "src/utils/TransformUtils";
import { Queue } from "src/models/Queue";

interface CodeProps {
  documentId: string;
}

export default function Code({documentId}: CodeProps) {
  const webSocket = Connector();
  const isProgrammaticChange = useRef<boolean>(false);
  const editorRef = useRef<monaco.editor.IStandaloneCodeEditor | null>(null);
  const historyRef = useRef<Map<number, Operation[]>>(new Map());
  const revisionId = useRef<number>(0);
  const outgoingOperations = useRef<Queue<Operation>>(new Queue());
  const isSendingOperation = useRef<boolean>(false);

  const sendNextOperation = useCallback(() => {
    if (!isSendingOperation.current) {
      let op: Operation | undefined = outgoingOperations.current.dequeue();
      if (op) {
        isSendingOperation.current = true;
        let transformedOp = transform(op, historyRef.current);
        transformedOp[0].revisionId = revisionId.current;
        // TODO finalize 2 ops
        webSocket.newOperation(transformedOp[0], documentId);
      }
    }
  }, [documentId, webSocket]);

  const onOperationTransformedAck = useCallback(
    (newRevisionId: number) => {
      revisionId.current = newRevisionId;
      isSendingOperation.current = false;
      sendNextOperation();
    },
    [sendNextOperation]
  );

  const onOperationReceived = useCallback(
    (op: Operation) => {
      if (op.originatorId !== webSocket.connection.connectionId) {
        let transformedOps: Operation[] = transform(op, historyRef.current);
        isProgrammaticChange.current = true;
        for (var change of transformedOps) {
          editorRef.current?.executeEdits(
            "server",
            [
              {
                range: {
                  startLineNumber: change.startLine,
                  endLineNumber: change.endLine,
                  startColumn: change.startColumn,
                  endColumn: change.endColumn,
                },
                text: change.text,
              },
            ]
          );

          if (revisionId.current < op.revisionId) {
            revisionId.current = op.revisionId;
          }

          let newHistory: Map<number, Operation[]> = historyRef.current;
          if (!newHistory.has(op.revisionId)) {
            newHistory.set(op.revisionId, []);
          }
          newHistory.get(op.revisionId)?.push(op);
          historyRef.current = newHistory;
        }
        isProgrammaticChange.current = false;
      }
    },
    [webSocket.connection.connectionId]
  );

  useEffect(() => {
    webSocket.events(onOperationReceived, onOperationTransformedAck);
  }, [
    webSocket,
    sendNextOperation,
    onOperationReceived,
    onOperationTransformedAck,
  ]);

  function onModelChange(
    value: string | undefined,
    event: monaco.editor.IModelContentChangedEvent
  ) {
    if (!isProgrammaticChange.current) {
      let op: Operation = fromEvent(
        event,
        webSocket.connection.connectionId ?? "",
        revisionId.current
      );

      let newHistory: Map<number, Operation[]> = historyRef.current;

      if (!newHistory.has(op.revisionId)) {
        newHistory.set(op.revisionId, []);
      }
      newHistory.get(op.revisionId)?.push(copy(op));
      historyRef.current = newHistory;
      outgoingOperations.current.enqueue(op);
      sendNextOperation();
    }
  }

  function handleEditorDidMount(
    editor: monaco.editor.IStandaloneCodeEditor | null,
    monaco: Monaco
  ) {
    editor?.getModel()?.setEOL(monaco.editor.EndOfLineSequence.LF);
    editor?.updateOptions({ quickSuggestions: false });
    webSocket.addToGroup(documentId);
    editorRef.current = editor;
  }

  return (
    // https://www.npmjs.com/package/@monaco-editor/react
    <>
      <Editor
        height="100vh"
        defaultLanguage="typescript"
        onChange={onModelChange}
        onMount={handleEditorDidMount}
        theme="vs-dark"
      />
    </>
  );
}
